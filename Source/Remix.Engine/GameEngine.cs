namespace Atlana.Engine
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using Atlana;
    using Atlana.Configuration;
    using Atlana.Creation;
    using Atlana.Data;
    using Atlana.Help;
    using Atlana.Interpret;
    using Atlana.Log;
    using Atlana.Network;
    using Atlana.World;

    /// <summary>
    /// Description of GameEngine.
    /// </summary>
    public sealed class GameEngine
    {
        private static GameEngine instance = new GameEngine();

        public static GameEngine Instance
        {
            get
            {
                return instance;
            }
        }

        public bool Running
        {
            get
            {
                return SettingsManager.MudRunning;
            }
        }

        private Communicator LocalComm;
        private Randomizer LocalRand;
        private Interpreter LocalInterpreter;
        private CreationManager LocalCM;
        private AreaManager LocalAM;

        private GameEngine()
        {
            SettingsManager.MudRunning = true;
        }

        public bool Initialize()
        {
            using (var context = new DataContext())
            {
                Logger.Info("Initializing Random Number Generator...");
                this.LocalRand = Randomizer.Instance;

                Logger.Info("Initializing Interpreter...");
                this.LocalInterpreter = Interpreter.Instance;

                Logger.Info("Initializing Creation Manager...");
                this.LocalCM = CreationManager.Instance;

                Logger.Info("Initializing Role Manager...");
                RoleManager.Current.LoadRoles();

                Logger.Info("Initializing Area Manager...");
                this.LocalAM = AreaManager.Instance;

                Logger.Info("Initializing Limbo...");
                this.LocalAM.InitializeLimbo();

                //load database info
                Logger.Info("-=[Loading Game Database]=-");

                Logger.Info("Loading Creation Options...");
                if (!this.LocalCM.LoadOptions())
                {
                    Logger.Bug("Error Loading Creation Options");
                    return false;
                }
                else
                {
                    Logger.Info("Creation Loaded: Options({0})", this.LocalCM.Count);
                }

                Logger.Info("Loading Help Articles...");
                if (!HelpManager.Instance.LoadArticles())
                {
                    Logger.Bug("Error Loading Help Articles");
                    return false;
                }
                else
                {
                    Logger.Info("Help Loaded: Catalogs({0}) Articles({1})", HelpManager.Instance.CatalogCount, HelpManager.Instance.ArticleCount);
                }

                Logger.Info("Loading Interpreter...");
                if (!this.LocalInterpreter.LoadCommands(context))
                {
                    Logger.Bug("Error Loading Interpreter");
                    return false;
                }
                else
                {
                    Logger.Info("Interpreter Loaded: Commands({0})", this.LocalInterpreter.Count);
                }

                Logger.Info("Loading Areas...");
                if (!this.LocalAM.LoadAreas(context))
                {
                    Logger.Bug("Error Loading Areas");
                    return false;
                }
                else
                {
                    Logger.Info("Totals: Areas({0}) Rooms({1})", this.LocalAM.Areas.Count, this.LocalAM.RoomCount);
                    Logger.Info("=========================================");
                    Logger.Info("{0,-15}|", "Name");
                    Logger.Info("=========================================");
                    foreach (Area a in this.LocalAM.Areas)
                    {
                        Atlana.Log.Logger.Info("{0,-15}", a.Name.ToUpperInvariant());
                    }

                    Logger.Info(String.Empty);
                }

                //Logger.Info("Linking Rooms...");
                //if (!this.LocalAM.LinkAll())
                //{
                //    Logger.Bug("Error Linking Rooms");
                //    return false;
                //}
                //else
                //{
                //    Logger.Info("Rooms Successfully Linked");
                //}
            }

            Logger.Info("Database Loaded!");
            Logger.Info("Initializing Server...");
            this.LocalComm = Communicator.Instance;
            this.LocalComm.ServerError += this.ServerError;
            Logger.Comm("Setting Port to {0}", SettingsManager.Port);
            this.LocalComm.Port = SettingsManager.Port;
            this.LocalComm.MaxBackLog = SettingsManager.MaxBacklogSize;
            if (!this.LocalComm.Initialize())
            {
                Atlana.Log.Logger.Bug("Communicator.Initialize: error initializing server");
                return false;
            }

            this.LocalComm.ClientAccepted += this.ClientAccepted;
            this.LocalComm.AcceptingSocket += this.AcceptingSocket;
            this.LocalComm.ClientDisconnect += this.ClientDisconnect;
            if (!this.LocalComm.Bind())
            {
                Logger.Bug("Communicator.Bind: error binding server");
                return false;
            }

            if (!this.LocalComm.Listen())
            {
                Logger.Bug("Communicator.Listen: error setting server to listen");
            }
            else
            {
                Logger.Comm("Server Up!");
            }

            return true;
        }

        public void ServerError(object sender, EventArgs e)
        {
            Logger.Bug((string)sender);
        }

        public void AcceptingSocket(Socket s)
        {
            Logger.Comm("Client @ [{0}] Connecting...", ((IPEndPoint)s.RemoteEndPoint).Address.ToString());
        }

        public void ClientDisconnect(ClientDescriptor d)
        {
            Logger.Comm("Client @ [{0}] Disconnected.", d.IPAddress);
        }

        public void ClientAccepted(ClientDescriptor d)
        {
            Logger.Comm("Client @ [{0}] Connected.", d.IPAddress);
            d.WriteToBuffer(HelpManager.Instance.GetHelp("greeting0").Text);
        }

        public bool Run()
        {
            bool ok = true;
            SettingsManager.StartTime = DateTime.Now;
            if (SettingsManager.UseAutoRestart)
            {
                Logger.Info("Auto-Restart Time Set: {0} {1}", SettingsManager.RestartTime.ToLongTimeString(), SettingsManager.RestartTime.ToLongDateString());
            }
            while (this.Running)
            {

                if (SettingsManager.UseAutoRestart && SettingsManager.RestartTime <= DateTime.Now)
                {
                    SettingsManager.MudRunning = false;
                    Logger.Info("Auto-Restart Process Started");
                }

                if (!this.LocalComm.AcceptNew())
                {
                    Logger.Bug("Communicator.AcceptNew: error while performing action");
                    ok = false;
                }

                if (this.LocalComm.Count > 0)
                {
                    if (!this.LocalComm.ReadData())
                    {
                        Atlana.Log.Logger.Bug("Communicator.ReadData: error reading data");
                        ok = false;
                    }

                    if (!this.LocalComm.ProcessInput(this.ProcessInput))
                    {
                        Atlana.Log.Logger.Bug("Communicator.ProcessInput: error processing input");
                        ok = false;
                    }

                    if (!this.LocalComm.WriteData())
                    {
                        Atlana.Log.Logger.Bug("Communicator.WriteData: error writing data");
                        ok = false;
                    }
                }

                Thread.Sleep(100);
                this.LocalComm.KillDead();
                GC.Collect();
            }
            return ok;
        }

        public bool Shutdown()
        {
            bool ok = true;
            if (this.LocalComm != null)
            {
                ok = !this.LocalComm.KillAll();
                this.LocalComm.Shutdown();
            }

            return ok;
        }

        public void ProcessInput(Atlana.Network.ClientDescriptor d)
        {
            if (d == null || d.PreviewCommand == null || d.HasError || d.IsClosing)
                return;
            if (d.State < Atlana.Network.DescriptorStates.Playing)
                Nanny(d);
            else
            {
                //if (d.Player != null && d.Player.Mobile != null && d.Player.Mobile.Room != null && this.LocalAM.Limbo != null)
                //{
                //    this.LocalAM.Limbo.MobToRoom(d.Player.Mobile);
                //}

                if (d.Player == null)
                {
                    Logger.Bug("GameEngine.ProcessInput: no player data associated with client@{0}", d.IPAddress);
                    d.WriteToBuffer("Error with your player's memory, disconnecting for safety reasons\r\n");
                    d.IsClosing = true;
                    return;
                }

                if (d.Player.Mobile == null)
                {
                    Logger.Bug("GameEngine.ProcessInput: no mobile data associated with client@{0}", d.IPAddress);
                    d.WriteToBuffer("Error with your player's memory, disconnecting for safety reasons\n\r");
                    d.IsClosing = true;
                    return;
                }

                if (d.Player != null && d.Player.Mobile != null && d.Player.Mobile.Room == null)
                {
                    //d.Player.Mobile.Room = this.LocalAM.Limbo;
                    d.Player.Mobile.Room = this.LocalAM.Areas.SelectMany(a => a.Rooms).FirstOrDefault(r => r.Id > 0) ?? this.LocalAM.Limbo;
                }

                switch (d.State)
                {
                    case DescriptorStates.Playing:
                        this.LocalInterpreter.Interpret(d.Player.Mobile, d.Command);
                        break;
                }
            }
        }

        public void Nanny(ClientDescriptor d)
        {
            if (d == null)
            {
                return;
            }

            string arg = d.Command;
            if (arg == null)
            {
                return;
            }

            switch (d.State)
            {
                default:
                    this.LocalCM.Process(d, arg);
                    return;

                case DescriptorStates.AskName:
                    d.Player = new Player();
                    if (arg == "new")
                    {
                        d.State = DescriptorStates.AskNewName;
                        this.LocalCM.PromptOption(d);
                        return;
                    }

                    if (!d.Player.Preload(arg))
                    {
                        d.Player.WriteLine("Player file not found. Goodbye!");
                        d.State = DescriptorStates.Closing;
                        return;
                    }

                    Logger.Info("Preloaded Player: {0}", arg);
                    d.Player.Write("Password:");
                    d.State = DescriptorStates.AskPassword;
                    return;

                case DescriptorStates.AskPassword:
                    if (d.Player == null)
                    {
                        Logger.Bug("Nanny: Descriptor.Player == NULL");
                        d.WriteToBuffer("Error detected in your memory data. Disconnecting for safety reasons.");
                        d.State = DescriptorStates.Closing;
                        return;
                    }

                    if (d.Player.Password != Convert.ToBase64String(Encoding.ASCII.GetBytes(arg)))
                    {
                        d.Player.WriteLine("Password incorrect. Goodbye!");
                        d.State = DescriptorStates.Closing;
                        return;
                    }

                    if (!d.Player.Load())
                    {
                        Logger.Bug("Nanny: Player.Load Failed: {0}", d.Player.Name);
                        d.Player.WriteLine("Error loading player file. Goodbye!");
                        d.State = DescriptorStates.Closing;
                        return;
                    }

                    Logger.Info("{0} Joined the Game", d.Player.Name);
                    d.Player.WriteLine("Press Enter...");
                    d.State = DescriptorStates.PressEnter;
                    return;

                case DescriptorStates.PressEnter:
                    d.State = DescriptorStates.Playing;
                    if (d.Player.Mobile == null)
                    {
                        d.Player.Mobile = new Mobile();
                    }

                    d.Player.Write(HelpManager.Instance.GetHelp("motd").Text);
                    return;
            }
        }
    }
}

namespace Atlana.Interpret
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Atlana.Data;
    using Atlana.Log;
    using Atlana.Configuration;
    
    /// <summary>
    /// Description of Interpreter.
    /// </summary>
    public sealed class Interpreter
    {
        private static Interpreter _instance;

        public static Interpreter Instance
        {
            get
            {
                if (Interpreter._instance == null)
                {
                    Interpreter._instance = new Interpreter();
                }

                return Interpreter._instance;
            }
        }

        internal static ICommandExecuter MakeScriptHandler(Command c)
        {
            return new ScriptCommandExecuter(c.Script);
        }
        
        private List<Command> commands;
        
        private Interpreter()
        {
            this.commands = new List<Command>();
        }

        public IQueryable<Command> Commands
        {
            get
            {
                return this.commands.AsQueryable();
            }
        }

        public int Count
        {
            get
            {
                return this.commands.Count;
            }
        }

        public bool LoadCommands(DataContext context)
        {
            var noCmd = new NoCommandExecuter();
            bool ok = true;
            try
            {
                using (var cmdRepo = new CommandRepository(context, true))
                {
                    var cmds = cmdRepo.All().ToList();
                    foreach (Command cmd in cmds)
                    {
                        if (String.IsNullOrWhiteSpace(cmd.Script))
                        {
                            cmd.ExecutionHandler = noCmd;
                        }
                        else
                        {
                            cmd.ExecutionHandler = Interpreter.MakeScriptHandler(cmd);
                        }
                        
                        cmd.Name = cmd.Name.ToLowerInvariant();
                        List<Role> roles = new List<Role>();
                        foreach (var r in cmd.Roles)
                        {
                            roles.Add(RoleManager.Current.GetRole(r.Id));
                        }

                        cmd.Roles = roles;
                        this.commands.Add(cmd);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Bug("Interpreter.LoadCommands: {0}", e.Message);
                ok = false;
            }

            return ok;
        }

        public void Interpret(Mobile m, string cmd)
        {
            if (String.IsNullOrEmpty(cmd))
            {
                m.WriteLine(String.Empty);
                return;
            }

            string command = Strings.OneArgument(ref cmd).ToLower();
            Command c = this.Commands.FirstOrDefault(cc => !cc.IsDisabled && cc.Name.StartsWith(command) && cc.CanExecute(m));
            if (c != null)
            {
                switch (c.ExecutionHandler.Do(m, cmd))
                {
                    default:
                        return;

                    case CommandReturn.Ignore:
                        m.WriteLine("Huh?");
                        return;

                    case CommandReturn.ShowExample:
                        m.WriteLine(string.Format("example: {0}", c.ExampleUsage.Replace("$command", c.Name).Replace("\\n", "\n").Replace("\\r", "\r")));
                        return;

                    case CommandReturn.Error:
                        m.WriteLine("There was an error executing the command.");
                        return;
                }
            }

            m.WriteLine("Huh?");
        }
    }
}

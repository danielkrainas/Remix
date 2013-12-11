// <copyright file="Communicator.cs" company="Atlana Team">
// *****************************************************
// *        __  __  _  _  __  __  __  __  ____         *
// *       (  \/  )( \/ )(  \/  )(  )(  )(  _ \        *
// *        )    (  \  /  )    (  )(__)(  )(_) )       *
// *       (_/\/\_) (__) (_/\/\_)(______)(____/        *
// *                                                   *
// * Atlana(c) 2009 - Daniel Krainas                    *
// *****************************************************
// </copyright>
// <author>Daniel Krainas</author>
// <email>dkrainas87@gmail.com</email>
// <date>6-15-2009</date>
// <summary>
// Contains delegates for the ClientDescriptor and Communicator classes
// and contains the Communicator class.
// </summary>
namespace Atlana.Network
{
    using System;
    using System.Collections.Generic;    
    using System.Net;
    using System.Net.Sockets;
    
    /// <summary>
    /// Event delegate for any Socket events.
    /// </summary>
    public delegate void SocketEventHandler(Socket s);
    
    /// <summary>
    /// Event delegate for any ClientDescriptor events.
    /// </summary>
    public delegate void ClientEventHandler(ClientDescriptor d);    
    
    /// <summary>
    /// Event delegate for any ClientDescriptor data events.
    /// </summary>
    public delegate void ClientDataEventHandler(ClientDescriptor d, ref byte[] data, ref int size);
    
    /// <summary>
    /// Description of Communicator.
    /// </summary>
    public sealed class Communicator
    {
        private List<ClientDescriptor> Clients;
        
        public event SocketEventHandler AcceptingSocket;
        
        public event ClientEventHandler ClientClosed;
        public event ClientEventHandler ClientError;
        public event ClientEventHandler ClientAccepted;        
        public event ClientEventHandler ClientDisconnect;
        
        public event EventHandler ShuttingDown;
        public event EventHandler ServerError;
        
        public event ClientDataEventHandler ClientRecvData;
        public event ClientDataEventHandler ClientSendData;
        public event ClientDataEventHandler ClientBuffering;
        
        public event EventHandler ClientRecvingData;
        public event EventHandler ClientSendingData;

        
        private static Communicator instance = new Communicator();
        public static Communicator Instance {
            get {
                return instance;
            }
        }
        
        public ClientDescriptor this[int i]
        {
            get{
                return this.Clients[i];
            }
        }
        
        public int Count
        {
            get{
                return this.Clients.Count;
            }
        }
        
        private Socket MudSocket;
        
        private int port;
        private int maxbacklog;
        
        public int Port
        {
            get{
                return this.port;
            }set{
                if(this.MudSocket!=null&&this.MudSocket.IsBound)
                    return;
                this.port=value;
            }
        }
        
        public int MaxBackLog
        {
            get{
                return this.maxbacklog;
            }set{
                if(this.MudSocket!=null&&this.MudSocket.IsBound)
                    return;
                this.maxbacklog=value;
            }
        }
        
        private Communicator()
        {
            this.Clients=new List<ClientDescriptor>();
        }
        
        private void OnClientDisconnect(ClientDescriptor d)
        {
            if(this.ClientDisconnect!=null)
                this.ClientDisconnect(d);
        }
        
        private void OnClientRecvData(ClientDescriptor d,ref byte[] buf,ref int size)
        {
            if(this.ClientRecvData!=null)
                this.ClientRecvData(d,ref buf,ref size);
        }
        
        private void OnClientSendData(ClientDescriptor d,ref byte[] buf,ref int size)
        {
            if(this.ClientSendData!=null)
                this.ClientSendData(d,ref buf,ref size);
        }
        
        private void OnClientBuffering(ClientDescriptor d,ref byte[] buf,ref int size)
        {
            if(this.ClientBuffering!=null)
                this.ClientBuffering(d,ref buf,ref size);
        }
        
        private void OnClientRecvingData(object sender,EventArgs e)
        {
            if(this.ClientRecvingData!=null)
                this.ClientRecvingData(sender,e);
        }
        
        private void OnClientSendingData(object sender,EventArgs e)
        {
            if(this.ClientSendingData!=null)
                this.ClientSendingData(sender,e);
        }
        
        private void OnClientError(ClientDescriptor d,string err)
        {
            if(d!=null)
                ServerError(string.Format("Client error @ [{0}]:{1}",d.IPAddress,err),EventArgs.Empty);
            if(this.ClientError!=null)
                this.ClientError(d);
        }
        
        private void OnShuttingDown()
        {
            if(this.ShuttingDown!=null)
                this.ShuttingDown(this,EventArgs.Empty);
        }
        
        private void OnClientClosed(ClientDescriptor d)
        {
            if(this.ClientClosed!=null)
                this.ClientClosed(d);
        }
        
        private void OnServerError(string error)
        {
            if(this.ServerError!=null)
                this.ServerError(error,EventArgs.Empty);
        }
        
        private void OnClientAccepted(ClientDescriptor d)
        {
            if(this.ClientAccepted!=null)
                this.ClientAccepted(d);
        }
        
        private void OnAcceptingSocket(Socket s)
        {
            if(this.AcceptingSocket!=null)
                this.AcceptingSocket(s);
        }
        
        public bool Initialize()
        {
            try
            {
                this.MudSocket=new Socket( AddressFamily.InterNetwork , SocketType.Stream,ProtocolType.IP );
                this.MudSocket.Blocking=false;
            }catch(Exception e)
            {
                OnServerError(e.Message);
                return false;
            }
            return true;
        }
        
        public bool Bind()
        {
            try{
                this.MudSocket.Bind( new IPEndPoint( System.Net.IPAddress.Any , this.Port ) );
            }catch(Exception e)
            {
                OnServerError("Communicator.Bind:"+e.Message);
                return false;
            }
            return true;
        }
        
        public bool Listen()
        {
            try{
                this.MudSocket.Listen(this.MaxBackLog);
            }catch(Exception e)
            {
                OnServerError("Communicator.Listen:"+e.Message);
                return false;
            }
            return true;
        }
        
        public void Shutdown()
        {
            try
            {
                if(!this.MudSocket.IsBound&&this.Count<=0)
                    return;
                foreach(ClientDescriptor d in this.Clients)
                {
                    try
                    {
                        d.Close();
                    }catch(Exception e)
                    {
                        OnServerError("ClientDescriptor.Close:"+e.Message);
                    }
                }
                this.MudSocket.Close();
            }catch(Exception e)
            {
                OnServerError("Communicator.Shutdown:"+e.Message);
            }
        }
        
        private void InitializeClient(ClientDescriptor d)
        {
            d.RecvData+= this.OnClientRecvData;
            d.SendData+= this.OnClientSendData;
            d.BufferOut+= this.OnClientBuffering;
            d.SendingData+= this.OnClientSendingData;
            d.RecvingData+= this.OnClientRecvingData;
        }
        
        private void DisposeClient(ClientDescriptor d)
        {
            d.RecvData-= this.OnClientRecvData;
            d.SendData-= this.OnClientSendData;
            d.BufferOut-= this.OnClientBuffering;
            d.SendingData-= this.OnClientSendingData;
            d.RecvingData-= this.OnClientRecvingData;
        }
        
        public bool AcceptNew()
        {
//            try{
                if(this.MudSocket.Poll(0, SelectMode.SelectRead))
                {
                    Socket s=this.MudSocket.Accept();
                    OnAcceptingSocket(s);
                    ClientDescriptor d=new ClientDescriptor(s);
                    InitializeClient(d);
                    Clients.Add(d);
                    OnClientAccepted(d);
                }
//            }catch(Exception e)
//            {
//                OnServerError("Communicator.AcceptNew:"+e.Message);
//                return false;
//            }
            return true;
        }
        public bool ReadData()
        {
            bool ok=true;
            foreach(ClientDescriptor d in this.Clients)
            {
                if(!d.HasError&&!d.IsClosing&&d.ReadReady)
                {
                    try{
                        if(!d.ReadFromSocket())
                        {
                            OnClientError(d,"ReadFromSocket: error reading from socket");
                            ok=false;
                        }
                    }catch(Exception e)
                    {
                        OnClientError(d,string.Format("Client @ [{0}] read error, disconnecting",d.IPAddress));
                        d.IsClosing=true;
                        ok=false;
                    }
                }
            }
            return ok;
        }
        
        public bool ProcessInput(ClientEventHandler cmdHandler)
        {
            bool ok=true;
            foreach(ClientDescriptor d in this.Clients)
            {
                if(!d.IsClosing&&d.PreviewCommand!=null)
                {
                    try
                    {
                        cmdHandler(d);
                    }catch(Exception e)
                    {
                        OnClientError(d,"Communicator.ProcessInput:"+e.Message);
                        ok=false;
                    }
                }
            }
            return ok;
        }        
        public bool WriteData()
        {
            bool ok=true;
            foreach(ClientDescriptor d in this.Clients)
            {
                if((!d.IsClosing||(d.IsClosing&&!d.HasError))&&d.WriteReady)
                {
                    try
                    {
                        if(!d.WriteToSocket())
                        {
                            OnClientError(d,"ClientDescriptor.WriteToSocket: error writing to socket");
                            ok=false;
                        }
                    }catch(Exception e)
                    {
                        OnClientError(d,"Communicator.WriteData:"+e.Message);
                        ok=false;
                    }
                }
            }
            return ok;
        }
        public bool KillAll()
        {
            bool ok=true;
            while(this.Clients.Count>0)
            {
                try{
                    if(!this.Clients[0].Close())
                    {
                        OnClientError(this.Clients[0],"Close: error closing socket");
                        ok=false;
                    }
                }catch(Exception e)
                {
                    OnClientError(this.Clients[0],e.Message);
                    ok=false;
                }
                DisposeClient(this.Clients[0]);
                this.Clients.RemoveAt(0);
            }
            return ok;
        }
        public bool KillDead()
        {
            bool ok=true;
            for(int i=0;i<this.Clients.Count;i++)
            {
                if(this.Clients[i].HasError||(this.Clients[i].IsClosing&&!this.Clients[i].WriteReady))
                {
                    try
                    {
                        if(!this.Clients[i].Close())
                        {
                            OnClientError(this.Clients[i],"Close: error closing socket");
                            ok=false;
                        }
                    }catch(Exception e)
                    {
                        OnClientError(this.Clients[i],e.Message);
                        ok=false;
                    }
                    DisposeClient(this.Clients[i]);
                    OnClientDisconnect(this.Clients[i]);
                    this.Clients.RemoveAt(i);
                    i=((i-1)<0)?0:i-1;
                }
            }
            return ok;
        }
    }
}

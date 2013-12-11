namespace Atlana.Network
{    
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    
    /// <summary>
    /// Description of ClientDescriptor.
    /// </summary>
    public class ClientDescriptor
    {        
        /// <summary>
        /// Maximum size of the read buffer.
        /// </summary>
        public const int MaxInputSize = 2048; // 2 kilobytes
        
        /// <summary>
        /// Maximum size of the write buffer.
        /// </summary>
        public const int MaxOutputSize = 4096; // 4 kilobytes
        
        private DescriptorStates state;
        
        private Atlana.Player player;
        
        /// <summary>
        /// States whether the client is to be closed next game loop.
        /// </summary>
        private bool isClosing;
        
        /// <summary>
        /// The input buffer of the client.
        /// </summary>
        private byte[] inBuffer;
        
        /// <summary>
        /// Last modified position in the input buffer.
        /// </summary>
        private int inPosition;
        
        /// <summary>
        /// The output buffer of the client.
        /// </summary>
        private byte[] outBuffer;
        
        /// <summary>
        /// Last modified position in the output buffer.
        /// </summary>
        private int outPosition;
        
        /// <summary>
        /// The client's socket.
        /// </summary>
        private Socket socket;
        
        /// <summary>
        /// The client socket's ip information.
        /// </summary>
        private System.Net.IPEndPoint ip;
        
        /// <summary>
        /// Initializes a new instance of the ClientDescriptor class.
        /// </summary>
        /// <param name="s">The socket of the client.</param>
        public ClientDescriptor(Socket s)
        {
            this.socket = s;
            this.socket.Blocking = true;
            this.ip = (IPEndPoint) s.RemoteEndPoint;
            this.IsClosing = false;
            this.inBuffer = new byte[ClientDescriptor.MaxInputSize];
            this.inPosition = 0;
            this.outBuffer = new byte[ClientDescriptor.MaxOutputSize];
            this.outPosition = 0;
            this.State=DescriptorStates.AskName;
            this.Player=null;
        }        
        
        /// <summary>
        /// Called when the client is about to be read from.
        /// </summary>
        public event EventHandler RecvingData;
        
        /// <summary>
        /// Called when the client is about to be written to.
        /// </summary>
        public event EventHandler SendingData;
        
        /// <summary>
        /// Called after the client has been read from.
        /// </summary>
        public event ClientDataEventHandler RecvData;
        
        /// <summary>
        /// Called after the client has been written to.
        /// </summary>
        public event ClientDataEventHandler SendData;
        
        /// <summary>
        /// Called when the output buffer is being written to.
        /// </summary>
        public event ClientDataEventHandler BufferOut;
        
        /// <summary>
        /// Called when the client socket is closed.
        /// </summary>
        public event EventHandler ConnectionClosed;        

        /// <summary>
        /// Gets or sets a value indicating the current state of the client
        /// </summary>
        public DescriptorStates State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
                if (!this.IsClosing && this.state == DescriptorStates.Closing)
                {
                    this.IsClosing = true;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating the current player associated with the socket.
        /// </summary>
        public Player Player
        {
            get
            {
                return this.player;
            }
            set
            {
                if(this.player!=null)
                {
                    this.player.Descriptor=null;
                }
                
                this.player = value;
                if(this.player!=null)
                {
                    this.player.Descriptor=this;
                }
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether the socket is currently connected.
        /// </summary>
        public bool Connected
        {
            get
            {
                return this.socket.Connected;
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the socket is to be closed.
        /// </summary>
        public bool IsClosing
        {
            get
            {
                return this.isClosing;
            }
            
            set
            {
                this.isClosing = value;
                if(this.IsClosing&&this.State!= DescriptorStates.Closing)
                {
                    this.State= DescriptorStates.Closing;
                }
            }
        }

        /// <summary>
        /// Gets the next command without removing it from the buffer.
        /// </summary>
        /// <remarks>
        /// A null reference will be returned if no commands are currently available.
        /// </remarks>
        public string PreviewCommand
        {
            get
            {
                bool nl = false;
                char pc = '\0';
                int p = 0;
                StringBuilder cmd = new StringBuilder();
                foreach (byte b in this.inBuffer)
                {
                    if (p == this.inPosition)
                    {
                        break;
                    }
                    
                    p++;
                    if ((pc == '\r' && b == (byte)'\n') || b == (byte)'\n')
                    {
                        nl = true;
                        break;
                    }
                    else if (pc == (byte)'\r')
                    {
                        p--;
                        nl = true;
                        break;
                    }
                    
                    if (b != (byte)'\r' && b != (byte)'\n')
                    {
                        cmd.Append((char)b);
                    }
                    
                    pc = (char)b;
                }
                
                if (!nl)
                {
                    return null;
                }
                
                return cmd.ToString();
            }
        }
        
        /// <summary>
        /// Gets the next available command and removes it from the input buffer.
        /// </summary>
        /// <remarks>
        /// A null reference will be returned if no commands are currently available.
        /// </remarks>
        public string Command
        {
            get
            {
                bool nl = false;
                char pc = '\0';
                int p = 0;
                StringBuilder cmd = new StringBuilder();
                foreach (byte b in this.inBuffer)
                {
                    if (p == this.inPosition)
                    {
                        break;
                    }
                    
                    p++;
                    if ((pc == '\r' && b == (byte)'\n') || b == (byte)'\n')
                    {
                        nl = true;
                        break;
                    }
                    else if (pc == (byte)'\r')
                    {
                        p--;
                        nl = true;
                        break;
                    }
                    
                    if (b != (byte)'\r' && b != (byte)'\n')
                    {
                        cmd.Append((char)b);
                    }
                    
                    pc = (char)b;
                }
                
                if (!nl)
                {
                    return null;
                }
                
                for (int i = p; i < this.inPosition; i++)
                {
                    this.inBuffer[i - p] = this.inBuffer[i];
                }
                
                this.inPosition -= p;
                return cmd.ToString();
            }
        }
        
        /// <summary>
        /// Gets the IP address of the client socket.
        /// </summary>
        public string IPAddress
        {
            get
            {
                return this.ip.Address.ToString();
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether the client is having any errors.
        /// </summary>
        public bool HasError
        {
            get
            {
                bool b = false;
                try
                {
                    b = this.socket.Poll(0, SelectMode.SelectError);
                }
                catch (ObjectDisposedException e)
                {
                    this.IsClosing = true;
                    return true;
                }
                
                return b;
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether the client is ready to be read from.
        /// </summary>
        public bool ReadReady
        {
            get
            {
                bool b = true;
                try
                {
                    b = this.socket.Connected && this.socket.Poll(0, SelectMode.SelectRead);
                }
                catch (ObjectDisposedException e)
                {
                    this.IsClosing = true;
                    return false;
                }
                
                return b;
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether the client is ready to be written to.
        /// </summary>
        public bool WriteReady
        {
            get
            {
                bool b = true;
                try
                {
                    b = this.socket.Connected && this.outPosition > 0 && this.socket.Poll(0, SelectMode.SelectWrite);
                }
                catch (ObjectDisposedException e)
                {
                    this.IsClosing = true;
                    return false;
                }
                
                return b;
            }
        }
                        
        /// <summary>
        /// Closes the client socket.
        /// </summary>
        /// <returns>If there were any errors when closing the client's socket.</returns>
        public bool Close()
        {
            this.socket.Shutdown(SocketShutdown.Both);
            this.socket.Close();
            this.OnConnectionClosed();
            return true;
        }
        
        /// <summary>
        /// Reads any available data from the client socket.
        /// </summary>
        /// <returns>If there were any errors while processing the read request.</returns>
        public bool ReadFromSocket()
        {
            if (this.ReadReady && !this.HasError)
            {
                if (this.HasError)
                {
                    return false;
                }
                
                this.OnRecvingData();
                try
                {
                    NetworkStream ns = new NetworkStream(this.socket);
                    int size = ((this.socket.Available + this.inPosition) > ClientDescriptor.MaxInputSize) ? ((this.socket.Available + this.inPosition) - ClientDescriptor.MaxInputSize) : this.socket.Available;
                    byte[] buf = new byte[size];
                    int read = ns.Read(buf, 0, buf.Length);
                    if (read == 0)
                    {
                        throw new InvalidDataException("Invalid data read from client");
                    }
                    
                    this.OnRecvData(ref buf, ref read);
                    Buffer.BlockCopy(buf, 0, this.inBuffer, this.inPosition, size);
                    this.inPosition += size;
                    if (size < read)
                    {
                        throw new StackOverflowException("Size of data read greater than size of buffer");
                    }
                }
                catch (ObjectDisposedException e)
                {
                    this.IsClosing = true;
                }
                catch (InvalidDataException ie)
                {
                    this.IsClosing = true;
                }
            }
            else
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Writes the output buffer to the client's socket.
        /// </summary>
        /// <returns>If there were any errors while processing the write request.</returns>
        public bool WriteToSocket()
        {
            if (this.WriteReady)
            {
                this.OnSendingData();
                if(this.Player!=null&&this.State== DescriptorStates.Playing)
                {
                    this.WriteToBuffer(string.Format("\r\n\r\n{0} ",this.Player.Prompt));
                }
                NetworkStream ns = new NetworkStream(this.socket);
                int size = this.outPosition;
                byte[] buf = new byte[size];                    
                Buffer.BlockCopy(this.outBuffer, 0, buf, 0, size);
                this.OnSendData(ref buf, ref size);
                ns.Write(buf, 0, size);
                this.outPosition -= size;
                if (this.outPosition < 0)
                {
                    this.outPosition = 0;
                }
                
                ns.Flush();
            }
            else
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Write's a string to the client's output buffer.
        /// </summary>
        /// <param name="text">The value to write.</param>
        /// <returns>If there were any errors while buffering.</returns>
        public bool WriteToBuffer(string text)
        {
            if ((this.outPosition + Encoding.ASCII.GetByteCount(text)) >= ClientDescriptor.MaxOutputSize)
            {
                return false;
            }
            string ansi=this.AnsiParse(text);
            byte[] buf = Encoding.ASCII.GetBytes(ansi);
            int size = Encoding.ASCII.GetByteCount(ansi);
            this.OnBufferOut(ref buf, ref size);
            Buffer.BlockCopy(buf, 0, this.outBuffer, this.outPosition, size);
            this.outPosition += size;
            return true;
        }

        protected virtual string AnsiParse(string s)
        {
            string t = "";
            bool fc = false;
            bool bc = false;
            string cc= "";
            foreach (char c in s)
            {
                if (fc) // foreground color
                {
                    switch (c)
                    {
                        case '&':
                            t += "&";
                            fc=false;
                            continue;
                        case 'z' : // reset
                            cc += "0;49;39";
                            break;
                        case 'x' : // black
                            cc += "0;30";
                            break;
                        case 'X' : // light black
                            cc += "1;30";
                            break;
                        case 'r' : // red
                            cc += "0;31";
                            break;
                        case 'R' : // light red
                            cc += "1;31";
                            break;
                        case 'g' : // green
                            cc += "0;32";
                            break;
                        case 'G' : // light green
                            cc += "1;32";
                            break;
                        case 'y' : // yellow
                            cc += "0;33";
                            break;
                        case 'Y' : // light yellow
                            cc += "1;33";
                            break;
                        case 'b' : // blue
                            cc += "0;34";
                            break;
                        case 'B' : // light blue
                            cc += "1;34";
                            break;
                        case 'p' : // purple
                            cc += "0;35";
                            break;
                        case 'P' : // light purple
                            cc += "1;35";
                            break;
                        case 'c' : // cyan
                            cc += "0;36";
                            break;
                        case 'C' : // light cyan
                            cc += "1;36";
                            break;
                        case 'w' : // white
                            cc += "0;37";
                            break;
                        case 'W' : // light white(redundant isnt it?)
                            cc += "1;37";
                            break;
                    }
                    fc=false;
                    cc += 'm';
                    t+=cc;
                    continue;
                }
                else if (bc) // background color
                {
                    switch (c)
                    {
                        case '^':
                            t += "^";
                            bc=false;
                            continue;
                        case 'x' : // black
                            cc += "0;40";
                            break;
                        case 'X' : // light black
                            cc += "1;40";
                            break;
                        case 'r' : // red
                            cc += "0;41";
                            break;
                        case 'R' : // light red
                            cc += "1;41";
                            break;
                        case 'g' : // green
                            cc += "0;42";
                            break;
                        case 'G' : // light green
                            cc += "1;42";
                            break;
                        case 'y' : // yellow
                            cc += "0;43";
                            break;
                        case 'Y' : // light yellow
                            cc += "1;43";
                            break;
                        case 'b' : // blue
                            cc += "0;44";
                            break;
                        case 'B' : // light blue
                            cc += "1;44";
                            break;
                        case 'p' : // purple
                            cc += "0;45";
                            break;
                        case 'P' : // light purple
                            cc += "1;45";
                            break;
                        case 'c' : // cyan
                            cc += "0;46";
                            break;
                        case 'C' : // light cyan
                            cc += "1;46";
                            break;
                        case 'w' : // white
                            cc += "0;47";
                            break;
                        case 'W' : // light white(redundant isnt it?)
                            cc += "1;47";
                            break;
                    }
                    bc=false;
                    cc += 'm';
                    t+=cc;
                    continue;
                }
                else if (c=='&')
                {
                    fc = true;
                    cc = "\x1B[";
                    continue;
                }
                else if (c=='^')
                {
                    bc = true;
                    cc ="\x1B[";
                    continue;
                }
                
                t += c;
            }
            return t;
        }
        
        /// <summary>
        /// Invokes the RecvingData event.
        /// </summary>
        protected virtual void OnRecvingData()
        {
            if (this.RecvingData != null)
            {
                this.RecvingData(this, EventArgs.Empty);
            }
        }
        
        /// <summary>
        /// Invokes the SendingData event.
        /// </summary>
        protected virtual void OnSendingData()
        {
            if (this.SendingData != null)
            {
                this.SendingData(this, EventArgs.Empty);
            }
        }
        
        /// <summary>
        /// Invokes the SendData event.
        /// </summary>
        /// <param name="data">Data to be sent to client.</param>
        /// <param name="size">Size of the <paramref name="data">data</paramref>.</param>
        protected virtual void OnSendData(ref byte[] data, ref int size)
        {
            if (this.SendData != null)
            {
                this.SendData(this, ref data, ref size);
            }
        }

        /// <summary>
        /// Invokes the RecvData event.
        /// </summary>
        /// <param name="data">Data received from the client.</param>
        /// <param name="size">Size of the <paramref name="data">data</paramref>.</param>        
        protected virtual void OnRecvData(ref byte[] data, ref int size)
        {
            if (this.RecvData != null)
            {
                this.RecvData(this, ref data, ref size);
            }
        }
        
        /// <summary>
        /// Invokes the BufferOut event.
        /// </summary>
        /// <param name="data">Data to be inserted into the client's output buffer.</param>
        /// <param name="size">Size of the <paramref name="data">data</paramref>.</param>   
        protected virtual void OnBufferOut(ref byte[] data, ref int size)
        {
            if (this.BufferOut != null)
            {
                this.BufferOut(this, ref data, ref size);
            }
        }
        
        /// <summary>
        /// Invokes the ConnectionClosed event.
        /// </summary>        
        protected virtual void OnConnectionClosed()
        {
            if (this.ConnectionClosed != null)
            {
                this.ConnectionClosed(this, EventArgs.Empty);
            }
        }                
    }
}

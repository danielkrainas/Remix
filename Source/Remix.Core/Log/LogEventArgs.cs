namespace Atlana.Log
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        public LogEventArgs()
            : this(String.Empty, LogChannel.Info)
        { }

        public LogEventArgs(string message, LogChannel channel)
            : base()
        {
            this.Message = message;
            this.Channel = channel;
        }

        public string Message
        {
            get;
            protected set;
        }

        public LogChannel Channel
        {
            get;
            protected set;
        }
    }
}

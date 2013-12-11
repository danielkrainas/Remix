using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Atlana.Log
{
    /// <summary>
    /// Singleton class, used for logging text to console and file.
    /// </summary>
    public sealed class Logger : IDisposable
    {
        #region "Constants"

        private const string LogStringFormat = "[{TIMESTAMP}] {CHANNEL}: {MESSAGE}";

        #endregion // "Constants"

        #region "Static Fields"

        private static Logger _instance = null;

        #endregion // "Static Fields"

        #region "Static Properties"

        public static Logger Instance
        {
            get
            {
                if (Logger._instance == null)
                {
                    Logger._instance = new Logger();
                }

                return Logger._instance;
            }
        }

        #endregion // "Static Properties"

        #region "Static Methods"

        public static void LogMessage(LogChannel channel, string value)
        {
            Logger.Instance.Log(channel, value);
        }

        public static void LogMessage(LogChannel channel, string format, object arg)
        {
            Logger.Instance.Log(channel, String.Format(format, arg));
        }

        public static void LogMessage(LogChannel channel, string format, object arg0, object arg1)
        {
            Logger.Instance.Log(channel, String.Format(format, arg0, arg1));
        }

        public static void LogMessage(LogChannel channel, string format, object arg0, object arg1, object arg2)
        {
            Logger.Instance.Log(channel, String.Format(format, arg0, arg1, arg2));
        }

        public static void LogMessage(LogChannel channel, string format, params object[] args)
        {
            Logger.Instance.Log(channel, String.Format(format, args));
        }

        public static void Bug(Exception e)
        {
            Logger.Instance.Log(e);
        }

        public static void Bug(string format, object arg)
        {
            Logger.Instance.Log(LogChannel.Bug, format, arg);
        }

        public static void Bug(string format, object arg0, object arg1)
        {
            Logger.Instance.Log(LogChannel.Bug, format, arg0, arg1);
        }

        public static void Bug(string format, object arg0, object arg1, object arg2)
        {
            Logger.Instance.Log(LogChannel.Bug, format, arg0, arg1, arg2);
        }

        public static void Bug(string format, params object[] args)
        {
            Logger.Instance.Log(LogChannel.Bug, format, args);
        }

        public static void Info(string value)
        {
            Logger.Instance.Log(LogChannel.Info, value);
        }

        public static void Info(string format, object arg)
        {
            Logger.Instance.Log(LogChannel.Info, format, arg);
        }

        public static void Info(string format, object arg0, object arg1)
        {
            Logger.Instance.Log(LogChannel.Info, format, arg0, arg1);
        }

        public static void Info(string format, object arg0, object arg1, object arg2)
        {
            Logger.Instance.Log(LogChannel.Info, format, arg0, arg1, arg2);
        }

        public static void Info(string format, params object[] args)
        {
            Logger.Instance.Log(LogChannel.Info, format, args);
        }

        public static void Comm(string value)
        {
            Logger.Instance.Log(LogChannel.Comm, value);
        }

        public static void Comm(string format, object arg)
        {
            Logger.Instance.Log(LogChannel.Comm, format, arg);
        }

        public static void Comm(string format, object arg0, object arg1)
        {
            Logger.Instance.Log(LogChannel.Comm, format, arg0, arg1);
        }

        public static void Comm(string format, object arg0, object arg1, object arg2)
        {
            Logger.Instance.Log(LogChannel.Comm, format, arg0, arg1, arg2);
        }

        public static void Comm(string format, params object[] args)
        {
            Logger.Instance.Log(LogChannel.Comm, format, args);
        }

        #endregion // "Static Methods"

        #region "Nested Types"

        private struct BufferedMessage
        {
            public BufferedMessage(string message, LogChannel channel)
                : this()
            {
                if (message == null)
                {
                    throw new ArgumentNullException("message");
                }

                this.Message = message;
                this.Channel = channel;
            }

            public string Message
            {
                get;
                set;
            }

            public LogChannel Channel
            {
                get;
                set;
            }
        }

        #endregion // "Nested Types"

        #region "Private Fields"

        private StreamWriter logFile;
        private ConsoleColor originalColor;
        private Queue<BufferedMessage> buffer;
        private bool isBuffering;

        #endregion // "Private Fields"

        #region "Events"

        public event LogEventHandler MessageLogged;

        #endregion // "Events"

        #region "Constructors"

        private Logger()
        {
            this.logFile = null;
            this.originalColor = Console.ForegroundColor;
            this.MessageLogged = null;
            this.buffer = new Queue<BufferedMessage>();
            this.isBuffering = false;
        }

        #endregion // "Constructors"

        #region "Methods"

        private void OnMessageLogged(string message, LogChannel channel)
        {
            LogEventArgs e = new LogEventArgs(message, channel);
            var d = this.MessageLogged;
            if (d != null)
            {
                d(this, e);
            }
        }

        private string GetOutputFormat()
        {
            var s = Logger.LogStringFormat;
            s = s.Replace("TIMESTAMP", "0");
            s = s.Replace("CHANNEL", "1");
            s = s.Replace("MESSAGE", "2");
            return s;
        }

        private void LogToFile(string output, bool newLine = true)
        {
            if (Logger.Instance.logFile != null)
            {
                if (newLine)
                {
                    this.logFile.WriteLine(output);
                }
                else
                {
                    this.logFile.Write(output);
                }

                Logger.Instance.logFile.Flush();
            }
        }

        public void StartBuffer()
        {
            this.isBuffering = true;
        }

        public void FlushBuffer()
        {
            this.isBuffering = false;
            while (this.buffer.Any())
            {
                var msg = this.buffer.Dequeue();
                this.Log(msg.Channel, msg.Message);
            }
        }

        public void CloseLogFile()
        {
            if (this.logFile != null)
            {
                if (!this.logFile.AutoFlush)
                {
                    this.logFile.Flush();
                }

                this.logFile.Close();
                this.logFile.Dispose();
                this.logFile = null;
            }
        }

        public void SetLogFile(string fileName)
        {
            this.CloseLogFile();
            this.logFile = new StreamWriter(File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write));
        }

        public void Log(LogChannel channel, string value)
        {
            if (this.isBuffering)
            {
                Logger.BufferedMessage msg = new BufferedMessage(value, channel);
                this.buffer.Enqueue(msg);
                return;
            }

            string timeStamp = DateTime.Now.ToString();
            if (value == String.Empty)
            {
                this.LogToFile(value, true);
                Console.WriteLine();
            }
            else
            {
                string output = String.Format(this.GetOutputFormat(), timeStamp, Enum.GetName(typeof(LogChannel), channel), value);
                this.LogToFile(output, true);
                Console.WriteLine(output);
                this.OnMessageLogged(value, channel);
            }
        }

        public void Log(LogChannel channel, string format, object arg)
        {
            this.Log(channel, String.Format(format, arg));
        }

        public void Log(LogChannel channel, string format, object arg0, object arg1)
        {
            this.Log(channel, String.Format(format, arg0, arg1));
        }

        public void Log(LogChannel channel, string format, object arg0, object arg1, object arg2)
        {
            this.Log(channel, String.Format(format, arg0, arg1, arg2));
        }

        public void Log(LogChannel channel, string format, params object[] args)
        {
            this.Log(channel, String.Format(format, args));
        }

        public void Log(Exception e)
        {
            this.Log(LogChannel.Bug, "Exception: {2}: {0}\n{1}", e.Message, e.StackTrace, e.GetType().FullName);
        }

        public void Log()
        {
            this.Log(LogChannel.Comm, String.Empty);
        }

        public bool LogProcess(LogChannel channel, Delegate action, string message, params object[] arguments)
        {
            Func<bool> f = delegate()
                {
                    var result = action.DynamicInvoke(arguments);
                    if (result != null && result is Boolean)
                    {
                        return (bool)result;
                    }

                    return true;
                };

            return this.LogProcess(channel, f, message);
        }

        public bool LogProcess(LogChannel channel, Func<bool> action, string message)
        {
            const string failMessage = "FAIL";
            const string successMessage = " OK ";
            bool result = false;
            string timeStamp = DateTime.Now.ToString();
            var output = String.Format(this.GetOutputFormat(), timeStamp, channel, message);
            this.LogToFile(output, false);
            Console.Write(output.PadRight(73, '.'));
            Exception ex = null;
            try
            {
                this.StartBuffer();
                result = action.Invoke();
            }
            catch (Exception e)
            {
                ex = e;
                result = false;
            }

            Console.Write("[");
            if (result)
            {
                Console.Write(successMessage);
            }
            else
            {
                Console.Write(failMessage);
            }

            Console.WriteLine("]");
            this.LogToFile(String.Format("[{0, 4}]", result ? successMessage : failMessage), true);
            if (ex != null)
            {
                this.Log(ex);
            }

            this.FlushBuffer();
            return result;
        }

        public void Dispose()
        {
            Console.ForegroundColor = this.originalColor;
        }

        #endregion // "Methods"
    }
}

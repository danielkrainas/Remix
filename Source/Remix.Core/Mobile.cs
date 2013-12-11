namespace Atlana
{
    using System;
    using Atlana.World;
    using Atlana.Interpret;
    using System.Collections.Generic;
    using System.Collections;
    using System.Linq;

    /// <summary>
    /// Description of Mobile.
    /// </summary>
    public class Mobile : MudObject
    {
        private Player player;

        public Mobile()
        {
            this.player = null;
            this.Level = 0;
        }

        public string Name
        {
            get;
            set;
        }

        public Room Room
        {
            get;
            set;
        }

        public int Level
        {
            get;
            set;
        }

        public Player Player
        {
            get
            {
                return this.player;
            }
            set
            {
                if (this.player != null)
                {
                    this.player.Mobile = null;
                }
                this.player = value;
                if (this.player != null && this.player.Mobile != this)
                {
                    this.player.Mobile = this;
                }
                if (this.player != null)
                {
                    this.Name = this.player.Name;
                }
            }
        }

        public bool IsPC
        {
            get
            {
                return (this.Player != null);
            }
        }

        public bool IsNPC
        {
            get
            {
                return (this.Player == null);
            }
        }

        public void Do(string cmd)
        {
            Interpreter.Instance.Interpret(this, cmd);
        }

        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(format, args));
        }

        public void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            this.WriteLine(string.Format(format, arg0, arg1, arg2));
        }

        public void WriteLine(string format, object arg0, object arg1)
        {
            this.WriteLine(string.Format(format, arg0, arg1));
        }

        public void WriteLine(string format, object arg0)
        {
            this.WriteLine(string.Format(format, arg0));
        }

        public void WriteLine(string value)
        {
            if (this.Player != null)
            {
                this.Player.WriteLine(value);
            }
        }

        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(format, args));
        }

        public void Write(string format, object arg0, object arg1, object arg2)
        {
            this.Write(string.Format(format, arg0, arg1, arg2));
        }

        public void Write(string format, object arg0, object arg1)
        {
            this.Write(string.Format(format, arg0, arg1));
        }

        public void Write(string format, object arg0)
        {
            this.Write(string.Format(format, arg0));
        }

        public void Write(string value)
        {
            if (this.Player != null)
            {
                this.Player.Write(value);
            }
        }

        public IEnumerable<Role> Roles
        {
            get
            {
                if (this.player != null)
                {
                    return this.player.Roles;
                }

                return Enumerable.Empty<Role>();
            }
        }
    }
}

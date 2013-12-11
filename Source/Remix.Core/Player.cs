namespace Atlana
{
    using System;
    using System.Xml;
    using Atlana.Configuration;
    using Atlana.Log;
    using System.Linq;
    using Atlana.Network;
    using System.ComponentModel.DataAnnotations;
    using Atlana.Data;
    using System.Collections.Generic;

    /// <summary>
    /// Description of Player.
    /// </summary>
    public class Player : MudObject
    {
        #region "Private Fields"

        private Mobile mobile;

        #endregion // "Private Fields"

        #region "Constructors"

        public Player()
        {
            this.Descriptor = null;
            this.Name = String.Empty;
            this.Password = String.Empty;
            this.mobile = null;
            this.Prompt = ">>";
        }

        #endregion // "Constructors"

        #region "Methods"

        public bool Load()
        {
            if (this.Name == null)
            {
                return false;
            }

            this.Mobile = new Mobile();
            return true;
        }

        public bool Preload(string name)
        {
            using (var playerRepo = new Repository<Player>())
            {
                var player = playerRepo.Find(p => p.Name == name, p => p.Roles);
                if (player == null)
                {
                    return false;
                }

                this.Name = player.Name;
                this.Password = player.Password;
                this.LastLogin = player.LastLogin;
                this.DateCreated = player.DateCreated;
                player.LastLogin = DateTime.UtcNow;
                var roles = new List<Role>(player.Roles.Select(r=> RoleManager.Current.GetRole(r.Id)));
                this.Roles = roles;
                playerRepo.Update(player);
            }

            return true;
        }

        public void WriteLine(string value)
        {
            this.Descriptor.WriteToBuffer(string.Format("{0}\r\n", value));
        }

        public void Write(string value)
        {
            this.Descriptor.WriteToBuffer(value);
        }

        public void Save()
        {
            if (this.Mobile != null)
            {
                using (var playerRepo = new Repository<Player>())
                {
                    var player = this.MemberwiseClone() as Player;
                    if (playerRepo.Update(player) > 0)
                    {
                        this.Write("Ok.");
                    }
                }
            }
        }

        public void Quit()
        {
            this.WriteLine("GoodBye!");
            this.Descriptor.IsClosing = true;
            if (this.Mobile != null && this.Mobile.Room != null)
            {
                this.Mobile.Room.WriteToAll(this.Mobile, "{0} disappears.", this.Name);
                this.Mobile.Room.MobFromRoom(this.Mobile);
                this.Mobile = null;
            }

            Logger.Info("{0} quit the game.", this.Name);
        }

        public void SetProperty(string property, object value)
        {
            switch (property)
            {
                case "name":
                    this.Name = Strings.Capitalize((string)value);
                    break;

                case "password":
                    this.Password = (string)value;
                    break;
            }
        }

        public object GetProperty(string property)
        {
            switch (property)
            {
                case "name": return this.Name;
                case "password": return this.Password;
            }

            return null;
        }

        #endregion // "Methods"

        #region "Properties"

        [NotMapped]
        public Mobile Mobile
        {
            get
            {
                return this.mobile;
            }
            set
            {
                if (this.mobile != null)
                {
                    this.mobile = null;
                }

                this.mobile = value;
                if (this.mobile != null && this.mobile.Player != this)
                {
                    this.mobile.Player = this;
                }
            }
        }

        [NotMapped]
        public ClientDescriptor Descriptor
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Password
        {
            get;
            set;
        }

        public DateTime LastLogin
        {
            get;
            set;
        }

        public DateTime DateCreated
        {
            get;
            set;
        }

        public ICollection<Role> Roles
        {
            get;
            set;
        }

        [NotMapped]
        public string Prompt
        {
            get;
            set;
        }

        #endregion // "Properties"
    }
}

namespace Atlana.World
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Description of Room.
    /// </summary>
    public class Room
    {
        public static Room Create(string title, string description)
        {
            var r = new Room()
            {
                Title = title,
                Description = description,
                Exits = new List<RoomExit>()
            };

            return r;
        }

        public Room()
        {
            this.Id = -1;
            this.Description = String.Empty;
            this.Title = String.Empty;
            this.MobsInRoom = new List<Mobile>();
            this.Exits = null;
        }

        public int Id
        {
            get;
            set;
        }

        public Area Area
        {
            get;
            set;
        }

        public int AreaId
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public ICollection<RoomExit> Exits
        {
            get;
            set;
        }

        [NotMapped]
        public List<Mobile> MobsInRoom
        {
            get;
            set;
        }

        public void SetExitRoom(ExitDirections dir, Room r)
        {
            if (this.HasExit(dir))
            {
                RoomExit e = GetExit(dir);
                //e.ToRoom = r;
            }
        }

        public bool HasExit(ExitDirections dir)
        {
            if (this.Exits.AsQueryable().Count(z => z.Direction == dir) > 0)
            {
                return true;
            }
            return false;
        }

        public RoomExit GetExit(ExitDirections dir)
        {
            return this.Exits.FirstOrDefault(z => z.Direction == dir);
        }

        public Room ExitRoom(ExitDirections dir)
        {
            var exit = this.GetExit(dir);
            if (exit != null)
            {
                return exit.DestinationRoom;
            }

            return null;
        }

        public void MobToRoom(Mobile m)
        {
            if (this.MobsInRoom.SingleOrDefault(z => z.Equals(m)) == null)
            {
                if (m.Room != null)
                {
                    m.Room.MobFromRoom(m);
                }
                this.MobsInRoom.Add(m);
                m.Room = this;
            }
        }

        public void MobFromRoom(Mobile m)
        {
            if (this.MobsInRoom.SingleOrDefault(z => z.Equals(m)) != null)
            {
                this.MobsInRoom.Remove(m);
                m.Room = null;
            }
        }

        public void WriteToAll(Mobile except, string value)
        {
            foreach (Mobile m in this.MobsInRoom)
            {
                if (m.Equals(except))
                {
                    continue;
                }
                m.WriteLine(value);
            }
        }

        public void WriteToAll(Mobile except, string format, object arg0)
        {
            this.WriteToAll(except, string.Format(format, arg0));
        }

        public void WriteToAll(Mobile except, string format, object arg0, object arg1)
        {
            this.WriteToAll(except, string.Format(format, arg0, arg1));
        }

        public void WriteToAll(Mobile except, string format, object arg0, object arg1, object arg2)
        {
            this.WriteToAll(except, string.Format(format, arg0, arg1, arg2));
        }

        public void WriteToAll(Mobile except, string format, params object[] args)
        {
            this.WriteToAll(except, string.Format(format, args));
        }

        public void WriteToAll(string value)
        {
            foreach (Mobile m in this.MobsInRoom)
            {
                m.WriteLine(value);
            }
        }

        public void WriteToAll(string format, object arg0)
        {
            this.WriteToAll(string.Format(format, arg0));
        }

        public void WriteToAll(string format, object arg0, object arg1)
        {
            this.WriteToAll(string.Format(format, arg0, arg1));
        }

        public void WriteToAll(string format, object arg0, object arg1, object arg2)
        {
            this.WriteToAll(string.Format(format, arg0, arg1, arg2));
        }

        public void WriteToAll(string format, params object[] args)
        {
            this.WriteToAll(string.Format(format, args));
        }

        public void ShowTo(Mobile m, bool detailed)
        {
            /*string s = string.Format("/(&W{0}&G)--", this.Title);
            s = s.PadRight(73, '-');
            s = "&G" + s;
            m.WriteLine("{0}\\", s);*/
            m.WriteLine("&W{0}&z", this.Title);
            m.WriteLine("&R----------------------------------------------------------------------------&z");
            m.WriteLine("&w{0}&z", this.Description);
            //m.WriteLine("&G\\--------------------------------------------------------------------/");
            m.WriteLine("&R----------------------------------------------------------------------------&z");
            if (detailed)
            {
                m.WriteLine("&WExits:");
                foreach (RoomExit e in this.Exits)
                {
                    m.WriteLine("&G{0} &W- &G{1}", ExitDirections.GetName(typeof(ExitDirections), (int)e.Direction), e.DestinationRoom.Title);
                }
            }

            m.Write("&z");
        }
    }
}
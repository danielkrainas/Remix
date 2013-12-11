namespace Atlana.World
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Description of Area.
    /// </summary>
    public class Area
    {
        public static Area Create(string name)
        {
            Area a = new Area();
            a.Name = name;
            a.Rooms = new List<Room>();
            return a;
        }

        public Area()
        {
            this.Id = -1;
            this.Name = String.Empty;
            this.Rooms = null;
        }

        public int Id
        {
            get;
            set;
        }

        public ICollection<Room> Rooms
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public Room GetRoom(int id)
        {
            return this.Rooms.FirstOrDefault(z => z.Id == id);
        }
    }
}

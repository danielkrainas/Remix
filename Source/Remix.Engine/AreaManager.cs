/*
 * Created by SharpDevelop.
 * User: Brendan
 * Date: 7/16/2009
 * Time: 12:34 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;
using System.Collections.Generic;
using Atlana.World;
using Atlana.Data;

namespace Atlana.Engine
{
    /// <summary>
    /// Description of AreaManager.
    /// </summary>
    public sealed class AreaManager
    {
        private static AreaManager instance = new AreaManager();

        public static AreaManager Instance
        {
            get
            {
                return instance;
            }
        }

        public Room Limbo
        {
            get;
            private set;
        }

        public List<Area> Areas
        {
            get;
            private set;
        }

        public int RoomCount
        {
            get
            {
                return this.Areas.Sum(a => a.Rooms.Count());
            }
        }

        private AreaManager()
        {
            this.Areas = new List<Area>();
            this.Limbo = null;
        }

        public Area GetArea(int roomId)
        {
            return this.Areas.FirstOrDefault(a => a.Rooms.Any(r => r.Id == roomId));
        }

        public void InitializeLimbo()
        {
            var a = Area.Create("System Area");
            var r = Room.Create("Limbo", String.Empty);
            r.Id = 0;
            this.Limbo = r;
            a.Rooms.Add(r);
            this.Areas.Add(a);
        }

        public bool LoadAreas(DataContext context)
        {
            using (var areaRepo = new AreaRepository(context, true))
            {
                this.Areas.AddRange(areaRepo.All());
            }

            return true;
        }
    }
}

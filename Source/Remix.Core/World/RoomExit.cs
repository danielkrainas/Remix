using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Atlana.World
{
    public class RoomExit
    {
        public RoomExit()
        {
            this.Id = -1;
            this.Room = null;
            this.DestinationRoom = null;
            this.Direction = ExitDirections.North;
        }

        public int Id
        {
            get;
            set;
        }

        public int RoomId
        {
            get;
            set;
        }

        public Room Room
        {
            get;
            set;
        }

        public int DestinationRoomId
        {
            get;
            set;
        }

        public Room DestinationRoom
        {
            get;
            set;
        }

        [Column("ExitDirectionId")]
        public ExitDirection Direction
        {
            get;
            set;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlana.Interpret;

namespace Atlana
{
    public class Role
    {
        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public ICollection<Command> Commands
        {
            get;
            set;
        }

        public ICollection<Player> Players
        {
            get;
            set;
        }
    }
}

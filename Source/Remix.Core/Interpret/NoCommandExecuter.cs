using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlana.Interpret
{
    public sealed class NoCommandExecuter : ICommandExecuter
    {
        public CommandReturn Do(Mobile m, string args)
        {
            m.WriteLine("The function for this command is missing.");
            return CommandReturn.OK;
        }
    }
}

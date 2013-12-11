using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlana.Interpret
{
    public interface ICommandExecuter
    {
        CommandReturn Do(Mobile m, string args);
    }
}

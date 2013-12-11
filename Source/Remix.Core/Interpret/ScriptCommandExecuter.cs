using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlana.Interpret
{
    internal class ScriptCommandExecuter : ICommandExecuter
    {
        private readonly string script;

        public ScriptCommandExecuter(string script)
        {
            this.script = script;
        }

        public CommandReturn Do(Mobile m, string args)
        {
            var result = CommandReturn.Error;
            var sm = new ScriptManager(new RubyScriptEngine());
            sm.SetVariable("m", m);
            sm.SetVariable("args", args);
            var r = sm.Execute(this.script);
            if (r == null)
            {
                result = CommandReturn.OK;
            }
            else if (r is CommandReturn)
            {
                result = (CommandReturn)r;
            }

            return result;
        }
    }
}

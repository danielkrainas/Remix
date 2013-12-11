// -----------------------------------------------------------------------
// <copyright file="ScriptingEngine.cs" company="Kennedy Space Center">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Atlana
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using IronRuby;
    using System.ComponentModel;
    using IronRuby.Builtins;
    using Microsoft.Scripting.Hosting;
    using Atlana.Log;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public sealed class ScriptManager
    {
        private static readonly Type DefaultEngineType = typeof(RubyScriptEngine);

        private static ScriptManager _instance = null;

        public static ScriptManager GlobalInstance
        {
            get
            {
                if (ScriptManager._instance == null)
                {
                    var engine = Activator.CreateInstance(ScriptManager.DefaultEngineType) as IScriptEngine;
                    ScriptManager._instance = new ScriptManager(engine);
                }

                return ScriptManager._instance;
            }
        }

        public static void RestoreDefaultGlobalEngine()
        {
            if (!ScriptManager.DefaultEngineType.IsInstanceOfType(ScriptManager.GlobalInstance.engine))
            {
                var engine = Activator.CreateInstance(ScriptManager.DefaultEngineType) as IScriptEngine;
                ScriptManager.SetGlobalEngine(engine);
            }
        }

        public static void SetGlobalEngine(IScriptEngine engine)
        {
            if (engine == null)
            {
                engine = Activator.CreateInstance(ScriptManager.DefaultEngineType) as IScriptEngine;
            }

            if (ScriptManager.GlobalInstance.engine != engine)
            {
                Logger.Instance.Log(LogChannel.Info, "Overriding global script engine.");
                foreach(var variable in ScriptManager.GlobalInstance.GetVariableNames())
                {
                    engine.SetVariable(variable, ScriptManager.GlobalInstance.GetVariable(variable));
                }

                ScriptManager.GlobalInstance.engine = engine;
            }
        }

        private IScriptEngine engine;

        public ScriptManager(IScriptEngine engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException();
            }

            this.engine = engine;
        }

        public string EngineName
        {
            get
            {
                return this.engine.Name;
            }
        }

        public dynamic Execute(string expression)
        {
            dynamic d = null;
            try
            {
                d = this.engine.Execute(expression);
            }
            catch (ScriptError se)
            {
                Logger.Instance.Log(se);
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
            }

            return d;
        }

        public void SetVariable(string name, object value)
        {
            this.engine.SetVariable(name, value);
        }

        public dynamic GetVariable(string name)
        {
            dynamic val = null;
            if (name.IndexOf('.') >= 0)
            {
                var v = name.Split('.');
                val = this.GetMember(v[0], v[1]);
            }
            else if(this.VarExists(name))
            {
                val = this.engine.GetVariable(name);
            }

            return val;
        }

        public dynamic GetMember(string varName, string memberName)
        {
            dynamic val = null;
            if (this.VarExists(varName))
            {
                val = this.engine.GetVariableMember(varName, memberName);
            }

            return val;
        }

        public bool VarExists(string name)
        {
            if (name.IndexOf('.') > 0)
            {
                var v = name.Split('.');
                name = v[0];
            }

            return this.engine.VarExists(name);
        }

        public string[] GetVariableNames()
        {
            return this.engine.GetVariableNames().ToArray();
        }
    }
}

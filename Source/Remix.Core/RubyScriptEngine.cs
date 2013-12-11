// -----------------------------------------------------------------------
// <copyright file="RubyScriptEngine.cs" company="Kennedy Space Center">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Atlana
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Scripting.Hosting;
    using IronRuby;
    using Atlana.Log;
    using System.Reflection;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public sealed class RubyScriptEngine : IScriptEngine
    {
        #region "Static Members"

        public static readonly string EngineName = "IronRuby Engine";

        internal static readonly object UniqueIdentityLock = new object();

        internal static volatile int _lastUniqueId = 0;

        internal static int GetUniqueID()
        {
            var uid = 0;
            lock (RubyScriptEngine.UniqueIdentityLock)
            {
                uid = ++RubyScriptEngine._lastUniqueId;
            }

            return uid;
        }

        #endregion // "Static Members"

        #region "Private Fields"

        private ScriptEngine engine = null;

        private ScriptScope baseScope = null;

        private int uid = 0;

        #endregion // "Private Fields"

        #region "Constructors"

        public RubyScriptEngine()
        {
            this.engine = Ruby.CreateEngine();
            this.uid = RubyScriptEngine.GetUniqueID();
            this.engine.Runtime.LoadAssembly(Assembly.GetExecutingAssembly());
            this.baseScope = this.engine.Runtime.CreateScope();
        }

        #endregion // "Constructors"

        public string Name
        {
            get 
            {
                return RubyScriptEngine.EngineName;
            }
        }

        public dynamic GetVariable(string name)
        {
            if (this.VarExists(name))
            {
                return this.baseScope.GetVariable(name);
            }

            return null;
        }

        public dynamic Execute(string expression)
        {
            dynamic d = null;
            try
            {
                d = this.engine.Execute(expression, this.baseScope);
            }
            catch (Exception e)
            {
                Logger.Bug(e);
            }

            return d;
        }

        public void SetVariable(string name, object value)
        {
            this.baseScope.SetVariable(name, value);
        }

        public bool VarExists(string name)
        {
            return this.baseScope.ContainsVariable(name);
        }

        public dynamic GetVariableMember(string varName, string memberName)
        {
            dynamic val = null;
            if (this.VarExists(varName))
            {
                var handle = this.baseScope.GetVariableHandle(varName);
                if (handle != null)
                {
                    val = this.engine.Operations.GetMember(varName, memberName);
                }
            }

            return val;
        }

        public IEnumerable<string> GetVariableNames()
        {
            return this.baseScope.GetVariableNames();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is RubyScriptEngine && obj.GetHashCode() == this.GetHashCode())
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return String.Format("{0}#{1}", this.Name, this.uid);
        }

        public override int GetHashCode()
        {
            return this.uid;
        }
    }
}

// -----------------------------------------------------------------------
// <copyright file="IScriptEngine.cs" company="Kennedy Space Center">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Atlana
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IScriptEngine
    {
        string Name
        {
            get;
        }

        dynamic GetVariable(string varName);

        dynamic Execute(string expression);

        void SetVariable(string varName, object value);

        bool VarExists(string varName);

        dynamic GetVariableMember(string varName, string memberName);

        IEnumerable<string> GetVariableNames();
    }
}

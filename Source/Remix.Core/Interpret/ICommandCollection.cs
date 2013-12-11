/*
 * Created by SharpDevelop.
 * User: Brendan
 * Date: 7/16/2009
 * Time: 6:08 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Atlana.Interpret
{
    /// <summary>
    /// Description of ICommandCollection.
    /// </summary>
    public interface ICommandCollection
    {
        string[] Commands{get;}
        Atlana.Interpret.CommandHandler GetCommand(string id);
    }
}

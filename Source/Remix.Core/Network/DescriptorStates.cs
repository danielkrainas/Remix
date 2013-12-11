using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlana.Network
{
    public enum DescriptorStates
    {
        Closing = -100, 
        AskName, 
        AskPassword, 
        AskNewName, 
        AskNewPassword, 
        ConfirmPassword, 
        PressEnter, 
        Playing = 0
    }
}

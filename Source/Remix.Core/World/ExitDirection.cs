using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Atlana.World;

namespace Atlana.World
{
    [ComplexType]
    public class ExitDirection
    {
        private ExitDirection()
            : this(default(ExitDirections))
        { }

        public ExitDirection(int value)
        {
            this.Value = value;
        }

        public ExitDirection(ExitDirections value)
        {
            this.Value = (int)value;
        }

        public static implicit operator int(ExitDirection value)
        {
            return value.Value;
        }

        public static implicit operator ExitDirection(int value)
        {
            return new ExitDirection(value);
        }

        public static implicit operator ExitDirections(ExitDirection value)
        {
            return (ExitDirections)value.Value;
        }

        public static implicit operator ExitDirection(ExitDirections value)
        {
            return new ExitDirection(value);
        }

        public int Value
        {
            get;
            set;
        }
    }
}

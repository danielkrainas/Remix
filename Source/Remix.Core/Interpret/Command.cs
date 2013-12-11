namespace Atlana.Interpret
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.DataAnnotations;

    public class Command
    {
        public int Id
        {
            get;
            set;
        }

        [StringLength(80)]
        public string ExampleUsage
        {
            get;
            set;
        }

        [StringLength(30)]
        public string Name
        {
            get;
            set;
        }

        public bool IsDisabled
        {
            get;
            set;
        }

        [StringLength(30)]
        public string ModuleGroup
        {
            get;
            set;
        }

        public DateTime DateCreated
        {
            get;
            set;
        }

        public DateTime LastModified
        {
            get;
            set;
        }

        public string Script
        {
            get;
            set;
        }

        public ICollection<Role> Roles
        {
            get;
            set;
        }

        [NotMapped]
        public ICommandExecuter ExecutionHandler
        {
            get;
            set;
        }

        public bool CanExecute(Mobile m)
        {
            if (m.Roles.Intersect(this.Roles).Any())
            {
                return true;
            }

            return false;
        }
    }
}

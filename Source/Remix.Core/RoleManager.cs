using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlana.Data;

namespace Atlana
{
    public class RoleManager
    {
        private static RoleManager _current;

        public static RoleManager Current
        {
            get
            {
                if (RoleManager._current == null)
                {
                    RoleManager._current = new RoleManager();
                }

                return RoleManager._current;
            }
        }

        private readonly List<Role> roles;

        private RoleManager()
        {
            this.roles = new List<Role>();
        }

        public Role GetRole(string name)
        {
            return this.roles.FirstOrDefault(r => r.Name == name);
        }

        public Role GetRole(int id)
        {
            return this.roles.FirstOrDefault(r => r.Id == id);
        }

        public void LoadRoles()
        {
            using (var roleRepo = new Repository<Role>())
            {
                this.roles.AddRange(roleRepo.All());
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlana.World;
using System.Data.Entity;
using System.Linq.Expressions;

namespace Atlana.Data
{
    public class AreaRepository : Repository<Area>
    {
        private readonly bool loadGraph;

        public AreaRepository(DataContext context, bool loadGraph = false)
            : base(context)
        {
            this.loadGraph = loadGraph;
        }

        public AreaRepository(bool loadGraph = false)
            : base()
        {
            this.loadGraph = loadGraph;
        }

        public override IQueryable<Area> All()
        {
            if (this.loadGraph)
            {
                return this.Set.Include(a => a.Rooms).Include(a => a.Rooms.Select(r => r.Exits)).AsQueryable();
            }

            return base.All();
        }
    }
}

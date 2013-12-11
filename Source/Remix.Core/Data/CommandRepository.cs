namespace Atlana.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Atlana.Interpret;
    using System.Data.Entity;

    public class CommandRepository : Repository<Command>
    {
        private readonly bool loadGraph;

        public CommandRepository(DataContext context, bool loadGraph = false)
            : base(context)
        {
            this.loadGraph = loadGraph;
        }

        public CommandRepository(bool loadGraph = false)
            : base()
        {
            this.loadGraph = loadGraph;
        }

        public override IQueryable<Command> All()
        {
            if (this.loadGraph)
            {
                return this.Set.Include(c => c.Roles).AsQueryable();
            }

            return base.All();
        }
    }
}

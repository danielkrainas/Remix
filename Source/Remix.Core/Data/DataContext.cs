namespace Atlana.Data
{
    using System.Data.Entity;
    using Atlana.Interpret;
    using Atlana.World;
    using Atlana.Help;

    public class DataContext : DbContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.ComplexType<ExitDirection>()
                .Property(o => o.Value)
                .HasColumnName("ExitDirectionId");

            modelBuilder.Entity<Room>()
                .HasRequired(r => r.Area)
                .WithMany(a => a.Rooms)
                .HasForeignKey(r => r.AreaId);

            modelBuilder.Entity<RoomExit>()
                .HasRequired(e => e.Room)
                .WithMany(r => r.Exits)
                .HasForeignKey(e => e.RoomId);

            //modelBuilder.Entity<RoomExit>()
            //    .HasRequired(e => e.Direction)
            //    .WithMany(d => d.Exits)
            //    .HasForeignKey(e => e.Direction);

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Commands)
                .WithMany(c => c.Roles)
                .Map(m =>
                {
                    m.ToTable("CommandRoles");
                    m.MapRightKey("CommandId");
                    m.MapLeftKey("RoleId");
                });

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Players)
                .WithMany(p => p.Roles)
                .Map(m =>
                {
                    m.ToTable("PlayerRoles");
                    m.MapRightKey("PlayerId");
                    m.MapLeftKey("RoleId");
                });
                
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Command> Commands
        {
            get;
            set;
        }

        public DbSet<Room> Rooms
        {
            get;
            set;
        }

        public DbSet<Area> Areas
        {
            get;
            set;
        }

        public DbSet<RoomExit> RoomExits
        {
            get;
            set;
        }

        public DbSet<HelpArticle> HelpArticles
        {
            get;
            set;
        }

        public DbSet<Role> Roles
        {
            get;
            set;
        }

        public DbSet<Player> Players
        {
            get;
            set;
        }
    }
}

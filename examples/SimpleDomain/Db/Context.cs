using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SimpleDomain.Db.Model;

namespace SimpleDomain.Db
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

		public DbSet<User> Users { get; set; }
		public DbSet<Department> Departments { get; set; }
		public DbSet<Organization> Organizations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
    
            // V1
            modelBuilder.Entity<User>(e => e.ToTable("Users"));
            modelBuilder.Entity<Department>(e => e.ToTable("Departments"));
       
            modelBuilder.Entity<User>()
                .HasOne(e => e.Department)
                .WithMany(e => e.Users)
                .HasForeignKey(e => e.DepartmentId);

            // V2
            modelBuilder.Entity<Organization>(e => e.ToTable("Organizations"));

            modelBuilder.Entity<Department>()
                .HasOne(e=>e.Organization)
                .WithMany(e=>e.Departments)
                .HasForeignKey(e => e.OrganizationId);

        }

    }
}
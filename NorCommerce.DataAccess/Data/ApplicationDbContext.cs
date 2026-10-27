using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NorCommerce.Models;

namespace NorCommerce.DataAccess.Data
{
    // The ApplicationDbContext class is the main database context for the application.
    // It inherits from IdentityDbContext to integrate ASP.NET Core Identity features.
    // While changing the behaivour, we need everything to be in sync with the IdentityDbContext,
    // so we need to use the same generic parameters as IdentityDbContext.
    // Changes: ApplicationUser, ApplicationUserRole, ApplicationRole
    public class ApplicationDbContext : IdentityDbContext<
    ApplicationUser, ApplicationRole, string,
    IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>,
    IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ShoppingCart> ShoppingCarts { get; set; } = null!;
        public DbSet<OrderHeader> OrderHeaders { get; set; } = null!;
        public DbSet<OrderDetails> OrderDetails { get; set; } = null!;
        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;
        public DbSet<ApplicationRole> ApplicationRoles { get; set; } = null!;
        public DbSet<ApplicationUserRole> ApplicationUserRole { get; set; } = null!;

        // Data seeding for Categories
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Here we specify the relationships between ApplicationUser, ApplicationRole, and ApplicationUserRole.
            // User can have many roles, and a role can be assigned to many users, hence the many-to-many relationship.
            modelBuilder.Entity<ApplicationUserRole>(ur =>
            {
                ur.HasOne(x => x.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(x => x.UserId);

                ur.HasOne(x => x.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(x => x.RoleId);
            });
        }
    }
}

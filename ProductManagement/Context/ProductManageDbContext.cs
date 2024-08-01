using Microsoft.EntityFrameworkCore;
using ProductManagement.Entity;
using ProductManagement.Interceptor;
using System.Reflection;

namespace ProductManagement.Context
{
    public class ProductManageDbContext : DbContext
    {
        private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;
        public ProductManageDbContext(DbContextOptions<ProductManageDbContext> options,
            AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor) 
            : base(options) 
        {
            _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductAudit> ProductAudits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "zakariyo",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("@Salom$123#"),
                    Role = "admin"
                },
                new User
                {
                    Id = 2,
                    Username = "yusuf",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("%Imzo*7$"),
                    Role = "user"
                }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Title = "Ko'ylak",
                    Quantity = 10,
                    Price = 15.00M
                },
                new Product
                {
                    Id = 2,
                    Title = "Parda",
                    Quantity = 20,
                    Price = 25.00M
                }
            );

            modelBuilder.Entity<ProductAudit>().ToTable("product_audit");

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                 .SetBasePath(AppContext.BaseDirectory)
                 .AddJsonFile("appsettings.json")
                 .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseNpgsql(connectionString)
                          .AddInterceptors(_auditableEntitySaveChangesInterceptor);
        }

    }
}

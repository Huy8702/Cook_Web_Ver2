using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CookWeb_Ver2.Models;

namespace CookWeb_Ver2.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        //câu lệnh này đảm bảo rằng khi ApplicationDbContext được tạo ra, nó sẽ có các cấu hình kết nối cơ sở dữ liệu được chỉ định từ các thiết lập trong dịch vụ của ứng dụng ASP.NET Core MVC.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<MachineType> MachineTypes { get; set; }
        public DbSet<Measuarement> Measuarements { get; set; }
        public DbSet<Recipes> Recipes { get; set; }
        public DbSet<RecipesDetail> RecipesDetails { get; set; }
        public DbSet<RolePermissions> RolePermissions { get; set; }
        public DbSet<StepsMakeRecipes> StepsMakeRecipes { get; set; }
        public DbSet<CookWeb_Ver2.Data.Role> Role { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<RolesSystem> RolesSystem { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ErrorList> ErrorLists { get; set; }
        //public virtual Task<int> SaveChangesAsycn()
        //{
        //    return Task.FromResult(0);
        //}
        internal Task SaveChangesAsycn()
        {
            throw new NotImplementedException();
        }

        ////Automatic make an initial database
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    builder.Entity<IdentityRole>.to;
        //    //modelBuilder.Entity<MachineType>().HasData(
        //    //    new MachineType { Name = "Stir-fry machine", Description = "Kitchen for stir-frying" },
        //    //    new MachineType { Name = "Fry machine", Description = "Kitchen for frying" },
        //    //    new MachineType { Name = "Grill machine", Description = "Kitchen for stir-frying" }
        //    //);
        //}
    }
}

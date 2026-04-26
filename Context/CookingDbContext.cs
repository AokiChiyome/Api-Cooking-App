using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using Test_Api.Models;

namespace Test_Api.Context
{
    public class CookingDbContext : DbContext
    {
        public CookingDbContext(DbContextOptions<CookingDbContext> options) : base(options) { }

        public DbSet<Account> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DishApproval> DishApprovals { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Favorite> Favorites { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().ToTable("Account");
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Dish>().ToTable("Dishes");
            modelBuilder.Entity<DishApproval>().ToTable("DishApprovals");
            modelBuilder.Entity<Comment>().ToTable("Comments");
            modelBuilder.Entity<Favorite>().ToTable("Favorites");



            modelBuilder.Entity<Dish>()
           .HasOne(d => d.Category)
           .WithMany(c => c.Dishes)
           .HasForeignKey(d => d.CategoryId)
           .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Dish>()
            .HasOne(d => d.Account)
            .WithMany(a => a.Dishes)
            .HasForeignKey(d => d.AccountId)
            .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<DishApproval>()
           .HasOne(da => da.Dish) 
           .WithMany() 
           .HasForeignKey(da => da.DishId) 
           .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Comment>()
            .HasOne(c => c.Account)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
            .HasOne(c => c.Dish)
            .WithMany()
            .HasForeignKey(c => c.DishId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
             .HasOne(f => f.Account) 
             .WithMany(a => a.Favorites) 
             .HasForeignKey(f => f.AccountId);

            modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Dish) 
            .WithMany() 
            .HasForeignKey(f => f.DishId);


        }
    }
}

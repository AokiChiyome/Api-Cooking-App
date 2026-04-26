using Microsoft.EntityFrameworkCore;
using Test_Api.Models;
namespace Test_Api.Context

{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleUser> RoleUsers { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupUser> GroupUsers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<RoleUser>().ToTable("RoleUsers");
            modelBuilder.Entity<Group>().ToTable("Groups");
            modelBuilder.Entity<GroupUser>().ToTable("GroupsUsers");
            modelBuilder.Entity<GroupUser>().HasKey(gu => new { gu.GroupId, gu.UserId });
            modelBuilder.Entity<RoleUser>().HasKey(ru => new { ru.UserId, ru.RoleId });

            modelBuilder.Entity<GroupUser>()
            .HasOne(gu => gu.group)
            .WithMany(g => g.GroupUser)
            .HasForeignKey(gu => gu.GroupId);

            modelBuilder.Entity<GroupUser>()
                .HasOne(gu => gu.user)
                .WithMany(g => g.GroupUser)
                .HasForeignKey(gu => gu.UserId);

            modelBuilder.Entity<RoleUser>()
                .HasOne(ru => ru.user)
                .WithMany(u => u.RoleUser)
                .HasForeignKey(ru => ru.UserId);

            modelBuilder.Entity<RoleUser>()
                .HasOne(ru => ru.role)
                .WithMany(r => r.RoleUser)
                .HasForeignKey(ru => ru.RoleId);

            modelBuilder.Entity<GroupUser>()
           .HasKey(gu => new { gu.GroupId, gu.UserId });

            modelBuilder.Entity<GroupUser>()
            .HasOne(gu => gu.user)
            .WithMany(u => u.GroupUser)
            .HasForeignKey(gu => gu.UserId);

        }
    }
}

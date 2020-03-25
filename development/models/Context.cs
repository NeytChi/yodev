using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Models;
using Models.AdminPanel;

namespace Common
{
    public partial class Context : DbContext
    {
        private bool useInMemoryDatabase = false;
        private bool useConfiguration = false;
        public Context()
        {

        }
        public Context(bool useInMemoryDatabase)
        {
            this.useInMemoryDatabase = useInMemoryDatabase;
            this.useConfiguration = true;
        }
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<BlogPost> BlogPosts { get; set; }
        public virtual DbSet<LendPiece> LendPieces { get; set; }
        
         protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (useInMemoryDatabase)
                optionsBuilder.UseInMemoryDatabase("yodev");
            optionsBuilder.EnableSensitiveDataLogging();
            if (useConfiguration) {
                if (!optionsBuilder.IsConfigured) {
                    optionsBuilder.UseMySql(databaseConnection());
                }
            }
        }
        public static string databaseConnection()
        {
            var sqlConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("database.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"database.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", 
                    optional: true, reloadOnChange: false)
                .Build();
            return "Server=" + sqlConfig.GetValue<string>("Server") +
                ";Database=" + sqlConfig.GetValue<string>("Database") + 
                ";User=" + sqlConfig.GetValue<string>("User") + 
                ";Pwd=" + sqlConfig.GetValue<string>("Password") + 
                ";Charset=utf8;";
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("admins");

                entity.HasKey(admin => admin.adminId)
                    .HasName("PRIMARY");

                entity.Property(admin => admin.adminId)
                    .HasColumnName("admin_id")
                    .HasColumnType("int(11)");

                entity.Property(admin => admin.adminEmail)
                    .HasColumnName("admin_email")
                    .HasColumnType("varchar(255)");
                
                entity.Property(admin => admin.adminFullname)
                    .HasColumnName("admin_fullname")
                    .HasColumnType("varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci");
                
                entity.Property(admin => admin.adminRole)
                    .HasColumnName("admin_role")
                    .HasColumnType("varchar(100)");
                    
                entity.Property(admin => admin.adminPassword)
                    .HasColumnName("admin_password")
                    .HasColumnType("varchar(255)");

                entity.Property(admin => admin.passwordToken)
                    .HasColumnName("password_token")
                    .HasColumnType("varchar(10)");
                
                entity.Property(admin => admin.createdAt)
                    .HasColumnName("created_at")
                    .HasColumnType("bigint(20)");
                
                entity.Property(admin => admin.lastLoginAt)
                    .HasColumnName("last_login_at")
                    .HasColumnType("bigint(20)");
                
                entity.Property(admin => admin.recoveryCode)
                    .HasColumnName("recovery_code")
                    .HasColumnType("int(11)");

                entity.Property(admin => admin.deleted)
                    .HasColumnName("deleted")
                    .HasColumnType("boolean");
            });
            modelBuilder.Entity<LendPiece>(entity =>
            {
                entity.ToTable("lend_pieces");

                entity.HasKey(lend => lend.lendId)
                    .HasName("PRIMARY");

                entity.Property(lend => lend.lendId)
                    .HasColumnName("lend_id")
                    .HasColumnType("int(11)");

                entity.Property(lend => lend.lendBody)
                    .HasColumnName("post_title")
                    .HasColumnType("text CHARACTER SET utf8 COLLATE utf8_general_ci");
                
                entity.Property(lend => lend.createdAt)
                    .HasColumnName("created_at")
                    .HasColumnType("bigint(20)");
                
                entity.Property(lend => lend.deleted)
                    .HasColumnName("deleted")
                    .HasColumnType("boolean");
            });
            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.ToTable("blog_posts");

                entity.HasKey(post => post.postId)
                    .HasName("PRIMARY");

                entity.Property(post => post.postId)
                    .HasColumnName("post_id")
                    .HasColumnType("int(11)");

                entity.Property(post => post.postImageUrl)
                    .HasColumnName("post_image_url")
                    .HasColumnType("varchar(255)");
                
                entity.Property(post => post.postTitle)
                    .HasColumnName("post_title")
                    .HasColumnType("text CHARACTER SET utf8 COLLATE utf8_general_ci");
                
                entity.Property(post => post.postBody)
                    .HasColumnName("post_body")
                    .HasColumnType("text CHARACTER SET utf8 COLLATE utf8_general_ci");
                
                entity.Property(post => post.createdAt)
                    .HasColumnName("created_at")
                    .HasColumnType("bigint(20)");
                
                entity.Property(post => post.deleted)
                    .HasColumnName("deleted")
                    .HasColumnType("boolean");
            });
        }
    }
}

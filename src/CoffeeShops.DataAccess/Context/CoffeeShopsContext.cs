using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoffeeShops.DataAccess.Models;

using Microsoft.EntityFrameworkCore.Metadata.Builders; 



namespace CoffeeShops.DataAccess.Context;

public class CoffeeShopsContext : DbContext
{
        public DbSet<UserDb> Users { get; set; }
        public DbSet<RoleDb> Roles { get; set; }
        public DbSet<DrinkDb> Drinks { get; set; }
        public DbSet<CategoryDb> Categories { get; set; }
        public DbSet<DrinksCategoryDb> DrinksCategories { get; set; }
        public DbSet<CompanyDb> Companies { get; set; }
        public DbSet<CoffeeShopDb> CoffeeShops { get; set; }
        public DbSet<MenuDb> Menus { get; set; }
        public DbSet<FavDrinksDb> FavDrinks{ get; set; }
        public DbSet<FavCoffeeShopsDb> FavCoffeeShops { get; set; }

    //public CoffeeShopsContext(DbContextOptions options) : base(options)
    //{
    //}
    public CoffeeShopsContext(DbContextOptions<CoffeeShopsContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            modelBuilder.Entity<RoleDb>(entity =>
            {
                entity.ToTable("roles");

                entity.HasKey(r => r.Id_role)
                      .HasName("id_role");

                entity.Property(r => r.Name)
                      .HasColumnName("name")
                      .HasColumnType("varchar(128)")
                      .IsRequired();

                entity.HasMany(r => r.Users) 
                      .WithOne(u => u.Role) 
                      .HasForeignKey(u => u.Id_role);
            });

            modelBuilder.Entity<UserDb>(entity =>
            {
                entity.ToTable("users"); 

                entity.HasKey(u => u.Id_user)
                      .HasName("id_user");

                entity.Property(u => u.Login)
                      .HasColumnName("login")
                      .HasColumnType("varchar(128)")
                      .IsRequired();

                entity.Property(u => u.Password)
                      .HasColumnName("password")
                      .HasColumnType("varchar(128)")
                      .IsRequired();

                entity.Property(u => u.BirthDate)
                .HasColumnName("birthdate")
                      .HasColumnType("date")
                      .IsRequired();

                entity.Property(u => u.Email)
                .HasColumnName("email")
                      .HasColumnType("varchar(256)")
                      .IsRequired();

                entity.HasOne(fcs => fcs.Role)
                      .WithMany(u => u.Users)
                      .HasForeignKey(fcs => fcs.Id_role);

                entity.HasMany(d => d.FavDrinks)
                  .WithOne(dc => dc.User)
                  .HasForeignKey(dc => dc.Id_user);

                entity.HasMany(d => d.FavCoffeeShops)
                  .WithOne(dc => dc.User)
                  .HasForeignKey(dc => dc.Id_user);
            });

        modelBuilder.Entity<DrinkDb>(entity =>
        {
            entity.ToTable("drinks");

            entity.HasKey(d => d.Id_drink)
                      .HasName("id_drink");
            entity.Property(d => d.Name)
                     .HasColumnName("name")
                     .HasColumnType("varchar(128)")
                     .IsRequired();

            entity.HasMany(d => d.FavDrinks)
                  .WithOne(fd => fd.Drink)
                  .HasForeignKey(fd => fd.Id_drink);

            entity.HasMany(d => d.DrinksCategory)
                  .WithOne(dc => dc.Drink)
                  .HasForeignKey(dc => dc.Id_drink);

        });

        modelBuilder.Entity<CategoryDb>(entity =>
        {
            entity.ToTable("categories");

            entity.HasKey(c => c.Id_category)
            .HasName("id_category");

            entity.Property(c => c.Name)
                .HasColumnName("name")
                .HasColumnType("varchar(128)")
                .IsRequired();

            entity.HasMany(c => c.DrinksCategory)
                  .WithOne(dc => dc.Category)
                  .HasForeignKey(dc => dc.Id_category);
        });

        modelBuilder.Entity<DrinksCategoryDb>(entity =>
        {
            entity.ToTable("drinkscategory");

            entity.HasKey(dc => new { dc.Id_drink, dc.Id_category });

            entity.HasOne(dc => dc.Drink)
                  .WithMany(d => d.DrinksCategory)
                  .HasForeignKey(dc => dc.Id_drink);

            entity.HasOne(dc => dc.Category)
                  .WithMany(c => c.DrinksCategory)
                  .HasForeignKey(dc => dc.Id_category);
        });

        modelBuilder.Entity<FavDrinksDb>(entity =>
        {
            entity.ToTable("favdrinks");

            entity.HasKey(fd => new { fd.Id_user, fd.Id_drink }); 


            entity.HasOne(fd => fd.User)
                  .WithMany(u => u.FavDrinks)
                  .HasForeignKey(fd => fd.Id_user);

            entity.HasOne(fd => fd.Drink)
                   .WithMany(u => u.FavDrinks)
                  .HasForeignKey(fd => fd.Id_drink);
        });


        modelBuilder.Entity<CoffeeShopDb>(entity =>
        {
            entity.ToTable("coffeeshops");

            entity.HasKey(cs => cs.Id_coffeeshop)
                  .HasName("id_coffeeshop");

            entity.Property(cs => cs.Id_company)
                  .HasColumnName("id_company")
                  .HasColumnType("uuid")
                  .IsRequired();

            entity.Property(cs => cs.Address)
                  .HasColumnName("address")
                  .HasColumnType("varchar(256)")
                  .IsRequired();

            entity.Property(cs => cs.WorkingHours)
                  .HasColumnType("varchar(64)")
                  .IsRequired();

            entity.HasOne(cs => cs.Company)
                  .WithMany(cs => cs.CoffeeShops)
                  .HasForeignKey(cs => cs.Id_company);

            entity.HasMany(cs => cs.FavCoffeeShops)
                  .WithOne(fcs => fcs.CoffeeShop)
                  .HasForeignKey(fcs => fcs.Id_coffeeshop);
        });

        modelBuilder.Entity<FavCoffeeShopsDb>(entity =>
        {
            entity.ToTable("favcoffeeshops");

            entity.HasKey(fcs => new { fcs.Id_user, fcs.Id_coffeeshop });

            entity.HasOne(fcs => fcs.User)
                  .WithMany(u => u.FavCoffeeShops)
                  .HasForeignKey(fcs => fcs.Id_user);

            entity.HasOne(fcs => fcs.CoffeeShop)
                  .WithMany(u => u.FavCoffeeShops)
                  .HasForeignKey(fcs => fcs.Id_coffeeshop);
        });

        modelBuilder.Entity<CompanyDb>(entity =>
        {
            entity.ToTable("companies");

            entity.HasKey(c => c.Id_company)
            .HasName("id_company");

            entity.Property(c => c.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(128)")
            .IsRequired();


            entity.Property(c => c.Website)
            .HasColumnName("website")
            .HasColumnType("varchar(256)");

            entity.HasMany(c => c.CoffeeShops)
                .WithOne(cs => cs.Company)
                .HasForeignKey(cs => cs.Id_company);
        });


        modelBuilder.Entity<MenuDb>(entity =>
        {
            entity.ToTable("menu");

            entity.HasKey(menu => new { menu.Id_drink, menu.Id_company });


            entity.Property(c => c.Size)
           .HasColumnName("size").HasColumnType("int"); 

            entity.Property(c => c.Price)
           .HasColumnName("price").HasColumnType("numeric(10,2)"); 

            entity.HasOne(fd => fd.Drink)
                  .WithMany(u => u.Menu)
                  .HasForeignKey(fd => fd.Id_drink);

            entity.HasOne(fd => fd.Company)
                   .WithMany(u => u.Menu)
                  .HasForeignKey(fd => fd.Id_company);
        });
    }





}

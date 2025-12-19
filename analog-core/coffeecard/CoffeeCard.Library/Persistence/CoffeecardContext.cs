using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoffeeCard.Common.Configuration;
using CoffeeCard.Models.DataTransferObjects.v2.Purchase;
using CoffeeCard.Models.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CoffeeCard.Library.Persistence
{
    public class CoffeeCardContext : DbContext
    {
        private readonly DatabaseSettings _databaseSettings;

        private readonly EnvironmentSettings _environmentSettings;

        public CoffeeCardContext(
            DbContextOptions<CoffeeCardContext> options,
            DatabaseSettings databaseSettings,
            EnvironmentSettings environmentSettings
        )
            : base(options)
        {
            _databaseSettings = databaseSettings;
            _environmentSettings = environmentSettings;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PosPurhase> PosPurchases { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<Programme> Programmes { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Statistic> Statistics { get; set; }
        public DbSet<ProductUserGroup> ProductUserGroups { get; set; }
        public DbSet<WebhookConfiguration> WebhookConfigurations { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuItemProduct> MenuItemProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(_databaseSettings.SchemaName);

            // Setup PUG compound primary key
            modelBuilder
                .Entity<ProductUserGroup>()
                .HasKey(pug => new { pug.ProductId, pug.UserGroup });

            modelBuilder
                .Entity<MenuItemProduct>()
                .HasKey(mip => new { mip.MenuItemId, mip.ProductId });

            modelBuilder
                .Entity<MenuItem>()
                .HasMany(mi => mi.AssociatedProducts)
                .WithMany(p => p.EligibleMenuItems)
                .UsingEntity<MenuItemProduct>();

            // Use Enum to Int for UserGroups
            var userGroupIntConverter = new EnumToNumberConverter<UserGroup, int>();
            // Use Enum to String for PurchaseTypes
            var purchaseTypeStringConverter = new EnumToStringConverter<PurchaseType>();

            var tokenTypeStringConverter = new EnumToStringConverter<TokenType>();

            modelBuilder
                .Entity<User>()
                .Property(u => u.UserGroup)
                .HasConversion(userGroupIntConverter);

            modelBuilder
                .Entity<Purchase>()
                .Property(p => p.Type)
                .HasConversion(purchaseTypeStringConverter);

            modelBuilder.Entity<User>().Property(u => u.UserState).HasConversion<string>();

            modelBuilder
                .Entity<ProductUserGroup>()
                .Property(pug => pug.UserGroup)
                .HasConversion(userGroupIntConverter);

            modelBuilder
                .Entity<WebhookConfiguration>()
                .Property(w => w.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Purchase>().Property(p => p.Status).HasConversion<string>();

            modelBuilder
                .Entity<Ticket>()
                .HasOne(t => t.Owner)
                .WithMany(u => u.Tickets)
                .HasForeignKey(t => t.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder
                .Entity<Token>()
                .Property(t => t.Type)
                .HasConversion(tokenTypeStringConverter);

            modelBuilder
                .Entity<Token>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Token>().Property(t => t.Expires).IsRequired();

            modelBuilder.Entity<Token>().Property(t => t.TokenHash).IsRequired();

            modelBuilder
                .HasDbFunction(typeof(Token).GetMethod(nameof(Token.Expired)))
                .HasSchema("dbo")
                .HasName("Expired");

            if (!Database.IsRelational())
                NonRelationalSeedData(modelBuilder);
        }

        private void NonRelationalSeedData(ModelBuilder modelBuilder)
        {
            StreamReader reader;

            reader = new StreamReader("SeedData/Programmes_202402131759.csv");
            reader.ReadLine();
            modelBuilder
                .Entity<Programme>()
                .HasData(
                    reader
                        .ReadToEnd()
                        .Split('\n')
                        .Where(line => line != "")
                        .Select(line =>
                        {
                            return new Programme
                            {
                                Id = int.Parse(line.Split(',')[0]),
                                ShortName = line.Split(',')[1],
                                FullName = line.Split(',')[2],
                                SortPriority = int.Parse(line.Split(',')[3]),
                            };
                        })
                );
            reader.Close();

            reader = new StreamReader("SeedData/ProductUserGroups_202402131759.csv");
            reader.ReadLine();
            modelBuilder
                .Entity<ProductUserGroup>()
                .HasData(
                    reader
                        .ReadToEnd()
                        .Split('\n')
                        .Where(line => line != "")
                        .Select(line =>
                        {
                            return new ProductUserGroup
                            {
                                ProductId = int.Parse(line.Split(',')[0]),
                                UserGroup = UserGroupExtention.fromInt(
                                    int.Parse(line.Split(',')[1])
                                ),
                            };
                        })
                );
            reader.Close();

            reader = new StreamReader("SeedData/MenuItemProducts_202402131759.csv");
            reader.ReadLine();
            modelBuilder
                .Entity<MenuItemProduct>()
                .HasData(
                    reader
                        .ReadToEnd()
                        .Split('\n')
                        .Where(line => line != "")
                        .Select(line =>
                        {
                            return new MenuItemProduct
                            {
                                MenuItemId = int.Parse(line.Split(',')[0]),
                                ProductId = int.Parse(line.Split(',')[1]),
                            };
                        })
                );
            reader.Close();

            reader = new StreamReader("SeedData/MenuItems_202402131759.csv");
            reader.ReadLine();
            modelBuilder
                .Entity<MenuItem>()
                .HasData(
                    reader
                        .ReadToEnd()
                        .Split('\n')
                        .Where(line => line != "")
                        .Select(line =>
                        {
                            return new MenuItem
                            {
                                Id = int.Parse(line.Split(',')[0]),
                                Name = line.Split(',')[1],
                            };
                        })
                );
            reader.Close();

            reader = new StreamReader("SeedData/Users_202402131759.csv");
            reader.ReadLine();
            modelBuilder
                .Entity<User>()
                .HasData(
                    reader
                        .ReadToEnd()
                        .Split('\n')
                        .Where(line => line != "")
                        .Select(line => line.Replace('"', '\0'))
                        .Select(line =>
                        {
                            return new User
                            {
                                Id = int.Parse(line.Split(',')[0]),
                                Email = line.Split(',')[1],
                                Name = line.Split(',')[2],
                                Password = line.Split(',')[3],
                                Salt = line.Split(',')[4],
                                Experience = int.Parse(line.Split(',')[5]),
                                DateCreated = DateTime.Parse(line.Split(',')[6]),
                                DateUpdated = DateTime.Parse(line.Split(',')[7]),
                                IsVerified = line.Split(',')[8] == "1",
                                PrivacyActivated = line.Split(',')[9] == "1",
                                UserGroup = UserGroupExtention.fromInt(
                                    int.Parse(line.Split(',')[10])
                                ),
                                UserState = line.Split(',')[11] switch
                                {
                                    "Deleted" => UserState.Deleted,
                                    "Active" => UserState.Active,
                                    _ => UserState.PendingActivition,
                                },
                                ProgrammeId = int.Parse(line.Split(',')[12]),
                            };
                        })
                );
            reader.Close();

            reader = new StreamReader("SeedData/Tickets_202402131759.csv");
            reader.ReadLine();
            modelBuilder
                .Entity<Ticket>()
                .HasData(
                    reader
                        .ReadToEnd()
                        .Split('\n')
                        .Where(line => line != "")
                        .Select(line => line.Replace('"', '\0'))
                        .Select(line =>
                        {
                            var split = line.Split(',');
                            int UsedOnMenuItemId;
                            return new Ticket
                            {
                                Id = int.Parse(split[0]),
                                DateCreated = DateTime.Parse(split[1]),
                                DateUsed =
                                    split[2] != "" ? DateTime.Parse(split[2]) : null,
                                ProductId = int.Parse(split[3]),
                                Status = int.Parse(split[4]) switch
                                {
                                    0 => TicketStatus.Unused,
                                    1 => TicketStatus.Used,
                                    2 => TicketStatus.Refunded,
                                    _ => TicketStatus.Unused,
                                },
                                OwnerId = int.Parse(split[5]),
                                PurchaseId = int.Parse(split[6]),
                                UsedOnMenuItemId = int.TryParse(split[7], out UsedOnMenuItemId) ? UsedOnMenuItemId : null,
                            };
                        })
                );
            reader.Close();

            reader = new StreamReader("SeedData/Purchases_202402131759.csv");
            reader.ReadLine();
            modelBuilder
                .Entity<Purchase>()
                .HasData(
                    reader
                        .ReadToEnd()
                        .Split('\n')
                        .Where(line => line != "")
                        .Select(line => line.Replace('"', '\0'))
                        .Select(line =>
                        {
                            var split = line.Split(',');
                            return new Purchase
                            {
                                Id = int.Parse(line.Split(',')[0]),
                                ProductName = split[1],
                                ProductId = int.Parse(split[2]),
                                Price = int.Parse(split[3]),
                                NumberOfTickets = int.Parse(split[4]),
                                DateCreated = DateTime.Parse(split[5]),
                                OrderId = split[6],
                                ExternalTransactionId = split[7],
                                Status = split[8] switch
                                {
                                    "Cancelled" => PurchaseStatus.Cancelled,
                                    "Completed" => PurchaseStatus.Completed,
                                    "Expired" => PurchaseStatus.Expired,
                                    "PendingPayment" => PurchaseStatus.PendingPayment,
                                    "Refunded" => PurchaseStatus.Refunded,
                                    _ => PurchaseStatus.Cancelled,
                                },
                                PurchasedById = int.Parse(split[9]),
                                Type = split[9] switch
                                {
                                    "MobilePayV1" => PurchaseType.MobilePayV1,
                                    "MobilePayV2" => PurchaseType.MobilePayV2,
                                    "Free" => PurchaseType.Free,
                                    "PointOfSale" => PurchaseType.PointOfSale,
                                    "Voucher" => PurchaseType.Voucher,
                                    _ => PurchaseType.Free,
                                },
                            };
                        })
                );
            reader.Close();

            // Vouchers
            reader = new StreamReader("SeedData/Vouchers_202402131759.csv");
            reader.ReadLine();
            modelBuilder
                .Entity<Voucher>()
                .HasData(
                    reader
                        .ReadToEnd()
                        .Split('\n')
                        .Where(line => line != "")
                        .Select(line => line.Replace('"', '\0'))
                        .Select(line =>
                        {
                            // CSV format:
                            // "Id","Code","DateCreated","DateUsed","Description","Requester","Product_Id","User_Id","PurchaseId"
                            var split = line.Split(',');
                            int UserId, PurchaseId;

                            return new Voucher
                            {
                                Id = int.Parse(split[0]),
                                Code = split[1],
                                DateCreated = DateTime.Parse(split[2]),
                                DateUsed = split[3] != "" ? DateTime.Parse(split[3]) : null,
                                Description = split[4] != "" ? split[4] : null,
                                Requester = split[5] != "" ? split[5] : null,
                                ProductId = int.Parse(split[6]),
                                UserId = int.TryParse(split[7], out UserId) ? UserId : null,
                                PurchaseId = int.TryParse(split[8], out PurchaseId) ? PurchaseId : null,
                            };
                        })
                );
            reader.Close();

            // Statistics
            reader = new StreamReader("SeedData/_Statistics__202402131759.csv");
            reader.ReadLine();
            // CSV format:
            // "Id","Preset","SwipeCount","LastSwipe","ExpiryDate","User_Id"
            modelBuilder
                .Entity<Statistic>()
                .HasData(
                    reader
                        .ReadToEnd()
                        .Split('\n')
                        .Where(line => line != "")
                        .Select(line => line.Replace('"', '\0'))
                        .Select(line =>
                        {
                            var split = line.Split(',');
                            return new Statistic
                            {
                                Id = int.Parse(split[0]),
                                Preset = split[1] switch
                                {
                                    "0" => StatisticPreset.Monthly,
                                    "1" => StatisticPreset.Semester,
                                    "2" => StatisticPreset.Total,
                                    _ => throw new ArgumentException(),
                                },
                                SwipeCount = int.Parse(split[2]),
                                LastSwipe = DateTime.Parse(split[3]),
                                ExpiryDate = DateTime.Parse(split[4]),
                                UserId = int.Parse(split[5]),
                            };
                        })
                );
            reader.Close();

            // Products
            reader = new StreamReader("SeedData/Products_202402131759.csv");
            reader.ReadLine();
            // CSV format:
            // "Id","Price","NumberOfTickets","Name","Description","ExperienceWorth","Visible"
            modelBuilder
                .Entity<Product>()
                .HasData(
                    reader
                        .ReadToEnd()
                        .Split('\n')
                        .Where(line => line != "")
                        .Select(line => line.Replace('"', '\0'))
                        .Select(line =>
                        {
                            var split = line.Split(',');
                            return new Product
                            {
                                Id = int.Parse(split[0]),
                                Price = int.Parse(split[1]),
                                NumberOfTickets = int.Parse(split[2]),
                                Name = split[3],
                                Description = split[4],
                                ExperienceWorth = int.Parse(split[5]),
                                Visible = split[6] == "1",
                            };
                        })
                );
            reader.Close();

            // Tokens
            reader = new StreamReader("SeedData/Tokens_202402131759.csv");
            reader.ReadLine();
            // CSV format:
            // "Id","TokenHash","User_Id"
            modelBuilder
                .Entity<Token>()
                .HasData(
                    reader
                        .ReadToEnd()
                        .Split('\n')
                        .Where(line => line != "")
                        .Select(line => line.Replace('"', '\0'))
                        .Select(line =>
                        {
                            var split = line.Split(',');
                            int UserId;
                            return new Token(tokenHash: split[1], type: TokenType.Refresh)
                            {
                                Id = int.Parse(split[0]),
                                TokenHash = split[1],
                                UserId = int.TryParse(split[2], out UserId) ? UserId : null,
                            };
                        })
                );
            reader.Close();

            // PosPurchases
            reader = new StreamReader("SeedData/PosPurchases_202402131759.csv");
            reader.ReadLine();
            // CSV format:
            // "PurchaseId","BaristaInitials"
            modelBuilder
                .Entity<PosPurhase>()
                .HasData(
                    reader
                        .ReadToEnd()
                        .Split('\n')
                        .Where(line => line != "")
                        .Select(line => line.Replace('"', '\0'))
                        .Select(line =>
                        {
                            var split = line.Split(',');
                            return new PosPurhase
                            {
                                PurchaseId = int.Parse(split[0]),
                                Purchase = null!, // Will be set by EF Core
                                BaristaInitials = split[1],
                            };
                        })
                );
            reader.Close();

            // WebhookConfigurations
            reader = new StreamReader("SeedData/WebhookConfigurations_202402131759.csv");
            reader.ReadLine();
            // CSV format:
            // "Id","Url","SignatureKey","Status","LastUpdated"
            modelBuilder
                .Entity<WebhookConfiguration>()
                .HasData(
                    reader
                        .ReadToEnd()
                        .Split('\n')
                        .Where(line => line != "")
                        .Select(line => line.Replace('"', '\0'))
                        .Select(line =>
                        {
                            var split = line.Split(',');
                            return new WebhookConfiguration
                            {
                                Id = Guid.Parse(split[0]),
                                Url = split[1],
                                SignatureKey = split[2],
                                Status = split[3] switch
                                {
                                    "Active" => WebhookStatus.Active,
                                    "Disabled" => WebhookStatus.Disabled,
                                    _ => WebhookStatus.Disabled,
                                },
                                LastUpdated = DateTime.Parse(split[4]),
                            };
                        })
                );
        }
    }
}

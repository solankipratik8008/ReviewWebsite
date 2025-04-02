using Microsoft.EntityFrameworkCore;
using Project_of_Sharan.Models;
using System;
using System.IO;

namespace Project_of_Sharan.Data
{
    public class Project_of_SharanContext : DbContext
    {
        private readonly IWebHostEnvironment _env;

        public Project_of_SharanContext(DbContextOptions<Project_of_SharanContext> options, IWebHostEnvironment env)
            : base(options)
        {
            _env = env;
        }

        public DbSet<User> User { get; set; }
        public DbSet<Review> Review { get; set; }
        public DbSet<ReviewImage> ReviewImages { get; set; }

        private byte[]? ConvertImageToByteArray(string fileName)
        {
            var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
            if (File.Exists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }
            return null;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seeding Admin and User
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "ConestogaCollege", Password = "123", Role = "Admin" },
                new User { UserId = 2, Username = "user1", Password = "password", Role = "User" }
            );

            // Seeding Reviews
            modelBuilder.Entity<Review>().HasData(
                new Review
                {
                    ReviewId = 1,
                    UserId = 1,
                    CoffeeShopItemName = "Starbucks",
                    Rating = 5,
                    ReviewText = "Amazing coffee and great ambiance!",
                    DatePosted = new DateTime(2023, 10, 1)
                },
                new Review
                {
                    ReviewId = 2,
                    UserId = 2,
                    CoffeeShopItemName = "Tim Hortons",
                    Rating = 4,
                    ReviewText = "Decent coffee, quick service.",
                    DatePosted = new DateTime(2023, 10, 2)
                }
            );

            // Seeding Review Images
            modelBuilder.Entity<ReviewImage>().HasData(
                new ReviewImage
                {
                    ReviewImageId = 1,
                    ReviewId = 1,
                    ImageData = ConvertImageToByteArray("starbucks.jpeg")
                },
                new ReviewImage
                {
                    ReviewImageId = 2,
                    ReviewId = 2,
                    ImageData = ConvertImageToByteArray("timhortons.jpeg")
                }
            );
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Project_of_Sharan.Data;
using Project_of_Sharan.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Project_of_Sharan.Controllers
{
    public class HomeController : BaseController
    {
        private readonly Project_of_SharanContext _context;

        public HomeController(Project_of_SharanContext context)
        {
            _context = context;
        }

        // Index action to list all reviews
        public async Task<IActionResult> Index(string searchTerm)
        {
            // Fetch reviews and include associated user data
            var reviewsQuery = _context.Review.Include(r => r.User).AsQueryable();

            // If there is a search term, filter the reviews
            if (!string.IsNullOrEmpty(searchTerm))
            {
                reviewsQuery = reviewsQuery.Where(r =>
                    (r.CoffeeShopItemName != null && r.CoffeeShopItemName.Contains(searchTerm)) ||
                    (r.ReviewText != null && r.ReviewText.Contains(searchTerm)) ||
                    (r.User.Username != null && r.User.Username.Contains(searchTerm))
                );
            }

            // Fetch the reviews from the database asynchronously
            var reviews = await reviewsQuery
                .Where(r => r.DatePosted != null)  // Only include reviews with a non-null DatePosted
                .OrderByDescending(r => r.DatePosted)
                .ToListAsync();

            // Load images for each review
            foreach (var review in reviews)
            {
                var reviewImages = await _context.ReviewImages
                    .Where(ri => ri.ReviewId == review.ReviewId)
                    .ToListAsync();

                review.ReviewImages = reviewImages;
            }

            return View(reviews);
        }

        // Get action to display the review creation form
        [HttpGet]
        public IActionResult CreateReview()
        {
            return View();
        }

        // Post action to handle the review creation process
        [HttpPost]
        public async Task<IActionResult> CreateReview(Review review, IFormFile[] images)
        {
            var username = HttpContext.Request.Form["Username"];
            var password = HttpContext.Request.Form["Password"];

            // Check if the user exists in the database
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid login credentials.";
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                review.UserId = user.UserId;
                review.DatePosted = DateTime.Now;

                // Handle the image upload if images are provided
                if (images != null && images.Length > 0)
                {
                    foreach (var image in images)
                    {
                        var reviewImage = new ReviewImage
                        {
                            Review = review,
                            ImageData = ConvertImageToByteArray(image)
                        };

                        _context.ReviewImages.Add(reviewImage);
                    }
                }

                _context.Review.Add(review);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Review created successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "There was an error in creating the review. Please try again.";
            return View(review);
        }

        // Helper method to convert image to byte array
        private byte[]? ConvertImageToByteArray(IFormFile image)
        {
            if (image != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    image.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            return null;
        }

        // Action to handle review deletion (for admin or the review owner)
        [HttpPost]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var review = await _context.Review.FindAsync(reviewId);
            if (review == null)
            {
                TempData["ErrorMessage"] = "Review not found.";
                return RedirectToAction("Index");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || (review.UserId != userId.Value && !IsAdmin()))
            {
                TempData["ErrorMessage"] = "You are not authorized to delete this review.";
                return RedirectToAction("Index");
            }

            _context.Review.Remove(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Review deleted successfully!";
            return RedirectToAction("Index");
        }

        // Helper method to check if the current user is an admin
        private bool IsAdmin()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return false;
            }
            var user = _context.User.Find(userId.Value);
            return user != null && user.Role == "Admin";
        }
    }
}

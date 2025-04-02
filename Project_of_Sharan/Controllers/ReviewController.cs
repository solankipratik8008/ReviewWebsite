using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Project_of_Sharan.Data;
using Project_of_Sharan.Models;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Project_of_Sharan.Controllers
{
    public class ReviewController : Controller
    {
        private readonly Project_of_SharanContext _context;

        public ReviewController(Project_of_SharanContext context)
        {
            _context = context;
        }

        // Display reviews with optional search
        public async Task<IActionResult> Index(string searchTerm)
        {
            var reviewsQuery = _context.Review.Include(r => r.User).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                reviewsQuery = reviewsQuery.Where(r => r.CoffeeShopItemName.Contains(searchTerm) ||
                                                       r.ReviewText.Contains(searchTerm) ||
                                                       r.User.Username.Contains(searchTerm));
            }

            var reviews = await reviewsQuery.ToListAsync();

            foreach (var review in reviews)
            {
                review.ReviewImages = await _context.ReviewImages
                    .Where(ri => ri.ReviewId == review.ReviewId)
                    .ToListAsync();
            }

            ViewData["SweetAlertSuccessMessage"] = TempData["SuccessMessage"];
            ViewData["SweetAlertErrorMessage"] = TempData["ErrorMessage"];

            return View(reviews);
        }

        // Display the Add Review form
        [HttpGet]
        public IActionResult AddReview()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(Review review, List<IFormFile> Images)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _context.User.FindAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Account");

            review.UserId = userId.Value;
            review.Rating = ValidateRating(review.Rating);

            _context.Review.Add(review);
            await _context.SaveChangesAsync(); // Ensure ReviewId is generated

            if (Images != null && Images.Count > 0)
            {
                foreach (var image in Images)
                {
                    if (image.Length > 0)
                    {
                        if (image.Length > 5 * 1024 * 1024) // 5MB Limit
                        {
                            TempData["ErrorMessage"] = "Image size should be less than 5MB.";
                            return RedirectToAction("AddReview");
                        }

                        if (image.ContentType == "image/jpeg" || image.ContentType == "image/png")
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await image.CopyToAsync(memoryStream);
                                _context.ReviewImages.Add(new ReviewImage
                                {
                                    ReviewId = review.ReviewId,
                                    ImageData = memoryStream.ToArray()
                                });
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Invalid file type. Please upload a JPEG or PNG image.";
                            return RedirectToAction("AddReview");
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Review added successfully!";
            return RedirectToAction("Index");
        }

        // Admin review management
        [HttpGet]
        public async Task<IActionResult> AdminIndex()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var reviews = await _context.Review
                .Include(r => r.User)
                .Include(r => r.ReviewImages)
                .ToListAsync();

            return View(reviews);
        }

        // Display reviews submitted by the logged-in user
        [HttpGet]
        public async Task<IActionResult> MyReviews()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var userReviews = await _context.Review
                .Include(r => r.User)
                .Where(r => r.UserId == userId.Value)
                .ToListAsync();

            foreach (var review in userReviews)
            {
                review.ReviewImages = await _context.ReviewImages
                    .Where(ri => ri.ReviewId == review.ReviewId)
                    .ToListAsync();
            }

            ViewData["UserId"] = userId;
            return View(userReviews);
        }

        // Admin edits a review
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Review review)
        {
            if (id != review.ReviewId) return NotFound();

            var existingReview = await _context.Review.FindAsync(id);
            if (existingReview == null) return NotFound();

            try
            {
                existingReview.CoffeeShopItemName = review.CoffeeShopItemName;
                existingReview.ReviewText = review.ReviewText;
                existingReview.Rating = ValidateRating(review.Rating);

                _context.Update(existingReview);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Review updated successfully!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Error updating review. Please try again.";
            }

            return RedirectToAction("AdminIndex");
        }

        // User edits their own review
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var review = await _context.Review.FindAsync(id);
            int? loggedInUserId = HttpContext.Session.GetInt32("UserId");

            if (loggedInUserId == null || review == null || review.UserId != loggedInUserId.Value)
            {
                return RedirectToAction("MyReviews");
            }

            return View(review);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _context.Review.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            // Ensure Rating is set to a valid value if it's null
            if (review.Rating == null)
            {
                review.Rating = 1; // Default value
            }

            return View(review);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, Review review)
        {
            if (id != review.ReviewId) return NotFound();

            var existingReview = await _context.Review.FindAsync(id);
            if (existingReview == null) return NotFound();

            int? loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null || existingReview.UserId != loggedInUserId.Value)
            {
                return RedirectToAction("MyReviews");
            }

            existingReview.CoffeeShopItemName = review.CoffeeShopItemName;
            existingReview.ReviewText = review.ReviewText;
            existingReview.Rating = ValidateRating(review.Rating);

            _context.Update(existingReview);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your review has been updated successfully!";
            return RedirectToAction("MyReviews");
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Review.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            return View(review); // Optional step to show a confirmation view
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Review.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Review.Remove(review);
            await _context.SaveChangesAsync();

            // You can use SweetAlert messages here for success
            TempData["SweetAlertSuccessMessage"] = "Review deleted successfully!";
            return RedirectToAction(nameof(AdminIndex)); // Redirect to the reviews list or another page
        }



        // Helper function to ensure valid ratings
        private int ValidateRating(int rating)
        {
            return (rating >= 1 && rating <= 5) ? rating : 1;
        }
    }
}

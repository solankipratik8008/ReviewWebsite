namespace Project_of_Sharan.Models
{
    public class ReviewImage
    {
        public int ReviewImageId { get; set; }
        public int ReviewId { get; set; }
        public byte[] ImageData { get; set; }  // Image data as byte array

        public Review Review { get; set; }  // Navigation property to the Review
    }
}

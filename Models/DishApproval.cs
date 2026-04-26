namespace Test_Api.Models
{
    public class DishApproval
    {
        public int Id { get; set; }
        public int DishId { get; set; }
        public string? Status { get; set; } 
        public int? ApprovedBy { get; set; } 
        public string? Reason { get; set; }   
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Dish Dish { get; set; }
    }
    public class DishApprovalDto
    {
        public int DishId { get; set; }
        public string Status { get; set; }  
        public int ApprovedBy { get; set; }
        public string Reason { get; set; }  
    }


}

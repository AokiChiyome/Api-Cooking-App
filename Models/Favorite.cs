using System.ComponentModel.DataAnnotations.Schema;

namespace Test_Api.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public int AccountId { get; set; }  
        public int DishId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        [ForeignKey("AccountId")]
        public Account Account { get; set; }  

        [ForeignKey("DishId")]
        public Dish Dish { get; set; }
    }
    public class FavoriteDto
    {
        public int AccountId { get; set; }
        public int DishId { get; set; }
    }

}

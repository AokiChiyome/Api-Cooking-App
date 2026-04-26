using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Test_Api.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        [JsonIgnore]
        public ICollection<Dish> Dishes { get; set; }
    }
    public class CategoryRequest
    {
        public int Id { get; set; }
    }
    public class CategoryCreateFormDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IFormFile ImageFile { get; set; }
    }


}


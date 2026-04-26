namespace Test_Api.Models
{
    public class Dish
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; } 
        public string ImageUrl { get; set; }
        public string Ingredients { get; set; }
        public string Description { get; set; }
        public int AccountId { get; set; }   
        public Account Account { get; set; }
        public Category Category { get; set; }
    }
    public class DishCreateDto
    {
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; }
        public string Ingredients { get; set; }
        public string Description { get; set; }
        public int AccountId { get; set; }
    }
    public class DishEditDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; }
        public string Ingredients { get; set; }
        public string Description { get; set; }
    }
    public class DishDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; }
        public string Ingredients { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; } 
    }


    public class DishCreateFormDto
    {
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public IFormFile ImageFile { get; set; }
        public string Ingredients { get; set; }
        public string Description { get; set; }
        public int AccountId { get; set; }
    }



}


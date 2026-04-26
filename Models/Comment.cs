namespace Test_Api.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int DishId { get; set; }
        public int UserId { get; set; }
        public string CommentText { get; set; }
        public long Timestamp { get; set; }
        public Account Account { get; set; }
        public Dish Dish { get; set; }
    }
    public class AddCommentDto
    {
        public int DishId { get; set; }
        public int UserId { get; set; }
        public string CommentText { get; set; }
        public long Timestamp { get; set; }
    }

}

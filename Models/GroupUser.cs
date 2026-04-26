namespace Test_Api.Models
{
    public class GroupUser
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }

        public Group group { get; set; }

        public User user { get; set; }

    }
    public class AddUserToGroup
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }
    }

    public class DeleteUserInGroup
    {
        public int GroupId { get; set; }
        

        public List<int> UserIds { get; set; }

    }

}

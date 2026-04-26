namespace Test_Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string ten { get; set; }
        public int tuoi { get; set; }
        public IList<RoleUser> RoleUser { get; set; }
        public IList<GroupUser> GroupUser { get; set; }
    }
    

}

using System.Text.Json.Serialization;

namespace Test_Api.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public IList<GroupUser>? GroupUser { get; set; }

    }
    public class AddGroupRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> UserIds { get; set; }

    }

    public class UpdateGroupRequest
    {
        public string Name { get; set; }
        public List<int> UserIds { get; set; }
    }

}

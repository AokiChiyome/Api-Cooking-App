using System.Text.Json.Serialization;

namespace Test_Api.Models
{
    public class Role
    {
        public int Id { get; set; }

        public string Rolename { get; set; }

        [JsonIgnore]
        public IList<RoleUser>? RoleUser { get; set; }

    }
}

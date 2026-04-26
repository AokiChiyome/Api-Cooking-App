using System.Text.Json.Serialization;


namespace Test_Api.Models
{
    public class RoleUser
    {
        public int UserId { get; set; }
        [JsonIgnore]
        public User? user { get; set; }


        public int RoleId { get; set; }
        [JsonIgnore]
        public Role? role { get; set; }
    }
}

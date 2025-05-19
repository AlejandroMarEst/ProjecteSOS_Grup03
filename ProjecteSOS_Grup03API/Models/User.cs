using Microsoft.AspNetCore.Identity;

namespace ProjecteSOS_Grup03API.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
    }
}

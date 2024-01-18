using Microsoft.AspNetCore.Identity;

namespace Notes.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }

    }

}

using System.Collections.Generic;
using DocumentDb.Fluent;

namespace PodcastsSyndicate.Models
{
    public class User : HasId
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string AuthToken { get; set; }

        IEnumerable<string> PodcastIds { get; set; }
    }
}
using System.Collections.Generic;
using DocumentDb.Fluent;

namespace PodcastsSyndicate.Models
{
    public class User : HasId
    {
        // Id is Email.
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsDeveloper { get; set; } = false;

        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string AuthToken { get; set; }

        IEnumerable<string> PodcastIds { get; set; }
    }
}
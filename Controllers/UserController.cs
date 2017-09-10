using Microsoft.AspNetCore.Mvc;

namespace PodcastsSyndicate.Controllers
{
    public class UserController : BaseController
    {
        [HttpGet("/user")]
        public IActionResult Get()
        {
            if(User == null)
                return Forbid("Bearer");
                
            return Ok(User);
        }
    }
}
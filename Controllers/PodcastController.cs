using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PodcastsSyndicate.Controllers
{
    public class PodcastController : Controller
    {
        [HttpGet("/podcast/{podcastName}/name")]
        public IActionResult Get(string podcastName)
        {
            return Ok(podcastName);
        }
    }
}

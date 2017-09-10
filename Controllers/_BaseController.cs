using System;
using Microsoft.AspNetCore.Mvc;
using PodcastsSyndicate.Models;

namespace PodcastsSyndicate.Controllers
{
    public abstract class BaseController : Controller
    {
        protected new User User => Request.HttpContext.Items["User"] as User;
    }
}
using Microsoft.AspNetCore.Mvc;

namespace MDS.Controllers
{
    public class Reviewscontroller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

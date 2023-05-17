using MDS.Data;
using MDS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MDS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<HomeController> logger

            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _logger = logger;

        }

        public IActionResult Index()
        {


            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Tari");
            }

            var search = "";

            var tari = db.ListaTari.OrderBy(a => a.Nume).ToList();

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 
                List<int> tariid = db.ListaTari.Where(t => t.Nume.Contains(search))
                    .Select(a => a.Id).ToList();
                tari = db.ListaTari.Where(t => tariid.Contains(t.Id)).ToList();

            }
            ViewBag.SearchString = search;
            int _perPage = 8;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            int totalItems = tari.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var offset = 0;
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }
            var paginatedArticles = tari.Skip(offset).Take(_perPage);
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.Tari = paginatedArticles;
            if (search != "")
            {
                if (totalItems == 0)
                {
                    TempData["message"] = "Nu s-a gasit tara care contine cuvantul :" + search;
                    return RedirectToAction("Index");
                }

                ViewBag.PaginationBaseUrl = "/Tari/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Tari/Index/?page";
            }

            return View();
        }




        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
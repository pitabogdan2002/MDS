using MDS.Data;
using MDS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace MDS.Controllers
{
    [Authorize]

    public class RezervariController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        public RezervariController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            var rezervari = db.ListaRezervari.OrderByDescending(a => a.CheckIn).ThenByDescending(a => a.CheckOut)
                                          .Include("User");
            int totalItems = rezervari.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;
            int _perPage = 8;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedrezervari = rezervari.Skip(offset).Take(_perPage);
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.Rezervari = paginatedrezervari;

            if (totalItems == 0)
            {
                TempData["message"] = "Nu s-a gasit nicio rezervare";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Rezervari/Index/" + currentPage;
            }

            return View();
        }

        [Authorize(Roles = "User")]
        public IActionResult New()
        {

            return View();
        }
        [HttpPost]
        [Authorize(Roles = "User")]
        public ActionResult New(Rezervare rez)
        {
            rez.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.ListaRezervari.Add(rez);
                db.SaveChanges();
                TempData["message"] = "Rezervare facuta cu succes !";
                return RedirectToAction("Index");
            }

            else
            {
                return View(rez);
            }
        }


        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            SetAccessRights();

            if (User.IsInRole("User"))
            {
                var rezervari = db.ListaRezervari
                                  .Include("ListaRezervari")
                                  .Where(b => b.UserId == _userManager.GetUserId(User))
                                  .Include("User")
                                  .Where(b => b.Id == id)
                                  .FirstOrDefault();

                if (rezervari == null)
                {
                    TempData["message"] = "Nu exista rezervari facute";
                    return RedirectToAction("Index", "Hoteluri");
                }

                return View(rezervari);
            }

            else
            if (User.IsInRole("Admin"))
            {
                var rezevari = db.ListaRezervari
                                  .Include("ListaCamere")
                                  .Where(b => b.UserId == _userManager.GetUserId(User))
                                  .Include("User")
                                  .Where(b => b.Id == id)
                                  .FirstOrDefault();


                if (rezevari == null)
                {
                    TempData["message"] = "Nu exista rezervari";
                    return RedirectToAction("Index", "Hoteluri");
                }


                return View(rezevari);
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi";
                return RedirectToAction("Index", "Posts");
            }
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Rezervare rez = db.ListaRezervari.Find(id);
            db.ListaRezervari.Remove(rez);
            TempData["message"] = "Rezervarea a fost stearsa";
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRezervari()
        {

            var selectList = new List<SelectListItem>();
            var rezerivari = from rez in db.ListaRezervari
                      select rez;
            foreach (var rezervare in rezerivari)
            {
                 
                selectList.Add(new SelectListItem
                {
                    Value =rezervare.Id.ToString()
                     
                });
            }
            return selectList;
        }

        [Authorize(Roles = "User")]
        public IActionResult Edit(int id)
        {

            Rezervare rezervare = db.ListaRezervari
                            .Where(b => b.Id == id)
                                  .First();


            rezervare = (Rezervare)GetAllRezervari();

            if (rezervare.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(rezervare);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei rezervari care nu va apartine";
                return RedirectToAction("Index");
            }

        }

        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("User") )
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }

    }
}

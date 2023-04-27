using MDS.Data;
using MDS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Data;

namespace MDS.Controllers
{
    public class TariController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public TariController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        [Authorize(Roles = "User,Admin")]

        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            SetAccessRights();

            if (User.IsInRole("User"))
            {   
                var tari = from tara in db.ListaTari
                           select tara;

                ViewBag.Tari = tari;

                return View();
            }
            else
            if (User.IsInRole("Admin"))
            {
                var tari = from tara in db.ListaTari
                           select tara;

                ViewBag.Tari = tari;

                return View();
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi";
                return RedirectToAction("Index", "Hoteluri");
            }

        }


        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            SetAccessRights();

            if (User.IsInRole("User") || User.IsInRole("Admin"))
            {
                var tara = db.ListaTari.Where(t => t.Id == id).FirstOrDefault();
      
                tara.ListaHoteluri = db.ListaHoteluri.Where(h => h.TaraId == id).ToList();

                if (tara.ListaHoteluri.Count == 0)
                {
                    TempData["message"] = "Nu exista hoteluri pentru tara selectata";
                    return RedirectToAction("Index", "Tari");
                }

                

                return View(tara);
            }
            else
            {
                TempData["message"] = "Nu aveti drepturi";
                return RedirectToAction("Index", "Tari");
            }
        }


        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult New(Tara tr)
        {
            tr.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.ListaTari.Add(tr);
                db.SaveChanges();
                TempData["message"] = "Tara a fost adaugata";
                return RedirectToAction("Index");
            }

            else
            {
                return View(tr);
            }
        }


        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }


        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Tara tara = db.ListaTari.Where(art => art.Id == id)
                                         .First();

            if (tara.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.ListaTari.Remove(tara);
                db.SaveChanges();
                TempData["message"] = "Tara a fost stearsa";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti o tara";
                return RedirectToAction("Index");
            }
        }

        public IActionResult Edit(int id)
        {

            Tara tara = db.ListaTari.Include("Hotel")
                                        .Where(art => art.Id == id)
                                        .First();

            if (tara.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(tara);
            }
            else
            {
                TempData["message"] = "Nu puteti edita aceasta tara";
                return View(tara);
            }

        }
    }
}

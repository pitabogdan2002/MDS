using MDS.Data;
using MDS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MDS.Controllers
{
    [Authorize]
    public class CamereController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CamereController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        


        [Authorize(Roles = "Agent,User,Admin")]
        public IActionResult Show(int id)
        {
            Camera camera = db.ListaCamere.Include("Hotel")
                                           .Include("ListaRezervari")
                                           .Include("ListaRezervari.User")
                                            .Where(art => art.Id == id)
                                            .First();

            

            return View(camera);

        }

        [Authorize(Roles = "User,Admin,Agent")]
        public IActionResult New()
        {
            Camera camera = new Camera();
                camera.Hoteluri = GetAllHotels();

            return View(camera);
        }


        public ActionResult New(Collection bm)
        {
            bm.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Collections.Add(bm);
                db.SaveChanges();
                TempData["message"] = "Colectia a fost adaugata";
                return RedirectToAction("Index");
            }

            else
            {
                return View(bm);
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


        [NonAction]
        public IEnumerable<SelectListItem> GetAllHotels()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var hoteluri = from hotel in db.ListaCamere
                             select hotel;

            // iteram prin categorii
            foreach (var hotel in hoteluri)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = hotel.Id.ToString(),
                    Text = hotel.Nume.ToString()
                });
            }
           
            return selectList;
        }


    }
}

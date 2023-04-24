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
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }


            Camera camera = new Camera();

            camera.Hoteluri = GetAllHotels();
            var camere = from camer in db.ListaCamere
                             orderby camer.PretNoapte
                             select camer;
            ViewBag.Camere = camere;
            return View(camera);
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


        [HttpPost]
        [Authorize(Roles = "User,Admin,Agent")]
        public ActionResult New(Camera cm)
        {
            //cm.UserId = _userManager.GetUserId(User);


            db.ListaCamere.Add(cm);
            db.SaveChanges();
            TempData["message"] = "Camera a fost adaugata";
            // return RedirectToAction("Index");
            return View(cm);

        }

        [Authorize(Roles = "Agent,Admin")]
        public IActionResult Edit(int id)
        {

            Camera camera = db.ListaCamere.Include("Hotel")
                                        .Where(art => art.Id == id)
                                        .First();

            camera.Hoteluri = GetAllHotels();


            if (camera.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(camera);
            }
            else
            {
                TempData["message"] = "Nu puteti edita aceasta camera deoarece nu va apartine";
                //return RedirectToAction("Index");
                return View(camera);
            }

        }


        [HttpPost]
        [Authorize(Roles = "Agent,Admin")]
        public ActionResult Delete(int id)
        {
            Camera camera = db.ListaCamere.Include("Hotel")
                                         .Where(art => art.Id == id)
                                         .First();

            if (camera.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            { 
                db.ListaCamere.Remove(camera);
                db.SaveChanges();
                TempData["message"] = "Camera a fost stearsa";
                //return RedirectToAction("Index");
                return View();
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti acesta camera";
                //return RedirectToAction("Index");
                return View();
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
            var hoteluri = from hotel in db.ListaHoteluri
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

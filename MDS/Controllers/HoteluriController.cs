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
    public class HoteluriController : Controller
    {

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public HoteluriController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }


        [Authorize(Roles = "User,Agent,Admin")]

        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            Hotel hotel = new Hotel();

            hotel.Tari = GetAllTari();
            var hoteluri = from hot in db.ListaHoteluri
                           orderby hot.Rating
                           select hot;
            ViewBag.Hoteluri = hoteluri;
            return View(hoteluri);
        }


        [Authorize(Roles = "User,Agent,Admin")]
        public IActionResult Show(int id)
        {

            Hotel hotel = db.ListaHoteluri.Include("Tara")
                                           .Include("ListaCamere")
                                           .Include("ListaCamere.User")
                                           .Include("ListaReviews")
                                           .Include("ListaReviews.User")
                                            .Where(art => art.Id == id)
                                            .First();

            var ratings = db.ListaReviews.Where(art => art.HotelId == id).ToList();

            if (ratings.Count() > 0)
            {
                var ratingSum = ratings.Sum(d => d.Rating.Value);
                ViewBag.RatingSum = ratingSum;
                var ratingCount = ratings.Count();
                ViewBag.RatingCount = ratingCount;
                hotel.Rating = ratingSum / ratingCount;
                ViewBag.RatingAvg = ratingSum / ratingCount;
            }
            else
            {
                hotel.Rating = 0;///???????
                ViewBag.RatingSum = 0;
                ViewBag.RatingCount = 0;
            }
            db.SaveChanges();

            SetAccessRights();

            return View(hotel);
        }


        [Authorize(Roles = "Agent,Admin")]
        public IActionResult New()
        {
            Hotel hotel = new Hotel();
            hotel.Rating = 0;
            hotel.Tari = GetAllTari();

            return View(hotel);
        }

        [HttpPost]
        [Authorize(Roles = "Agent,Admin")]
        public ActionResult New(Hotel bm)
        {
            bm.Rating = 0;
            db.ListaHoteluri.Add(bm);
            db.SaveChanges();

            TempData["message"] = "Hotelul a fost adaugat";
            //return RedirectToAction("Index");
            return View(bm);

        }

        [HttpPost]
        [Authorize(Roles = "Agent,Admin")]
        public ActionResult Edit(int id)
        {
            Hotel hotel = db.ListaHoteluri.Include("Tara")
                                             .Where(art => art.Id == id)
                                             .First();
            hotel.Tari = GetAllTari();

            if (hotel.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(hotel);
            }
            else
            {
                TempData["message"] = "Nu puteti edita acest hotel";
                //return RedirectToAction("Index");
                return View(hotel);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Agent,Admin")]
        public ActionResult Delete(int id)
        {
            Hotel hotel = db.ListaHoteluri.Include("Tara")
                                             .Where(art => art.Id == id)
                                             .First();

            if (hotel.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.ListaHoteluri.Remove(hotel);
                db.SaveChanges();
                TempData["message"] = "Hotelul a fost sters";
                //return RedirectToAction("Index");
                return View();
            }
            else
            {
                TempData["message"] = "Nu puteti sterge acest hotel";
                //return RedirectToAction("Index");
                return View();
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllTari()
        {
            var selectList = new List<SelectListItem>();

            var tari = from tara in db.ListaTari
                       select tara;

            foreach (var tara in tari)
            {
                selectList.Add(new SelectListItem
                {
                    Value = tara.Id.ToString(),
                    Text = tara.Nume.ToString()
                });
            }


            return selectList;
        }

        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            ViewBag.EsteAdmin = User.IsInRole("Admin");
            ViewBag.EsteAdmin = User.IsInRole("Agent");
            ViewBag.UserCurent = _userManager.GetUserId(User);
        }


    }
}
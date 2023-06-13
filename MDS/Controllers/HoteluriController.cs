using Ganss.Xss;
using MDS.Data;
using MDS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.ComponentModel.Design;

namespace MDS.Controllers
{

    [Authorize]
    public class HoteluriController : Controller
    {
        private Dictionary<Hotel, List<Camera>> availableRoomsByHotel ;

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
            return View(hotel);
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
                ViewBag.RatingAvg = 0;
            }
            db.SaveChanges();

            SetAccessRights();

            return View(hotel);
        }
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show([FromForm] Review rev)
        {
             
            rev.UserId = _userManager.GetUserId(User);


            if (ModelState.IsValid)
            {
                db.ListaReviews.Add(rev);
                db.SaveChanges();
                return Redirect("/Hoteluri/Show/" + rev.HotelId);
            }

            else
            {
                Hotel h = db.ListaHoteluri.Include("Tara")
                                         .Include("User")
                                         .Include("ListaReviews")
                                         .Include("ListaReviews.User")
                                         .Where(art => art.Id == rev.HotelId)
                                         .First();

                //return Redirect("/Articles/Show/" + comm.ArticleId);

                // Adaugam bookmark-urile utilizatorului pentru dropdown
                ViewBag.UserCollections = db.ListaTari
                                          .Where(b => b.UserId == _userManager.GetUserId(User))
                                          .ToList();

                SetAccessRights();

                return View(h);
            }
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
            bm.Tari = GetAllTari();
            bm.UserId = _userManager.GetUserId(User);
            db.ListaHoteluri.Add(bm);
            db.SaveChanges();
           

            TempData["message"] = "Hotelul a fost adaugat";
            return RedirectToAction("Show", "Hoteluri", new { id = bm.Id });

        }

 
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
        public IActionResult Edit(int id, Hotel requestArticle)
        {
            var sanitizer = new HtmlSanitizer();

            Hotel h = db.ListaHoteluri.Find(id);


            try
            { 
                if (h.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    h.Nume = requestArticle.Nume;
                    h.Locatie = requestArticle.Locatie;

                    requestArticle.Facilitati = sanitizer.Sanitize(requestArticle.Facilitati);

                    h.Facilitati = requestArticle.Facilitati;
                     

                    h.TaraId = requestArticle.TaraId;
                    
                    db.SaveChanges();
                    TempData["message"] = "Hotelul a fost modificat";
                    return RedirectToAction("Show", "Hoteluri", new { id = id });

                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui hotel care nu va apartine";
                    return RedirectToAction("Index", "Tari");

                }
            }
            catch (Exception e)
            {
                return View(requestArticle);
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
                var c = db.ListaCamere.Where(c => c.Hotel == hotel).First();
                var r = db.ListaReviews.Where(r => r.HotelId == hotel.Id).ToList();
                var rezervari = db.ListaRezervari.Where(r => r.CameraId == c.Id).ToList();
                db.ListaRezervari.RemoveRange(rezervari);
                db.ListaReviews.RemoveRange(r);
                // Delete the hotel record
                db.ListaHoteluri.Remove(hotel);
                db.SaveChanges();
                TempData["message"] = "Hotelul a fost sters";
                return RedirectToAction("Index","Hoteluri");
            }
            else
            {
                TempData["message"] = "Nu puteti sterge acest hotel";
                return RedirectToAction("Index", "Hoteluri");
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
            ViewBag.EsteAgent = User.IsInRole("Agent");
            ViewBag.UserCurent = _userManager.GetUserId(User);
        }

        public IActionResult CautareHoteluri(List<string> selectedFilters)
        {

            List<string> filterOptions = new List<string>
    {
        "Filter1",
        "Filter2",
        "Filter3"
    };

            ViewBag.FilterOptions = filterOptions;
            ViewBag.SelectedFilters = selectedFilters;

            //ViewBag.Hoteluri = null;
            var hoteluri = from hot in db.ListaHoteluri
                           orderby hot.Rating
                           select hot;
            var tari = db.ListaTari.ToList();
            ViewBag.Countries = tari;
            //ViewBag.Hoteluri = hoteluri;


            var hotelsWithAvailableRooms = new List<Hotel>();
            if (Convert.ToString(HttpContext.Request.Query["checkinDate"]) != null && Convert.ToString(HttpContext.Request.Query["checkoutDate"]) != null)
            {
                if (HttpContext.Request.Query["checkinDate"] != "" && HttpContext.Request.Query["checkoutDate"] != "")
                {

                    DateTime checkinDate = DateTime.Parse(HttpContext.Request.Query["checkinDate"]);
                    DateTime checkoutDate = DateTime.Parse(HttpContext.Request.Query["checkoutDate"]);

                    ViewBag.In = checkinDate;
                    ViewBag.Out = checkoutDate;
                    int numPersons = int.Parse(HttpContext.Request.Query["numPersons"]);
                    string country = HttpContext.Request.Query["country"];

                    var hotels = db.ListaHoteluri.Include(h => h.ListaCamere).Where(h => h.Tara.Nume == country).ToList();
                    availableRoomsByHotel = new Dictionary<Hotel, List<Camera>>();

                    foreach (var hotel in hotels)
                    {
                        var availableRooms = hotel.ListaCamere
                        .Where(room =>
                             room.Capacitate >= numPersons &&
                        !db.ListaRezervari.Any(reservation =>
                        reservation.CameraId == room.Id &&
                         reservation.CheckIn <= checkinDate &&
                        reservation.CheckOut >= checkoutDate
                    )  ).ToList();

                        if (selectedFilters != null && selectedFilters.Any())
                        {
                            availableRooms = availableRooms.Where(room =>
                                selectedFilters.All(filter => room.Descriere.Contains(filter))).ToList();
                        }

                        if (availableRooms.Count > 0)
                        {
                            hotelsWithAvailableRooms.Add(hotel);
                        }
                        availableRoomsByHotel.Add(hotel, availableRooms);
                    }

                   ViewBag.CamereHoteluri =  availableRoomsByHotel;

                }

                else
                    {
                        TempData["message"] = "Toate campurile sunt obligatorii";
                        ViewBag.Mesaj = TempData["message"];
                        return RedirectToAction("CautareHoteluri");
                    }
                
            }
           
            ViewBag.CheckinDate = "2023-05-05";
            ViewBag.CheckoutDate = "2023-05-05";
            ViewBag.NumPersons = "";
            ViewBag.Hoteluri = hotelsWithAvailableRooms;

            return View();
        }


       

        private List<string> GetDesiredItemsOptions()
        {
            // Retrieve options from a data source or hard-code them
            List<string> options = new List<string>
            {
                "Room Service",
                "AC"
            };

            return options;
        }


    }
}
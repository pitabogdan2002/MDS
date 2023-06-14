using Ganss.Xss;
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
        private int cameratrimisa = 0;

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
        [Authorize(Roles = "User,Admin,Agent")]
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

        public IActionResult RezervarileMele()
        {   
            var rezervari = db.ListaRezervari.OrderByDescending(a => a.CheckIn).ThenByDescending(a => a.CheckOut)
                                          .Include("User").Where(a =>a.UserId== _userManager.GetUserId(User));
            var userName = _userManager.GetUserName(User);
            var user = db.Users.FirstOrDefault(u => u.UserName == userName);
            ViewBag.UserName = user.UserName;


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

        public IActionResult New(int id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"].ToString();
            }

            var rez = new Rezervare
            {
                Suma = 0,
                CameraId = id
            };

            return View(rez);
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public ActionResult New(Rezervare rez)
        {
            var sanitizer = new HtmlSanitizer();
          
            rez.UserId = _userManager.GetUserId(User);

            var camera = db.ListaCamere.Include("Hotel")
                .Include("ListaRezervari")
                .Include("ListaRezervari.User")
                .Where(art => art.Id == rez.CameraId)
                .First();

            if (ModelState.IsValid)
            {
                if (rez.CheckOut <= rez.CheckIn)
                {
                    TempData["message"] = "Data de check-out trebuie sa fie mai mare decat data de check-in!";

                    if (TempData.ContainsKey("message"))
                    {
                        ViewBag.Message = TempData["message"].ToString();
                    }
                    return View(rez);
                }

                var overlappingReservations = camera.ListaRezervari
                    .Where(r => !(rez.CheckOut < r.CheckIn || rez.CheckIn > r.CheckOut))
                    .ToList();

                if (overlappingReservations.Count > 0)
                {
                    TempData["message"] = "Camera este deja rezervata pentru perioada selectata!";
                    if (TempData.ContainsKey("message"))
                    {
                        ViewBag.Message = TempData["message"].ToString();
                    }
                    return View(rez);
                }

                TimeSpan zile = rez.CheckOut - rez.CheckIn;
                int nrzile = (int)zile.TotalDays;
                rez.Suma = (nrzile+1) * camera.PretNoapte;

                db.ListaRezervari.Add(rez);
                db.SaveChanges();
                TempData["message"] = "Rezervare facuta cu succes!";
                return RedirectToAction("Show", "Camere", new { id = rez.CameraId });
            }
            else
            {
                if (TempData.ContainsKey("message"))
                {
                    ViewBag.Message = TempData["message"].ToString();
                }
                rez.Suma = 0;
                return View(rez);
            }
        }



        [Authorize(Roles = "User,Admin")]
        public ActionResult Show(int id)
        {
            var reservation = db.ListaRezervari.Find(id);
            var camera = db.ListaCamere.SingleOrDefault(c => c.Id == reservation.CameraId);

            ViewBag.Reservation = reservation;
            ViewBag.Camera = camera;
            SetAccessRights();
            return View();
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Rezervare rezervare = db.ListaRezervari.Find(id);

            if (rezervare == null)
            {
                return NotFound();
            }

            if (rezervare.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
            {
                TempData["message"] = "Nu aveți dreptul să faceți modificări asupra unei rezervări care nu vă aparține";
                return RedirectToAction("Index");
            }

            return View(rezervare);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Rezervare rez)
        {
            // Se cauta rezervarea in baza de date dupa ID
            Rezervare reservationToEdit = db.ListaRezervari.Find(id);

            // Se verifica daca utilizatorul este proprietarul rezervarii sau admin-ul
            if (reservationToEdit.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                // Daca utilizatorul este proprietarul sau admin-ul, se actualizeaza rezervarea cu noile date din formular
                reservationToEdit.CheckIn = rez.CheckIn;
                reservationToEdit.CheckOut = rez.CheckOut;
                reservationToEdit.ListaClienti = rez.ListaClienti;

                var camera = db.ListaCamere.Include("Hotel")
                .Include("ListaRezervari")
                .Include("ListaRezervari.User")
                .Where(art => art.Id == reservationToEdit.CameraId)
                .First();

                TimeSpan zile = reservationToEdit.CheckOut - reservationToEdit.CheckIn;
                int nrzile = (int)zile.TotalDays;
                
                reservationToEdit.Suma = (nrzile+1) * camera.PretNoapte;

                // Se salveaza modificarile in baza de date
                db.SaveChanges();

                // Se afiseaza un mesaj de confirmare si se redirectioneaza catre pagina principala
                TempData["message"] = "Rezervarea a fost modificată cu succes";
                return RedirectToAction("Index");
            }
            else
            {
                // Daca utilizatorul nu are dreptul sa modifice rezervarea, se redirectioneaza catre pagina principala cu un mesaj de eroare
                TempData["message"] = "Nu aveți dreptul să faceți modificări asupra unei rezervări care nu vă aparține";
                return RedirectToAction("Index");
            }
        }



        [HttpPost]
        public ActionResult Delete(int id)
        {
            Rezervare rez = db.ListaRezervari.Find(id);
            db.ListaRezervari.Remove(rez);
            TempData["message"] = "Rezervarea a fost ștearsă";
            db.SaveChanges();
            return RedirectToAction("Index");
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

    


    }
}
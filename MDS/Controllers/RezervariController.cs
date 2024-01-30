using AngleSharp.Dom;
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
            var currentDate = DateTime.Now;
            var userId = _userManager.GetUserId(User);

            // Obține rezervările pentru utilizatorul curent și care au Disponibila setată la false
            var rezervari = db.ListaRezervari
                .Where(a => a.UserId == userId && a.Anulata == 0)  // Filtrare pentru rezervările cu Disponibila = false
                .OrderByDescending(a => a.CheckIn)
                .ThenByDescending(a => a.CheckOut)
                .Include("User")
                .ToList();

            var userName = _userManager.GetUserName(User);
            var user = db.Users.FirstOrDefault(u => u.UserName == userName);

            // Separă rezervările în cele trecute și cele viitoare
            var pastReservations = rezervari.Where(a => a.CheckOut < currentDate).ToList();
            var futureReservations = rezervari.Where(a => a.CheckOut >= currentDate).ToList();

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

            // Paginare pentru rezervările trecute
            var paginatedPastReservations = pastReservations.Skip(offset).Take(_perPage);
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            ViewBag.PastReservations = paginatedPastReservations;
            ViewBag.FutureReservations = futureReservations;

            if (totalItems == 0)
            {
                TempData["message"] = "Nu s-a găsit nicio rezervare";
                return RedirectToAction("Index", "Tari");
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Rezervari/Index/" + currentPage;
            }

            return View();
        }


        public IActionResult CancelReservation(int id)
        {
            var userId = _userManager.GetUserId(User);

            var reservationToDelete = db.ListaRezervari.Find(id);

            if (reservationToDelete != null)
            {


                // Setează Disponibila la true
                reservationToDelete.Anulata = 1;

                // Round the number sumDecimal / 10
                decimal rotunjire = Math.Round((decimal)reservationToDelete.Suma / 10);

                // Subtract the rounded value from sumDecimal

                if ((decimal)reservationToDelete.Suma - rotunjire > 0)
                {
                    reservationToDelete.Suma = (float)((decimal)reservationToDelete.Suma - rotunjire);

                    // Salvează modificările în baza de date
                    db.SaveChanges();

                    TempData["message"] = $"Rezervarea a fost anulată cu succes la prețul de {rotunjire} $";
                }
                else
                {
                    TempData["message"] = "Nu s-a putut anula rezervarea din cauza prețului prea mic.";
                }
            }
            else
            {
                TempData["message"] = "Nu s-a putut anula rezervarea.";
            }

            // Redirecționează către pagina originală sau unde dorești să mergi după anularea rezervării
            return RedirectToAction("RezervarileMele");
        }



        public IActionResult RezervariAnulate()
        {
            // Obține rezervările anulate din baza de date care au Disponibila setată la true
            var reservationsToDisplay = db.ListaRezervari
                .Where(r => r.Anulata == 1)
                .ToList();


            // Update ViewBag cu lista de rezervări anulate de afișat
            ViewBag.CanceledReservations = reservationsToDisplay;

            // Redirect to the original page or wherever you want to go after processing canceled reservations.
            return View();
        }

        public IActionResult PreiaRezervarea(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Find the reservation in the database
            var rezervareToPreia = db.ListaRezervari.Find(id);

            if (rezervareToPreia != null)
            {
                // Update the details with current user data
                rezervareToPreia.UserId = userId; // Set the UserId to the current user
                rezervareToPreia.Anulata = 0;     // Set Anulata to 0

                // Save the changes to the database
                db.SaveChanges();

                TempData["message"] = "Rezervarea a fost preluată cu succes.";
            }
            else
            {
                TempData["message"] = "Nu s-a putut prelua rezervarea.";
            }

            // Redirect to the original page or wherever you want to go after updating the reservation.
            return RedirectToAction("RezervarileMele");
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
            ViewBag.Future = false;
            ViewBag.Reservation = reservation;
            ViewBag.Camera = camera;
            SetAccessRights();
            if(reservation.CheckIn > DateTime.Today)
            {
                ViewBag.Future = true;
            }
            
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
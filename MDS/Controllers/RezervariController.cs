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
        [Authorize(Roles = "User")]
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

        public IActionResult New(int id)
        {
            cameratrimisa = id;
            ViewBag.x = cameratrimisa;
            Rezervare rez = new Rezervare();
            rez.Suma = 0;
            rez.CameraId = id;

            return View(rez);
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public ActionResult New(Rezervare rez)
        {
            var sanitizer = new HtmlSanitizer();

            rez.UserId = _userManager.GetUserId(User);

            var camera = db.ListaCamere.SingleOrDefault(c => c.Id == cameratrimisa);
            if (camera == null || camera.Disponibila == false)
            {
                return RedirectToAction("Show/" + cameratrimisa, "Camere");
            }
            if (ModelState.IsValid)
            {
                rez.CheckIn = rez.CheckIn;
                rez.CheckOut = rez.CheckOut;
                rez.ListaClienti = rez.ListaClienti;
                camera.Disponibila = true;

                TimeSpan zile = rez.CheckOut - rez.CheckIn;
                int nrzile = (int)zile.TotalDays;
                if (nrzile == 0)
                    nrzile = 1;
                rez.Suma = nrzile * camera.PretNoapte;

                db.ListaRezervari.Add(rez);
                db.SaveChanges();
                TempData["message"] = "Rezervare facuta cu succes !";
                return RedirectToAction("Index");
            }
            else
            {
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
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei rezervari care nu va apartine";
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

                // Se salveaza modificarile in baza de date
                db.SaveChanges();

                // Se afiseaza un mesaj de confirmare si se redirectioneaza catre pagina principala
                TempData["message"] = "Rezervarea a fost modificata cu succes";
                return RedirectToAction("Index");
            }
            else
            {
                // Daca utilizatorul nu are dreptul sa modifice rezervarea, se redirectioneaza catre pagina principala cu un mesaj de eroare
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei rezervari care nu va apartine";
                return RedirectToAction("Index");
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

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
        // toti utilizatorii pot vedea Bookmark-urile existente in platforma
        // fiecare utilizator vede bookmark-urile pe care le-a creat
        // HttpGet - implicit
        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            SetAccessRights();

            if (User.IsInRole("User"))
            {
                var bookmarks = from bookmark in db.ListaTari.Include("User")
                               .Where(b => b.UserId == _userManager.GetUserId(User))
                                select bookmark;

                ViewBag.Collections = bookmarks;

                return View();
            }
            else
            if (User.IsInRole("Admin"))
            {
                var bookmarks = from bookmark in db.ListaTari.Include("User")
                                select bookmark;

                ViewBag.Collections = bookmarks;

                return View();
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi";
                return RedirectToAction("Index", "Posts");
            }

        }

        // Afisarea tuturor articolelor pe care utilizatorul le-a salvat in 
        // bookmark-ul sau 

        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            SetAccessRights();

            if (User.IsInRole("User"))
            {
                var bookmarks = db.ListaTari
                                  .Include("PostCollections.Post.Category")
                                  .Include("PostCollections.Post.User")
                                  .Include("User")
                                  .Where(b => b.Id == id)
                                  .Where(b => b.UserId == _userManager.GetUserId(User))
                                  .FirstOrDefault();

                if (bookmarks == null)
                {
                    TempData["message"] = "Tara nu exista sau nu aveti drepturi";
                    return RedirectToAction("Index", "Posts");
                }

                return View(bookmarks);
            }

            else
            if (User.IsInRole("Admin"))
            {
                var bookmarks = db.ListaTari
                                  .Include("PostCollections.Post.Category")
                                  .Include("PostCollections.Post.User")
                                  .Include("User")
                                  .Where(b => b.Id == id)
                                  .FirstOrDefault();


                if (bookmarks == null)
                {
                    TempData["message"] = "Resursa cautata nu poate fi gasita";
                    return RedirectToAction("Index", "Posts");
                }


                return View(bookmarks);
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi";
                return RedirectToAction("Index", "Posts");
            }
        }


        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult New(Tara bm)
        {
            bm.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.ListaTari.Add(bm);
                db.SaveChanges();
                TempData["message"] = "Tara a fost adaugata";
                return RedirectToAction("Index");
            }

            else
            {
                return View(bm);
            }
        }


        // Conditiile de afisare a butoanelor de editare si stergere
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

        public ICollection<SelectListItem> GetAllHoteluri()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.ListaHoteluri
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Nume.ToString()
                });
            }
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName.ToString();

                selectList.Add(listItem);
             }*/


            // returnam lista de categorii
            return selectList;
        }
        // Se sterge un articol din baza de date 
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Tara article = db.ListaTari.Where(art => art.Id == id)
                                         .First();

            if (article.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.ListaTari.Remove(article);
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

            tara.ListaHoteluri = (ICollection<Hotel>?)GetAllHoteluri();


            if (tara.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(tara);
            }
            else
            {
                TempData["message"] = "Nu puteti edita aceasta tara";
                //return RedirectToAction("Index");
                return View(tara);
            }

        }
    }
}


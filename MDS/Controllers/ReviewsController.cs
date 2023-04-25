using MDS.Data;
using MDS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace MDS.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public ReviewsController(
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
                var bookmarks = from bookmark in db.ListaReviews.Include("User")
                               .Where(b => b.UserId == _userManager.GetUserId(User))
                                select bookmark;

                ViewBag.Collections = bookmarks;

                return View();
            }
            else
            if (User.IsInRole("Admin"))
            {
                var bookmarks = from bookmark in db.ListaReviews.Include("User")
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
                var bookmarks = db.ListaReviews
                                  .Include("PostCollections.Post.Category")
                                  .Include("PostCollections.Post.User")
                                  .Include("User")
                                  .Where(b => b.Id == id)
                                  .Where(b => b.UserId == _userManager.GetUserId(User))
                                  .FirstOrDefault();

                if (bookmarks == null)
                {
                    TempData["message"] = "Review-ul nu exista sau nu aveti drepturi";
                    return RedirectToAction("Index", "Posts");
                }

                return View(bookmarks);
            }

            else
            if (User.IsInRole("Admin"))
            {
                var bookmarks = db.ListaReviews
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
        public ActionResult New(Review bm)
        {
            bm.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.ListaReviews.Add(bm);
                db.SaveChanges();
                TempData["message"] = "Review-ul a fost adaugat";
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


        public ActionResult Delete(int id)
        {
            Review article = db.ListaReviews.Where(art => art.Id == id)
                                         .First();

            if (article.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.ListaReviews.Remove(article);
                db.SaveChanges();
                TempData["message"] = "Review-ul a fost sters";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un review";
                return RedirectToAction("Index");
            }
        }
        public IActionResult Edit(int id)
        {

            Review review = db.ListaReviews
                                        .Where(art => art.Id == id)
                                        .First();



            if (review.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(review);
            }
            else
            {
                TempData["message"] = "Nu puteti edita aceasta review-ul";
                //return RedirectToAction("Index");
                return View(review);
            }

        }
    }
}

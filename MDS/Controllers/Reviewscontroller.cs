using MDS.Data;
using MDS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace MDS.Controllers
{
    public class ListaReviewsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public ListaReviewsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        [HttpPost]
        public IActionResult New(Review rev)
        {

            if (ModelState.IsValid)
            {
                db.ListaReviews.Add(rev);
                db.SaveChanges();
                return Redirect("/Hoteluri/Show/" + rev.HotelId);
            }

            else
            {
                return Redirect("/Hoteluri/Show/" + rev.HotelId);
            }

        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id)
        {
            Review rev = db.ListaReviews.Find(id);

            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.ListaReviews.Remove(rev);
                db.SaveChanges();
                return Redirect("/Hoteluri/Show/" + rev.HotelId);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti review-ul";
                return RedirectToAction("Index", "Hoteluri");
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Review rev = db.ListaReviews.Find(id);

            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(rev);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati review-ul";
                return RedirectToAction("Index", "Hoteluri");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Review requestReview)
        {
            Review rev = db.ListaReviews.Find(id);

            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {

                    db.SaveChanges();

                    return Redirect("/Hoteluri/Show/" + rev.HotelId);
                }
                else
                {
                    return View(requestReview);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                return RedirectToAction("Index", "Hoteluri");
            }
        }
    }
}

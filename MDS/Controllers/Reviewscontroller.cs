using Ganss.Xss;
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
                TempData["message"] = "Nu aveți dreptul să ștergeți review-ul";
                return RedirectToAction("Index", "Tari");
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Review rev = db.ListaReviews.Include("Hotel")
                                        .Where(art => art.Id == id)
                                        .First(); 

            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(rev);
            }

            else
            {
                TempData["message"] = "Nu aveți dreptul să editați review-ul";
                return RedirectToAction("Index", "Tari");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Review requestReview)
        {
            var sanitizer = new HtmlSanitizer();
            Review rev = db.ListaReviews.Include("Hotel")
                                          .Where(art=>art.Id==id)
                                          .First();
            try
            {
                if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {

                    requestReview.Continut = sanitizer.Sanitize(requestReview.Continut);
                    rev.Continut = requestReview.Continut;

                    db.SaveChanges();

                    return RedirectToAction("Show", "Hoteluri", new { id = rev.Hotel.Id });
                }
                else
                {
                    TempData["message"] = "Nu aveți dreptul să faceți modificări";
                    return RedirectToAction("Show","Hoteluri", new {id = rev.Hotel.Id} );
                }
            }
            catch (Exception e)
            {
                return View(requestReview);
            }
        }
    }
}

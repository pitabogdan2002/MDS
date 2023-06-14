﻿using MDS.Data;
using MDS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MDS.Controllers
{
    namespace ArticlesApp.Controllers
    {
        [Authorize(Roles = "Admin")]
        public class UsersController : Controller
        {
            private readonly ApplicationDbContext db;

            private readonly UserManager<ApplicationUser> _userManager;

            private readonly RoleManager<IdentityRole> _roleManager;

            public UsersController(
                ApplicationDbContext context,
                UserManager<ApplicationUser> userManager,
                RoleManager<IdentityRole> roleManager
                )
            {
                db = context;

                _userManager = userManager;

                _roleManager = roleManager;
            }
            public IActionResult Index()
            {
                var users = from user in db.Users
                            orderby user.UserName
                            select user;

                ViewBag.UsersList = users;

                return View();
            }

             

            public async Task<ActionResult> Edit1(string id)
            {
                ApplicationUser user = db.Users.Find(id);

                user.AllRoles = GetAllRoles();

                var roleNames = await _userManager.GetRolesAsync(user); // Lista de nume de roluri

                // Cautam ID-ul rolului in baza de date
                var currentUserRole = _roleManager.Roles
                                                  .Where(r => roleNames.Contains(r.Name))
                                                  .Select(r => r.Id)
                                                  .First(); // Selectam 1 singur rol
                ViewBag.UserRole = currentUserRole;

                return View(user);
            }

            [HttpPost]
            public async Task<ActionResult> Edit(string id, ApplicationUser newData, [FromForm] string newRole)
            {
                ApplicationUser user = db.Users.Find(id);

                user.AllRoles = GetAllRoles();


                if (ModelState.IsValid)
                {    
                    user.UserName = newData.UserName;
                    user.Email = newData.Email;
                    user.Nume = newData.Nume;
                    user.Prenume = newData.Prenume;
                    user.PhoneNumber = newData.PhoneNumber;


                    // Cautam toate rolurile din baza de date
                    var roles = db.Roles.ToList();

                    foreach (var role in roles)
                    {
                        // Scoatem userul din rolurile anterioare
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                    }
                    // Adaugam noul rol selectat
                    var roleName = await _roleManager.FindByIdAsync(newRole);
                    await _userManager.AddToRoleAsync(user, roleName.ToString());

                    db.SaveChanges();

                }
                return RedirectToAction("Index");
            }


            [HttpPost]
            public IActionResult Delete(string id)
            {
                var user = db.Users
                             .Include("ListaReviews")
                             
                             .Where(u => u.Id == id)
                             .First();

                 
                if (user.ListaReviews.Count > 0)
                {
                    foreach (var comment in user.ListaReviews)
                    {
                        db.ListaReviews.Remove(comment);
                    }
                }
                

                db.Users.Remove(user);
                db.SaveChanges();

                return RedirectToAction("Index");
            }


            [NonAction]
            public IEnumerable<SelectListItem> GetAllRoles()
            {
                var selectList = new List<SelectListItem>();

                var roles = from role in db.Roles
                            select role;

                foreach (var role in roles)
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = role.Id.ToString(),
                        Text = role.Name.ToString()
                    });
                }
                return selectList;
            }
        }
    }

}

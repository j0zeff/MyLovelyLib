﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASP.NET_MVC_testapp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;

namespace ASP.NET_MVC_testapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, MyDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult SearchUsers(string searchTerm)
        {
            if (searchTerm.IsNullOrEmpty())
            {
                var FullList = _context.AplicationUsers.ToList();
                return PartialView("_UserList", FullList);
            }
            var searchResults = _context.AplicationUsers
            .Where(b => b.Id.Contains(searchTerm) || b.Email.Contains(searchTerm) || b.Firstname.Contains(searchTerm) || b.Lastname.Contains(searchTerm))
            .ToList();

            ViewBag.IsAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (ViewBag.IsAjaxRequest)
            {
                // AJAX request, return partial view
                return PartialView("_UserList", searchResults);
            }
            else
            {
                return View("Users");
            }
        }

        public IActionResult Index()
        {
            List<Event> _Eventlist = _context.Events.ToList();
            return View(_Eventlist);
        }

        public IActionResult Users(int? pageNumber)
        {
            int pageSize = 5;
            var users = _context.AplicationUsers.ToList();
            

            var userList = PaginatedList<ApplicationUser>.Create(users, pageNumber ?? 1, pageSize);
            return View(userList);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null || _context.AplicationUsers == null)
            {
                return NotFound();
            }

            var user = await _context.AplicationUsers.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null || _context.AplicationUsers == null)
            {
                return NotFound();
            }

            var user = await _context.AplicationUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }
            var _user = _context.AplicationUsers.Find(id);
            _user = user;
            if (ModelState.IsValid)
            {
                try
                {
                    //_context.Update(user);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Users");
            }
            return View(_user);
        }

        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null || _context.AplicationUsers == null)
            {
                return NotFound();
            }

            var user = await _context.AplicationUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Books == null)
            {
                return Problem("Entity set 'MyDbContext.Book'  is null.");
            }
            var user = await _context.AplicationUsers.FindAsync(id);
            if (user != null)
            {
                _context.AplicationUsers.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Users");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        private bool UserExists(string id)
        {
            return (_context.AplicationUsers?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
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
using System.Security.Claims;
using System.Data.Entity;
using System.Drawing.Printing;
using Microsoft.VisualBasic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace ASP.NET_MVC_testapp.Controllers
{
    //todo apply Route attribute to define correct route to the controller
    //[Route("api/books/my")]
    //todo use Authorize to garantie existing of user claims
    //[Authorize]
    public class MyBooksController : Controller
    {
        private readonly MyDbContext _context;
        private readonly ILogger _logger;

        public MyBooksController(MyDbContext context, ILogger<MyBooksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        //todo use Http Verb attribute
        //[HttpPost("favorites")]
        //if more that 5 argument please use class
        public IActionResult AddToFavorites(int bookId, bool IsFavorite/*naming*/, string name, string view, string controller,/*handle redirect on UI part*/ int? pageIndex/*use default value*/)
        {
            //create base class with CurrentUser identity and needed methods
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var temp = _context.UserFavoriteBooks
                .Where(b => b.user_id == currentUserId && b.book_id == bookId)
                .ToList()//Get all data from db to memory by predicate
                .FirstOrDefault();//Get first item from the list

            if (temp == null)
            {
                //todo move to separate method
                if (bookId != null && currentUserId != null)
                {
                    //todo best way is to use switch instead of if\else
                    switch (name)
                    {
                        case "":
                            break;
                    }

                    var favoriteBook_1 = new FavoriteBook();
                    //
                    //  fill favoriteBook_1 properties
                    //
                    if (name == "favorite")
                    {
                        var favoriteBook = new FavoriteBook { book_id = bookId, user_id = currentUserId, favorite = IsFavorite };
                        _context.UserFavoriteBooks.Add(favoriteBook);
                    }
                    else if (name == "current")
                    {
                        var favoriteBook = new FavoriteBook { book_id = bookId, user_id = currentUserId, currentlyReading = IsFavorite };
                        _context.UserFavoriteBooks.Add(favoriteBook);
                    }
                    else if (name == "will")
                    {
                        var favoriteBook = new FavoriteBook { book_id = bookId, user_id = currentUserId, willRead = IsFavorite };
                        _context.UserFavoriteBooks.Add(favoriteBook);
                    }
                    else if (name == "already")
                    {
                        var favoriteBook = new FavoriteBook { book_id = bookId, user_id = currentUserId, alreadyRead = IsFavorite };
                        _context.UserFavoriteBooks.Add(favoriteBook);

                    }

                    //_context.UserFavoriteBooks.Add(favoriteBook_1);
                    _context.SaveChanges();
                }
                return RedirectToRoute(new { action = view, controller = controller });
            }


            //todo best way is to use switch instead of if\else
            switch (name)
            {
                case "":
                    break;
            }

            if (name == "favorite")
            {
                //move _context.SaveChanges(); to the final row instead of code duplication in each if\else
                if (!IsFavorite)
                {
                    temp.favorite = IsFavorite;
                    if (!temp.alreadyRead && !temp.willRead && !temp.favorite && !temp.currentlyReading)
                    {
                        _context.UserFavoriteBooks.Remove(temp);
                    }
                    _context.SaveChanges();
                    return RedirectToAction("ShowList", new { num = 4, pageIndex = pageIndex });
                }
                else
                {
                    temp.favorite = IsFavorite;
                    _context.SaveChanges();
                }
            }
            else if (name == "current")
            {
                if (!IsFavorite)
                {
                    temp.currentlyReading = IsFavorite;
                    if (!temp.alreadyRead && !temp.willRead && !temp.favorite && !temp.currentlyReading)
                    {
                        _context.UserFavoriteBooks.Remove(temp);
                    }
                    _context.SaveChanges();
                    return RedirectToAction("ShowList", new { num = 2, pageIndex = pageIndex });
                }
                else
                {
                    temp.currentlyReading = IsFavorite;
                    _context.SaveChanges();
                }
            }
            else if (name == "will")
            {
                if (!IsFavorite)
                {
                    temp.willRead = IsFavorite;
                    if (!temp.alreadyRead && !temp.willRead && !temp.favorite && !temp.currentlyReading)
                    {
                        _context.UserFavoriteBooks.Remove(temp);
                    }
                    _context.SaveChanges();
                    return RedirectToAction("ShowList", new { num = 3, pageIndex = pageIndex });
                }
                else
                {
                    temp.willRead = IsFavorite;
                    _context.SaveChanges();
                }
            }
            else if (name == "already")
            {
                if (!IsFavorite)
                {
                    temp.alreadyRead = IsFavorite;
                    if (!temp.alreadyRead && !temp.willRead && !temp.favorite && !temp.currentlyReading)
                    {
                        _context.UserFavoriteBooks.Remove(temp);
                    }
                    _context.SaveChanges();
                    return RedirectToAction("ShowList", new { num = 1, pageIndex = pageIndex });
                }
                else
                {
                    temp.alreadyRead = IsFavorite;
                    _context.SaveChanges();
                }
            }

            return RedirectToRoute(new { action = view, controller = controller });
        }
        public IActionResult Index()
        {
            var viewModel = GetLists();
            return View(viewModel);
        }
        public IActionResult ShowList(int num, int? pageNumber)
        {
            var ViewModel = GetLists();
            int pageSize = 5;

            if (num == 1)
                return View(new ListViewModel { booklist = PaginatedList<Book>.Create(ViewModel.alreadyRead, pageNumber ?? 1, pageSize), name = "already" });
            else if (num == 2)
                return View(new ListViewModel { booklist = PaginatedList<Book>.Create(ViewModel.currentlyReading, pageNumber ?? 1, pageSize), name = "current" });
            else if (num == 3)
                return View(new ListViewModel { booklist = PaginatedList<Book>.Create(ViewModel.willRead, pageNumber ?? 1, pageSize), name = "will" });
            else if (num == 4)
                return View(new ListViewModel { booklist = PaginatedList<Book>.Create(ViewModel.favoriteBooks, pageNumber ?? 1, pageSize), name = "favorite" });
            return NotFound();
        }
        public IActionResult PaginatedList(int? pageNumber, string name)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int pageSize = 5;
            var ViewModel = GetLists();
            if (name == "already")
            {
                var favoriteBooks = _context.UserFavoriteBooks.Where(b => b.user_id == currentUserId && b.alreadyRead == true).ToList();
                List<Book> listbook = new List<Book>();
                foreach (var item in favoriteBooks)
                {
                    listbook.Add(_context.Books.Where(b => b.BookId == item.book_id).FirstOrDefault());
                }

                return View("ShowList", new ListViewModel { booklist = PaginatedList<Book>.Create(ViewModel.favoriteBooks, pageNumber ?? 1, pageSize), name = "already" });
            }
            else if (name == "current")
            {
                var favoriteBooks = _context.UserFavoriteBooks.Where(b => b.user_id == currentUserId && b.currentlyReading == true).ToList();
                List<Book> listbook = new List<Book>();
                foreach (var item in favoriteBooks)
                {
                    listbook.Add(_context.Books.Where(b => b.BookId == item.book_id).FirstOrDefault());
                }

                return View("ShowList", new ListViewModel { booklist = PaginatedList<Book>.Create(ViewModel.favoriteBooks, pageNumber ?? 1, pageSize), name = "current" });

            }
            else if (name == "will")
            {
                var favoriteBooks = _context.UserFavoriteBooks.Where(b => b.user_id == currentUserId && b.willRead == true).ToList();
                List<Book> listbook = new List<Book>();
                foreach (var item in favoriteBooks)
                {
                    listbook.Add(_context.Books.Where(b => b.BookId == item.book_id).FirstOrDefault());
                }

                return View("ShowList", new ListViewModel { booklist = PaginatedList<Book>.Create(ViewModel.favoriteBooks, pageNumber ?? 1, pageSize), name = "will" });
            }
            else
            {
                var favoriteBooks = _context.UserFavoriteBooks.Where(b => b.user_id == currentUserId && b.favorite == true).ToList();
                List<Book> listbook = new List<Book>();
                foreach (var item in favoriteBooks)
                {
                    listbook.Add(_context.Books.Where(b => b.BookId == item.book_id).FirstOrDefault());
                }

                return View("ShowList", new ListViewModel { booklist = PaginatedList<Book>.Create(ViewModel.favoriteBooks, pageNumber ?? 1, pageSize), name = "favorite" });

            }
        }
        public IndexViewModel GetLists()
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var books = _context.UserFavoriteBooks.Where(b => b.user_id == currentUserId).ToList();
            List<Book> ReadBooks = new List<Book>();
            List<Book> CurrentlyReadingBooks = new List<Book>();
            List<Book> FavoriteBooks = new List<Book>();
            List<Book> ToReadBooks = new List<Book>();
            foreach (var book in books)
            {
                var Book = _context.Books.Where(b => b.BookId == book.book_id).FirstOrDefault();
                if (book.alreadyRead == true)
                    ReadBooks.Add(Book);
                if (book.willRead)
                    ToReadBooks.Add(Book);
                if (book.currentlyReading == true)
                    CurrentlyReadingBooks.Add(Book);
                if (book.favorite == true)
                    FavoriteBooks.Add(Book);
            }
            var viewModel = new IndexViewModel
            {
                favoriteBooks = FavoriteBooks,
                willRead = ToReadBooks,
                currentlyReading = CurrentlyReadingBooks,
                alreadyRead = ReadBooks,
            };
            return viewModel;
        }
        public IActionResult SearchBooks(string searchTerm, string name)
        {
            var books = GetLists();
            var searchResults = new List<Book>();
            if (searchTerm.IsNullOrEmpty())
            {
                return PartialView("_BookList", books);
            }
            else
            {
                if (name == "favorite")
                {
                    searchResults = books.favoriteBooks
                                        .Where(b => b.Title.Contains(searchTerm) 
                                                    || b.AuthorName.Contains(searchTerm) 
                                                    || b.AuthorSurname.Contains(searchTerm)
                                                    //todo read about culture https://phrase.com/blog/posts/all-you-need-to-know-about-cultureinfo-in-net-applications/
                                                    //not a good idea search by date using strings
                                                    || b.ReleaseDate.ToString().Contains(searchTerm))
                                        .ToList();
                }
                else if (name == "current")
                {
                    searchResults = books.currentlyReading
            .Where(b => b.Title.Contains(searchTerm) || b.AuthorName.Contains(searchTerm) || b.AuthorSurname.Contains(searchTerm) || b.ReleaseDate.ToString().Contains(searchTerm))
            .ToList();
                }
                else if (name == "already")
                {
                    searchResults = books.alreadyRead
            .Where(b => b.Title.Contains(searchTerm) || b.AuthorName.Contains(searchTerm) || b.AuthorSurname.Contains(searchTerm) || b.ReleaseDate.ToString().Contains(searchTerm))
            .ToList();
                }
                else if (name == "will")
                {
                    searchResults = books.willRead
            .Where(b => b.Title.Contains(searchTerm) || b.AuthorName.Contains(searchTerm) || b.AuthorSurname.Contains(searchTerm) || b.ReleaseDate.ToString().Contains(searchTerm))
            .ToList();
                }
                else return NotFound();
            }


            ViewBag.IsAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (ViewBag.IsAjaxRequest)
            {
                return View("~/Views/Books/_BookList.cshtml", searchResults);

            }
            else
            {
                return View("IndexLib");
            }
        }

        [HttpPost]
        public IActionResult OnPostAddToMyBook(int bookId, string selectedOption, int? pageNumber)
        {
            switch (selectedOption)
            {
                case "favorites":
                    AddToFavorites(bookId, true, "favorite", "IndexLib", "Books", pageNumber);
                    break;
                case "alreadyRead":
                    AddToFavorites(bookId, true, "already", "IndexLib", "Books", pageNumber);
                    break;
                case "currentlyReading":
                    AddToFavorites(bookId, true, "current", "IndexLib", "Books", pageNumber);
                    break;
                case "willRead":
                    AddToFavorites(bookId, true, "will", "IndexLib", "Books", pageNumber);
                    break;
                default:

                    break;
            }
            return RedirectToRoute(new { action = "IndexLib", controller = "Books", pageNumber = pageNumber });
        }
    }
}



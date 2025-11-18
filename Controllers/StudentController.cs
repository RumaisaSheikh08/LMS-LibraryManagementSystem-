using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Azure.Core;
using LMSAvanza.Data;
using LMSAvanza.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LMSAvanza.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly IConfiguration _configuration;
        public StudentController(ApplicationDBContext context, IConfiguration configuration)
        {
            _dbContext = context;
            _configuration = configuration;
        }

        public ActionResult RegisterStudent()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterStudent(StudentModel student)
        {
            var lastStudentRecord = _dbContext.Students.OrderByDescending(s => s.StudentID).FirstOrDefault();
            int number = Convert.ToInt32(lastStudentRecord?.StudentID.Substring(1) ?? "0") + 1;

            var studentModel = new StudentModel
            {
                StudentID = "S" + number,
                Email = student.Email,
                Password = student.Password,
                Name = student.Name,
                OutstandingFees = "$0",
                Flag = "0"
            };

            _dbContext.Students.Add(studentModel);
            _dbContext.SaveChanges();

            TempData["RegisterationStatus"] = "Request Sent";
            return RedirectToAction("RegisterStudent");
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(StudentModel student)
        {
            TempData["Status"] = string.Empty;
            var studentLogin = _dbContext.Students
                .FirstOrDefault(s => s.Email == student.Email && s.Password == student.Password && s.Flag == "1");

            if (studentLogin != null)
            {
                HttpContext.Session.SetString("StudentID", studentLogin.StudentID);
                HttpContext.Session.SetString("Email", student.Email);
                HttpContext.Session.SetString("Name", studentLogin.Name);
                var token = GenerateJwtToken(student.Email);

                Response.Cookies.Append("jwtToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(60)
                });

                return RedirectToAction("Dashboard");
            }

            TempData["Error"] = "Invalid email or password.";
            return View();

        }

        [Authorize]
        public ActionResult Dashboard()
        {
            var emailStudent = HttpContext.Session.GetString("Email");
            var nameStudents = HttpContext.Session.GetString("Name");

            var allBooks = _dbContext.Books.Where(b => b.AvailableCopies != 0).ToList();

            ViewBag.Books = allBooks;
            ViewBag.Email = emailStudent;
            ViewBag.Name = nameStudents;

            return View();
        }

        [HttpPost]
        public ActionResult SearchBook(string searchTerm)
        {
            string[] categoryBooks = searchTerm.Split("-");
            string bookName = categoryBooks[0].Trim();
            string categoryName = categoryBooks[1].Trim();

            var bookDetails = _dbContext.Books
               .FirstOrDefault(b => b.Title == bookName && b.Category == categoryName);

            if (bookDetails != null)
            {
                HttpContext.Session.SetString("BooksId", bookDetails.BookID.ToString());
                ViewBag.BookDetails = bookDetails;
            }

            return View();
        }

        [HttpPost]
        public ActionResult ReserveBook()
        {
            var studentId = HttpContext.Session.GetString("StudentID");
            var bookIdStr = HttpContext.Session.GetString("BooksId");

            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(bookIdStr))
            {
                TempData["Error"] = "Missing session data.";
                return RedirectToAction("Dashboard");
            }

            var lastTransaction = _dbContext.Transactions.OrderByDescending(t => t.TxnID).FirstOrDefault();
            int number = Convert.ToInt32(lastTransaction?.TxnID.Substring(1) ?? "0") + 1;

            var transaction = new TransactionModel
            {
                TxnID = "T" + number,
                StudentID = studentId,
                BookID = double.Parse(bookIdStr),
                IssueDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(14),
                Fee = "$0",
                Reserved = "0"
            };

            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();

            TempData["Status"] = "Request Sent";
            return RedirectToAction("Dashboard");
        }

        public ActionResult MyBooks()
        {
            var studentId = HttpContext.Session.GetString("StudentID");
            var allTransactions = _dbContext.Transactions.Where(t => t.StudentID == studentId).ToList();
            int number = 0;
            int sum = 0;

            foreach (var t in allTransactions)
            {
                number = Convert.ToInt32(t.Fee.Substring(1));
                sum = sum + number;
            }

            ViewBag.DueFees = "$" + sum;
            return View();
        }
        public ActionResult BookDetails()
        {
            //Reserved entity is for checking whether the book has been
            //returned or not
            var studentId = HttpContext.Session.GetString("StudentID");
            var result = (from b in _dbContext.Books
                          join t in _dbContext.Transactions
                          on b.BookID equals t.BookID
                          where t.StudentID == studentId && t.Reserved == "1"
                          select new BookDetailsModel
                          {
                              Title = b.Title,
                              Author = b.Author,
                              Category = b.Category,
                              DueDate = t.DueDate,
                              ReturnDate = t.ReturnDate,
                              BookId = b.BookID
                          }).ToList();
           
            return View(result);
        }

        [HttpPost]
        public ActionResult ReturnBook(double BookID)
        {
            var studentId = HttpContext.Session.GetString("StudentID");
            var returnBook = _dbContext.Transactions
             .FirstOrDefault(r => r.BookID == BookID && r.StudentID == studentId);


            var detailsBook = _dbContext.Books
            .FirstOrDefault(r => r.BookID == BookID);

            if (returnBook != null)
            {
                returnBook.ReturnDate = DateTime.Today;
                detailsBook.AvailableCopies = detailsBook.AvailableCopies + 1;
                _dbContext.SaveChanges();
            }
            return View("MyBooks");
        }

        public ActionResult PaymentDues()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PaymentDues(string nameCard, string cardNumber, string dateExpiry, string securityCode)
        {
            if(string.IsNullOrWhiteSpace(nameCard) && string.IsNullOrWhiteSpace(cardNumber) && string.IsNullOrWhiteSpace(dateExpiry) && string.IsNullOrWhiteSpace(securityCode))
            {
                TempData["Failure Message"] = "Payment Failed";
                return View();
            }
            var emailStudent = HttpContext.Session.GetString("Email");


            return View();
        }

        private string GenerateJwtToken(string email)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub , email),
                new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
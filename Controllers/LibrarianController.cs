using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LMSAvanza.Data;
using LMSAvanza.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Versioning;
using OfficeOpenXml;

namespace LMSAvanza.Controllers
{
    public class LibrarianController : Controller
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly IConfiguration _configuration;

        public LibrarianController(ApplicationDBContext context , IConfiguration configuration)
        { 
            _dbContext = context;
            _configuration = configuration;
        }
        // GET: LibrarianController
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(AdminModel admin)
        {
            var adminLogin = _dbContext.Admin
                .FirstOrDefault(s => s.Email == admin.Email && s.Password == admin.Password);

            if (adminLogin != null)
            {
                HttpContext.Session.SetString("AdminID", adminLogin.AdminID);
                HttpContext.Session.SetString("Email", admin.Email);
                HttpContext.Session.SetString("Name", adminLogin.Name);
                var token = GenerateJwtToken(admin.Email);

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

        public ActionResult Dashboard()
        {
            var adminEmail = HttpContext.Session.GetString("Email");
            var adminName = HttpContext.Session.GetString("Name");

            var allBooks = _dbContext.Books.Where(b => b.AvailableCopies != 0).ToList();

            ViewBag.Books = allBooks;
            ViewBag.Email = adminEmail;
            ViewBag.Name = adminName;

            return View();
        }
        public ActionResult StudentRegisteration()
        {
            var allStudents = _dbContext.Students.Where(t => t.Flag == "0").ToList();
            return View(allStudents);
        }

        [HttpPost]
        public ActionResult ApproveRequest(string StudentID)
        {
            var studentDetails = _dbContext.Students
           .FirstOrDefault(s => s.StudentID == StudentID);

            if(studentDetails != null)
            {
                studentDetails.Flag = "1";
                _dbContext.SaveChanges();
            }

            return RedirectToAction("StudentRegisteration");
        }

        public ActionResult GenerateReports()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GenerateReports(string SearchItem)
        {
            if (SearchItem == "bookInv")
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "BookInventory.xlsx");

                // Check if file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Excel template not found.");
                }

                // Load the existing Excel file
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var package = new ExcelPackage(stream);


                var worksheet = package.Workbook.Worksheets[0]; // First worksheet

                var data = _dbContext.Books.ToList();
                worksheet.Cells[3, 1].Value = "Book Title";
                worksheet.Cells[3, 2].Value = "Author";
                worksheet.Cells[3, 3].Value = "Category";
                worksheet.Cells[3, 4].Value = "Available Copies";


                using (var range = worksheet.Cells[3, 1, 3, 4])
                {
                    range.Style.Font.Bold = true;
                }

                for (int i = 2; i < data.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = data[i].Title;
                    worksheet.Cells[i + 2, 2].Value = data[i].Author;
                    worksheet.Cells[i + 2, 3].Value = data[i].Category;
                    worksheet.Cells[i + 2, 4].Value = data[i].AvailableCopies;
                }

                // Export file to browser
                var output = new MemoryStream();
                package.SaveAs(output);
                output.Position = 0;
                return File(output,
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       "BookReports.xlsx");
            }
            else if (SearchItem == "studentInv")
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "StudentRegisteration.xlsx");

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Excel template not found.");
                }

                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var package = new ExcelPackage(stream);


                var worksheet = package.Workbook.Worksheets[0];

                var data = _dbContext.Students.ToList();
                worksheet.Cells[3, 1].Value = "Student ID";
                worksheet.Cells[3, 2].Value = "Student Name";
                worksheet.Cells[3, 3].Value = "Student Email";
                worksheet.Cells[3, 4].Value = "Outstanding Fees";


                using (var range = worksheet.Cells[3, 1, 3, 4])
                {
                    range.Style.Font.Bold = true;
                }

                for (int i = 2; i < data.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = data[i].StudentID;
                    worksheet.Cells[i + 2, 2].Value = data[i].Name;
                    worksheet.Cells[i + 2, 3].Value = data[i].Email;
                    worksheet.Cells[i + 2, 4].Value = data[i].OutstandingFees;
                }

                // Export file to browser
                var output = new MemoryStream();
                package.SaveAs(output);
                output.Position = 0;
                return File(output,
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       "StudentReports.xlsx");

            }
            else if (SearchItem == "transactionInv")
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "Transactiondetail.xlsx");

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Excel template not found.");
                }

                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var package = new ExcelPackage(stream);


                var worksheet = package.Workbook.Worksheets[0];

                var data = from student in _dbContext.Students
                           join transaction in _dbContext.Transactions
                           on student.StudentID equals transaction.StudentID
                           select new
                           {
                               student.StudentID,
                               student.Name,
                               transaction.IssueDate,
                               transaction.DueDate,
                               transaction.ReturnDate,
                               transaction.BookID,
                               transaction.Fee
                           };

                worksheet.Cells[3, 1].Value = "Student ID";
                worksheet.Cells[3, 2].Value = "Student Name";
                worksheet.Cells[3, 3].Value = "Student Book ID";
                worksheet.Cells[3, 4].Value = "Issue Date";
                worksheet.Cells[3, 5].Value = "Due Date";
                worksheet.Cells[3, 6].Value = "Return Date";
                worksheet.Cells[3, 7].Value = "Fee";


                using (var range = worksheet.Cells[3, 1, 3, 7])
                {
                    range.Style.Font.Bold = true;
                }

                int row = 4;
                foreach (var item in data)
                {
                    worksheet.Cells[row, 1].Value = item.StudentID;
                    worksheet.Cells[row, 2].Value = item.Name;
                    worksheet.Cells[row, 3].Value = item.BookID;
                    worksheet.Cells[row, 4].Value = item.IssueDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 5].Value = item.DueDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 6].Value = item.ReturnDate.ToString("yyyy-MM-dd");
                    row++;
                }
                // Export file to browser
                var output = new MemoryStream();
                package.SaveAs(output);
                output.Position = 0;
                return File(output,
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       "TransactionReports.xlsx");
            }

            return View();
        }

        public ActionResult BookManagement()
        {
            TempData["Message"] = string.Empty;
            var books = (from b in _dbContext.Books
                        select new BooksModel()
                        { 
                            BookID = b.BookID,
                            Title = b.Title,
                            Author = b.Author,
                            Category = b.Category,
                            AvailableCopies = b.AvailableCopies
                        }).ToList();
            return View(books);
        }

        [HttpPost]
        public ActionResult BookManagement(string Title, string Author , string Category, string AvailableCopies)
        {
            TempData["Message"] = string.Empty;
            var books = (from b in _dbContext.Books
                            select new BooksModel()
                            {
                                BookID = b.BookID,
                                Title = b.Title,
                                Author = b.Author,
                                Category = b.Category,
                                AvailableCopies = b.AvailableCopies
                            }).ToList();
            
            var lastBookRecord = _dbContext.Books.OrderByDescending(s => s.BookID).FirstOrDefault();
            double number = lastBookRecord.BookID + 1;
            var bookModel = new BooksModel
            {
               BookID = number,
               Author = Author,
               Title = Title,
               Category = Category,
               AvailableCopies = Convert.ToDouble(AvailableCopies)
            };

            _dbContext.Books.Add(bookModel);
            _dbContext.SaveChanges();
            TempData["Message"] = "Successfully Added";
            return View(books);
        }

        [HttpPost]
        public ActionResult DeleteBook(double BookID)
        {
            TempData["Message"] = string.Empty;
            var transactions = (from t in _dbContext.Transactions
                               where t.BookID == BookID
                               select t.TxnID).ToList();

            foreach(var t in transactions)
            {
                var transactionDetails = _dbContext.Transactions.FirstOrDefault(b => b.BookID == BookID);
                _dbContext.Transactions.Remove(transactionDetails);
                _dbContext.SaveChanges();
            }

            var books = (from b in _dbContext.Books
                         select new BooksModel()
                         {
                             BookID = b.BookID,
                             Title = b.Title,
                             Author = b.Author,
                             Category = b.Category,
                             AvailableCopies = b.AvailableCopies
                         }).ToList();

            var bookDetails = _dbContext.Books.FirstOrDefault(b => b.BookID == BookID);
            _dbContext.Books.Remove(bookDetails);
            _dbContext.SaveChanges();

            return View("Dashboard");
        }

        [HttpPost]
        public ActionResult UpdateBook(int BookID)
        {
            var book = _dbContext.Books.FirstOrDefault(b => b.BookID == BookID);
            return View(book);
        }

        [HttpPost]
        public ActionResult UpdatedBook(BooksModel bookModel)
        {
            var findBook = _dbContext.Books.FirstOrDefault(b => b.BookID == bookModel.BookID);
            

            findBook.Title = bookModel.Title;
            findBook.Author = bookModel.Author;
            findBook.Category = bookModel.Category;
            findBook.AvailableCopies = bookModel.AvailableCopies;

            _dbContext.SaveChanges();

            TempData["Status"] = "Successfully Updated";
            return View("UpdateBook");
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

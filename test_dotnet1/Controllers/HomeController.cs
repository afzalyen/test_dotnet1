using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test_dotnet1_Models.Models;
using test_dotnet_Data_Access; // Ensure this matches your data access layer namespace
using test_dotnet_Data_Access.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace test_dotnet1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var questions = _context.Questions.ToList(); // Fetch the list of questions from the database
            return View(questions);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize] // Ensure only authorized users can access the AskQuestion functionality
        public IActionResult AskQuestion()
        {
            return View("~/Views/Questions/AskQuestion.cshtml"); // Specify the path to the AskQuestion view
        }

        [HttpPost]
        [Authorize]
        public IActionResult AskQuestion(Question question)
        {
            if (ModelState.IsValid)
            {
                question.CreatedAt = DateTime.Now;
                question.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Assuming the user is logged in
                _context.Questions.Add(question);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("~/Views/Questions/AskQuestion.cshtml", question); // Specify the path again in case of invalid model
        }

        [Authorize]
        public IActionResult AnswerQuestion(int id)
        {
            var question = _context.Questions.Find(id);
            if (question == null)
            {
                return NotFound();
            }
            return View("~/Views/Questions/AnswerQuestion.cshtml", question); // Specify the path to the AnswerQuestion view
        }

        [HttpPost]
        [Authorize]
        public IActionResult AnswerQuestion(Answer answer)
        {
            if (ModelState.IsValid)
            {
                _context.Answers.Add(answer);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            // If model state is invalid, return the same view with the model
            return View("~/Views/Questions/AnswerQuestion.cshtml", answer); // Specify the path in case of invalid model
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

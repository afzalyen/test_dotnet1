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
        [ValidateAntiForgeryToken]
        public IActionResult AskQuestion(Question question)
        {
            question.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!ModelState.IsValid)
            {
                // Log validation errors for debugging
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Error in {state.Key}: {error.ErrorMessage}");
                        _logger.LogError($"Error in {state.Key}: {error.ErrorMessage}");
                    }
                }
                return View("~/Views/Questions/AskQuestion.cshtml", question);
            }

            question.CreatedAt = DateTime.Now;
            question.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Questions.Add(question);
            //_context.SaveChanges();
            int result = _context.SaveChanges(); // Save changes to the database
            _logger.LogInformation($"{result} question(s) saved to the database.");
            Console.WriteLine($"{result} question(s) saved to the database.");
            return RedirectToAction("Index");
        }

        //[Authorize]
        //public IActionResult AnswerQuestion(int id)
        //{
        //    var question = _context.Questions.Find(id);
        //    if (question == null)
        //    {
        //        return NotFound();
        //    }
        //    return View("~/Views/Questions/AnswerQuestion.cshtml", question); // Specify the path to the AnswerQuestion view
        //}

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


        public IActionResult AnswerQuestion(int id)
        {
            var question = _context.Questions
                .Include(q => q.Answers) // Assuming you want to include answers
                .FirstOrDefault(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            return View("~/Views/Questions/AnswerQuestion.cshtml", question); // Changed this line
        }


        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(Answer answer)
        {
            answer.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid)
            {
                // Set user ID and timestamp
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                answer.UserId = userId;
                answer.CreatedAt = DateTime.Now;

                // Add the answer to the context
                _context.Answers.Add(answer);

                try
                {
                    // Attempt to save changes to the database
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Answer successfully saved.");
                    return RedirectToAction("AnswerQuestion", new { id = answer.QuestionId });
                }
                catch (Exception ex)
                {
                    // Log any database errors
                    Console.WriteLine($"Error saving answer: {ex.Message}");
                    _logger.LogError($"Error saving answer: {ex.Message}");
                }
            }
            else
            {
                // Log ModelState errors for debugging
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Model state error in {state.Key}: {error.ErrorMessage}");
                        _logger.LogError($"Model state error in {state.Key}: {error.ErrorMessage}");
                    }
                }
            }

            // If ModelState is invalid or save fails, reload the question and answers
            var question = _context.Questions
                                   .Include(q => q.Answers)
                                   .FirstOrDefault(q => q.Id == answer.QuestionId);
            return View("~/Views/Questions/AnswerQuestion.cshtml", question);
        }
    }
}


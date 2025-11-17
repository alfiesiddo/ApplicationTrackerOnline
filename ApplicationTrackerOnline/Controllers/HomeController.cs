using ApplicationTrackerOnline.Data;
using ApplicationTrackerOnline.Models;
using ApplicationTrackerOnline.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApplicationTrackerOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ApplicationDbContext _context;
        private readonly ApplicationStatsService _statsService;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context, ApplicationStatsService statsService)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _statsService = statsService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Applications()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var applications = await _context.jobApplications.Where(l => l.UserId == userId).ToListAsync();
            return View(applications);
        }


        public IActionResult AddApplication()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNewApplication(string Company, string Role, string Location, int Salary, string Portal)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            JobApplication application = new JobApplication(Role, Company, Location, Portal, Salary, userId);

            _context.jobApplications.Add(application);
            await _context.SaveChangesAsync();

            return RedirectToAction("AddApplication", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteApplication(JobApplication application)
        {
            if (application != null)
            {
                _context.jobApplications.Remove(application);
                await _context.SaveChangesAsync();

                return RedirectToAction("Applications", "Home");
            }
            return RedirectToAction("Applications", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus([FromBody] UpdateStatusDTO model)
        {
            var application = await _context.jobApplications.FindAsync(model.Id);
            if (application != null)
            {
                application.Status = model.Status;
                await _context.SaveChangesAsync();
            }
            return Json(new { status = application.Status });
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyApplicationsChartData()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var data = await _statsService.GetDailyApplicationsLast3Months(userId);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetApplicationAverages()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
            }
            var data = _statsService.GetApplicationCounts(userId);

            return Json(data);
        }

        [HttpGet]
        public async Task <IActionResult> GetVaryingStages()
        {
            _logger.LogInformation("GetVaryingStages action called.");
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID is null or empty.");
                return Unauthorized();
            }

            _logger.LogInformation($"Fetching stages for user: {userId}");
            var data = await _statsService.GetDifferentStages(userId);
            _logger.LogInformation("Data fetched successfully.");

            return PartialView("_ApplicationStagesCard", data);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

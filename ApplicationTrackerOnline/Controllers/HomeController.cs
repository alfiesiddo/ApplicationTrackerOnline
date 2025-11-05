using ApplicationTrackerOnline.Data;
using ApplicationTrackerOnline.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Applications()
        {
            return View();
        }

        public IActionResult AddApplication()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNewApplication(string Company, string Role, string Location, int Salary, string Portal)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            string userId = "";
            if (user != null)
            { 
                userId = user.Id;
            }

            JobApplication application = new JobApplication(Role, Company, Location, Portal, userId);

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
            }

            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

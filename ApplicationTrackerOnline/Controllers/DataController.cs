using ApplicationTrackerOnline.Data;
using ApplicationTrackerOnline.Models;
using ApplicationTrackerOnline.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTrackerOnline.Controllers
{
    public class DataController : Controller
    {
        private readonly DataTransferringService _dataService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DataController(DataTransferringService dataService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _dataService = dataService;
            _userManager = userManager;
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSheet()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var apps = await _context.jobApplications.Where(a => a.UserId == userId).ToListAsync();

            int id = await _dataService.CreateAndStoreSpreadsheet(apps, userId);

            return RedirectToAction("Download", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var file = await _dataService.GetSpreadsheet(id);

            if (file == null)
            {
                return NotFound();
            }
            return File(file.SheetData,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",file.Filename);
        }
    }
}

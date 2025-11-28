using ApplicationTrackerOnline.Data;
using ApplicationTrackerOnline.Models;
using ApplicationTrackerOnline.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace ApplicationTrackerOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
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
        public async Task<IActionResult> CreateSheet()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            List<JobApplication> apps = await _context.jobApplications.Where(a => a.UserId == userId).ToListAsync();

            await _dataService.CreateAndStoreSpreadsheet(apps, userId);

            return Ok("Spreadsheet succesfully created.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var file = await _dataService.GetSpreadsheet(id);

            if(file == null)
            {
                return NotFound();
            }

            return File(file.SheetData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.Filename);
        }


    }
}

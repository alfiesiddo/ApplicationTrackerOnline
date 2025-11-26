using ApplicationTrackerOnline.Data;
using ApplicationTrackerOnline.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;



namespace ApplicationTrackerOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileAccessController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MobileAccessController(ApplicationDbContext context)
        {
            _context = context; 
        }

        [HttpGet("test")]
        [Authorize(AuthenticationSchemes = "JwtScheme")]
        public IActionResult Test()
        {
            var userId = GetUserId();

            return Ok(new { message = "Hello from protected endpoint", userId });
        }

        [HttpGet("applications")]
        [Authorize(AuthenticationSchemes = "JwtScheme")]
        public async Task<IActionResult> GetApplications()
        {
            string userId = await GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var applications = await _context.jobApplications.Where(l => l.UserId == userId).ToListAsync();

            return Ok(applications);
        }

        [HttpPost("addapplication")]
        [Authorize(AuthenticationSchemes = "JwtScheme")]
        public async Task<IActionResult> AddApplication(string Company, string Role, string Location, int Salary, string Portal)
        {
            string userId = await GetUserId();
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            JobApplication application = new JobApplication(Role, Company, Location, Portal, Salary, userId);
            _context.jobApplications.Add(application);
            await _context.SaveChangesAsync();

            return Ok("Application Added");
        }

        [HttpDelete("deleteapplication")]
        [Authorize(AuthenticationSchemes = "JwtScheme")]
        public async Task<IActionResult> DeleteApplication(int Id)
        {
            string userId = await GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var app = _context.jobApplications.FirstOrDefault(x => x.Id == Id);

            if (app != null)
            {
                _context.jobApplications.Remove(app);
                _context.SaveChanges();
            }
            return Ok("Application Deleted");

        }

        [HttpPatch("updatestatus")]
        [Authorize(AuthenticationSchemes = "JwtScheme")]
        public async Task<IActionResult> UpdateApplicationStatus(int Id, int status, DateTime? date)
        {
            string userId = await GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var application = await _context.jobApplications.FindAsync(Id);
            
            if(application == null)
            {
                return NotFound("Application not found");
            }
          
            application.Status = status;

            if(date != null && date.HasValue)
            {
                if (status == 3)
                {
                    application.AssessmentDeadline = date.Value;
                }
                else if (status == 4)
                {
                    application.InterviewDate = date.Value;
                }
                else
                {
                    application.InterviewDate = null;
                    application.AssessmentDeadline = null;
                }
            }


                await _context.SaveChangesAsync();
            
            return Ok($"Application:{application.Role} Status Updated to {status}");
        }

        private async Task<string> GetUserId()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            return userId;
        }
    }
}
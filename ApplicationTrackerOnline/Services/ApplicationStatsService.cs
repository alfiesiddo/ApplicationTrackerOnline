using ApplicationTrackerOnline.Data;
using ApplicationTrackerOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTrackerOnline.Services
{
    public class ApplicationStatsService
    {
        private readonly ApplicationDbContext _context;
        
        public ApplicationStatsService(ApplicationDbContext context)
        {
            _context = context;
        }

        //return top 5 locations user has applied for jobs to.
        private async Task<List<string>> getTopLocations(string userId)
        {
            List<string> output = new List<string>();

            var topLocations = await _context.jobApplications
                .Where(j => j.UserId == userId && j.Location != null)
                .GroupBy(j => j.Location)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(5)
                .ToListAsync();

            foreach (var location in topLocations)
            {
                output.Add(location.ToString());
            }
            return output;
        }


        //show amount of different statuses
        private async Task<ApplicationStagesDTO> getDifferentTypes(string userId)
        {
            var rejectedCount = await _context.jobApplications
                .CountAsync(j => j.UserId == userId && j.Status == 0);

            var appliedCount = await _context.jobApplications
                .CountAsync(j => j.UserId == userId && j.Status == 1);

            var scoutedCount = await _context.jobApplications
                .CountAsync(j => j.UserId == userId && j.Status == 2);

            var assessmentCount = await _context.jobApplications
                .CountAsync(j => j.UserId == userId && j.Status == 3);

            var interviewCount = await _context.jobApplications
                .CountAsync(j => j.UserId == userId && j.Status == 4);

            var offeredCount = await _context.jobApplications
                .CountAsync(j => j.UserId == userId && j.Status == 5);

            ApplicationStagesDTO output = new ApplicationStagesDTO()
            {
                rejectedCount = rejectedCount,
                appliedCount = appliedCount,
                assessmentCount = assessmentCount,
                interviewCount = interviewCount,
                offeredCount = offeredCount,
                scoutedCount = scoutedCount
            };

            return output;
        } 

    }
}

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
            ApplicationStagesDTO output = new ApplicationStagesDTO();



            return output;
        } 

    }
}

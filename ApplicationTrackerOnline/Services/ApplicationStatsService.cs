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
        public async Task<List<string>> getTopLocations(string userId)
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
        public async Task<ApplicationStagesDTO> GetDifferentStages(string userId)
        {
            var rejectedCount = await _context.jobApplications
                .CountAsync(j => j.UserId == userId && j.Status == 0);

            var appliedCount = await _context.jobApplications
                .CountAsync(j => j.UserId == userId);

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

        public async Task<ApplicationCountDTO> GetApplicationCounts(string userId)
        {
            int today = await applicationsToday(userId);
            int week = await applicationsThisWeek(userId);
            int month = await applicationsThisMonth(userId);

            ApplicationCountDTO output = new ApplicationCountDTO()
            {
                todayCount = today,
                thisWeekCount = week,
                thisMonthCount = month
            };

            return output;
        }

        //function that returns chart data of daily applications
        public async Task<List<DailyApplicationDTO>> GetDailyApplications(string userId)
        {       
            List<DailyApplicationDTO> dailyCounts = _context.jobApplications
                    .Where(a => a.UserId == userId)
                    .GroupBy(a => a.AppliedDate.Date)
                    .Select(g => new DailyApplicationDTO
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();        

            return dailyCounts;
        }

        //applications this month
        private async Task<int> applicationsThisMonth(string userId)
        {
            DateTime today = DateTime.Today;
            DateTime monthAgo = today.AddMonths(-1);

            int output = await _context.jobApplications.CountAsync(j => j.UserId == userId && j.AppliedDate.Date <= today && j.AppliedDate >= monthAgo);

            return output;
        }

        //applications this week

        private async Task<int> applicationsThisWeek(string userId)
        {
            DateTime today = DateTime.Today;
            DateTime weekAgo = today.AddDays(-7);

            int output = await _context.jobApplications.CountAsync(j =>j.UserId == userId && j.AppliedDate.Date <= today && j.AppliedDate >= weekAgo);

            return output;
        }

        //applications today
        private async Task<int> applicationsToday(string userId)
        {
            DateTime today = DateTime.Today;

            int output = await _context.jobApplications.CountAsync(j=> j.UserId == userId && j.AppliedDate.Date == today);

            return output;
        }
    }
}

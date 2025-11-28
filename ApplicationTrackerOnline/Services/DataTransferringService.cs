using ApplicationTrackerOnline.Data;
using ApplicationTrackerOnline.Models;

using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTrackerOnline.Services
{
    public class DataTransferringService
    {
        private readonly ApplicationDbContext _context;
        public DataTransferringService(ApplicationDbContext context) 
        { 
            _context = context;
        }

        public async Task<int> CreateAndStoreSpreadsheet(List<JobApplication> applications, string userId)
        {

            var exportList = applications.Select(a => new SpreadsheetExportDTO
            {
                Id = a.Id,
                Status = a.Status switch
                {
                    0 => "Rejected",
                    1 => "Applied",
                    2 => "Scouted",
                    3 => "Assessments",
                    4 => "Interview",
                    5=> "Offered",
                    _ => "Unknown"
                },
                Role = a.Role,
                CompanyName = a.CompanyName,
                Location = a.Location,
                Salary = a.Salary,
                PortalURL = a.PortalURL,
                AssessmentDeadline = a.AssessmentDeadline,
                InterviewDate = a.InterviewDate

            }).ToList();

            await DeleteExcessSheets(userId);
            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Applications");
            var table = ws.Cell(1, 1).InsertTable(exportList, "ApplicationsTable", true);

            table.Theme = XLTableTheme.TableStyleMedium1;

            ws.Columns().AdjustToContents();

            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            byte[] bytes = ms.ToArray();

            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            var sheet = new ApplicationsSpreadsheet
            {
                Filename = $"Applications({today}).xlsx",
                SheetData = bytes,
                UserId = userId
            };

            _context.applicationsSpreadsheets.Add(sheet);
            await _context.SaveChangesAsync();

            return sheet.Id;
        }

        private async Task DeleteExcessSheets(string userId)
        {
            var sheets = await _context.applicationsSpreadsheets.Where(j => j.UserId == userId).ToListAsync();

            if (sheets.Count >= 1)
            {
                foreach (var sheet in sheets)
                {
                    _context.Remove(sheet);
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ApplicationsSpreadsheet> GetSpreadsheet(int id)
        {
            var sheet = await _context.applicationsSpreadsheets.FindAsync(id);

            if(sheet != null)
            {
                return sheet;
            }
            return null;
        }
    }
}

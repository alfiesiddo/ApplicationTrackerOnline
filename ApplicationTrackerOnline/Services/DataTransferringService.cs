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
            applications = applications.OrderBy(a => a.Status).ToList();
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
                    5 => "Offered",
                    _ => "Unknown"
                },
                Role = a.Role,
                CompanyName = a.CompanyName,
                Location = a.Location,
                Salary = $"£{a.Salary}",
                PortalURL = a.PortalURL,
                AssessmentDeadline = a.AssessmentDeadline,
                InterviewDate = a.InterviewDate
            }).ToList();

            await DeleteExcessSheets(userId);

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Applications");
            var table = ws.Cell(1, 1).InsertTable(exportList, "ApplicationsTable", true);

            table.ShowAutoFilter = false;

            // Apply colors

            int statusColumnIndex = table.Fields.First(f => f.Name == "Status").Index + 1;
            foreach (var row in table.DataRange.Rows())
            {
                string status = row.Cell(statusColumnIndex).GetString();
                var cell = row.Cell(statusColumnIndex);

                cell.Style.Fill.BackgroundColor = status switch
                {
                    "Rejected" => XLColor.Red,
                    "Applied" => XLColor.BabyBlue,
                    "Scouted" => XLColor.Violet,
                    "Assessments" => XLColor.DarkOrange,
                    "Interview" => XLColor.MediumAquamarine,
                    "Offered" => XLColor.Lime,
                    _ => XLColor.White
                };
            }

            table.Theme = XLTableTheme.TableStyleMedium1;
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            var bytes = ms.ToArray();

            var today = DateOnly.FromDateTime(DateTime.Now);

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

using ApplicationTrackerOnline.Data;
using ApplicationTrackerOnline.Models;

using ClosedXML.Excel;

namespace ApplicationTrackerOnline.Services
{
    public class DataTransferringService
    {
        private readonly ApplicationDbContext _context;
        public DataTransferringService(ApplicationDbContext context) 
        { 
            _context = context;
        }

        public async Task CreateAndStoreSpreadsheet(List<JobApplication> applications, string userId)
        {
            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Applications");
            var table = ws.Cell(1, 1).InsertTable(applications, "ApplicationsTable", true);

            table.Theme = XLTableTheme.TableStyleDark1;

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
        }

        public async Task<byte[]> GetSpreadsheet(int id)
        {
            var sheet = await _context.applicationsSpreadsheets.FindAsync(id);

            if(sheet != null)
            {
                return sheet.SheetData;
            }
            return null;
        }
    }
}

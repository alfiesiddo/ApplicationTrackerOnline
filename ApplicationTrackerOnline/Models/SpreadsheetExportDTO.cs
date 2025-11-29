using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationTrackerOnline.Models
{
    public class SpreadsheetExportDTO
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string Salary{get; set;}

        public string PortalURL { get; set; }

        public string Status {  get; set; }

        public DateTime? AssessmentDeadline { get; set; }
        public DateTime? InterviewDate { get; set; }
    }
}

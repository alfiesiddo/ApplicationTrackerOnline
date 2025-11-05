using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ApplicationTrackerOnline.Models
{
    public class JobApplication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public string Role { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string PortalURL { get; set; }

        public int Salary {  get; set; }


        public int Status { get; set; } = 0; //0 = Rejected, 1 = Applied, 2 = Scouted, 3 = Assessments, 4 = Interview, 5 = Offered

        public DateTime AppliedDate { get; set; } = DateTime.Now;


        public JobApplication() { }

        public JobApplication(string role, string company, string location, string portal,int salary, string userId)
        {
            Role = role;
            CompanyName = company;
            Location = location;
            PortalURL = portal;
            Salary = salary;

            UserId = userId;
        }
    }
}

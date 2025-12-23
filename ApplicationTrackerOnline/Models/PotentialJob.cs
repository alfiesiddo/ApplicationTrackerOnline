using System.ComponentModel.DataAnnotations;

namespace ApplicationTrackerOnline.Models
{
    public class PotentialJob
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public string EmployerName { get; set; }
        public string JobListing { get; set; }

        public override string ToString()
        {
            return $"Job Listing for {EmployerName}";
        }

        public PotentialJob() { }

        public PotentialJob(string employername, string url, string userId)
        {
            EmployerName = employername;
            JobListing = url;
            UserId = userId;
        }
    }
}

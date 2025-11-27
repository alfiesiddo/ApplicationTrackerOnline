namespace ApplicationTrackerOnline.Models
{
    public class ApplicationDateDTO
    {
        public DateTime? Date { get; set; }
        public string Organization {  get; set; }

        public override string ToString()
        {
            return $"{Organization}: {Date:ddd dd MMM H:mm tt}";
        }

    }
}

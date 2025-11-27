namespace ApplicationTrackerOnline.Models
{
    public class ApplicationDateDTO
    {
        public DateTime? Date { get; set; }
        public string Organization {  get; set; }

        public override string ToString()
        {
            return $"{Organization}: {Date:dddd dd MMMM h:mm tt}";
        }

    }
}

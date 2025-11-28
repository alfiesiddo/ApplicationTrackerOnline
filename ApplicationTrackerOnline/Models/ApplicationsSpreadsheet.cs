namespace ApplicationTrackerOnline.Models
{
    public class ApplicationsSpreadsheet
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public string Filename { get; set; }
        public byte[] SheetData { get; set; }

    }
}

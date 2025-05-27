namespace ProjecteSOS_Grup03WebPage.DTOs
{
    public class UserProfileDTO
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int? Points { get; set; }
        public DateOnly? StartDate { get; set; }
        public bool? IsAdmin { get; set; }
    }
}

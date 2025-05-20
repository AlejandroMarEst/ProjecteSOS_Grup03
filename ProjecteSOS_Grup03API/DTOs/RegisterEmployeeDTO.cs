namespace ProjecteSOS_Grup03API.DTOs
{
    public class RegisterEmployeeDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateOnly? StartDate { get; set; }
        public double Salary { get; set; }
        public string? ManagerId { get; set; }
        public bool IsAdmin { get; set; }
    }
}

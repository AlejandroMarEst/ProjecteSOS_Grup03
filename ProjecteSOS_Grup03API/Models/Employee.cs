namespace ProjecteSOS_Grup03API.Models
{
    public class Employee : User
    {
        public DateOnly StartDate { get; set; }

        public double Salary { get; set; }

        public string? ManagerId { get; set; }
        public Employee? Manager { get; set; }

        public bool IsAdmin { get; set; }

        public ICollection<Employee> Employees { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}

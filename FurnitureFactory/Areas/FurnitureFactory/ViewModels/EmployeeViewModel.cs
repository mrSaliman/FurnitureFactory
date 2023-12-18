using FurnitureFactory.Areas.FurnitureFactory.Models;

namespace FurnitureFactory.Areas.FurnitureFactory.ViewModels;

public class EmployeeViewModel
{
    public IEnumerable<Employee> Employees { get; set; }
    public PageViewModel PageViewModel { get; set; }
    
    public SortViewModel? SortViewModel { get; set; }
    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string MiddleName { get; set; } = null!;
    public string Position { get; set; } = null!;
    public string Education { get; set; } = null!;
}
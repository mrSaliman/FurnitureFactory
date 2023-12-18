using FurnitureFactory.Areas.FurnitureFactory.Models;

namespace FurnitureFactory.Areas.FurnitureFactory.ViewModels;

public class OrderViewModel
{
    public IEnumerable<Order> Orders { get; set; }

    public PageViewModel PageViewModel { get; set; }
    public SortViewModel? SortViewModel { get; set; }

    public DateTime? OrderDate { get; set; }

    public string CustomerCompanyName { get; set; }

    public decimal SpecialDiscount { get; set; }

    public bool IsCompleted { get; set; }

    public string ResponsibleEmployeeFirstName { get; set; }
}
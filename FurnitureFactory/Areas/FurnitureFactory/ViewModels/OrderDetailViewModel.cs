using System.ComponentModel.DataAnnotations;
using FurnitureFactory.Areas.FurnitureFactory.Models;

namespace FurnitureFactory.Areas.FurnitureFactory.ViewModels;

public class OrderDetailViewModel
{
    public IEnumerable<OrderDetail> OrderDetails { get; set; }
    public PageViewModel PageViewModel { get; set; }
    public SortViewModel? SortViewModel { get; set; }
    public DateTime? OrderDate { get; set; }
    public string FurnitureName { get; set; }
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
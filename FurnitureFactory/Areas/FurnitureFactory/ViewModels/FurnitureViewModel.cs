using FurnitureFactory.Areas.FurnitureFactory.Models;

namespace FurnitureFactory.Areas.FurnitureFactory.ViewModels;

public class FurnitureViewModel
{
    public IEnumerable<Furniture> Furnitures { get; set; }
    public PageViewModel PageViewModel { get; set; }

    public string FurnitureName { get; set; }

    public string Description { get; set; }

    public string MaterialType { get; set; }

    public decimal Price { get; set; }

    public int QuantityOnHand { get; set; }
}
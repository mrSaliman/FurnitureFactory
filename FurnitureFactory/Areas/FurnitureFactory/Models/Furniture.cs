namespace FurnitureFactory.Areas.FurnitureFactory.Models;

public class Furniture
{
    public int FurnitureId { get; set; }

    public string FurnitureName { get; set; } = null!;

    public string? Description { get; set; }

    public string MaterialType { get; set; } = null!;

    public decimal Price { get; set; }

    public int QuantityOnHand { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
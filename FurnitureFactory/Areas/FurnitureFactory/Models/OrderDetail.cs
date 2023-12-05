namespace FurnitureFactory.Areas.FurnitureFactory.Models;

public class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int FurnitureId { get; set; }

    public int Quantity { get; set; }

    public virtual Furniture Furniture { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
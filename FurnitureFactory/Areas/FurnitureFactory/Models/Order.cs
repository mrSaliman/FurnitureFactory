namespace FurnitureFactory.Areas.FurnitureFactory.Models;

public class Order
{
    public int OrderId { get; set; }

    public DateTime OrderDate { get; set; }

    public int CustomerId { get; set; }

    public decimal SpecialDiscount { get; set; }

    public bool IsCompleted { get; set; }

    public int ResponsibleEmployeeId { get; set; }

    public Customer? Customer { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public Employee? ResponsibleEmployee { get; set; }
}
namespace BackEndAPI.Models;

public class OrderItem
{
    public int Id { get; private set; }
    public required int OrderId { get; set; }
    public required int ProductId { get; set; }
    public required int Quantity { get; set; }
    public decimal Subtotal { get; private set; }

    public void CalculateSubtotal(decimal price)
    {
        Subtotal = Quantity * price;
    }
}
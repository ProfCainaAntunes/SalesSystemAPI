namespace BackEndAPI.Models;

public enum OrderStatus {Pendente, Pago, Preparando, Entregue, Cancelado}

public class Order
{
    public int Id { get; private set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public decimal Total { get; private set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pendente;

    public required int ClientId { get; set; }
    public required int SellerId { get; set; }

    public List<OrderItem> Items { get; set; } = new List<OrderItem>();

    public void CalculateTotal()
    {
        decimal sum = 0;
        foreach (OrderItem item in Items)
            sum += item.Subtotal;
        Total = sum;
    }
}
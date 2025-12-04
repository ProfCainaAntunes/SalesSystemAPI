namespace BackEndAPI.Models;

public class Product
{
    public int Id { get; private set; }
    public required string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public required string Category { get; set; } = string.Empty; 
    public required decimal Price { get; set; }
    public int StockQuantity { get; set; } = 0;

    public void Consume(int quantity)
    {
        StockQuantity -= quantity;
    }
}
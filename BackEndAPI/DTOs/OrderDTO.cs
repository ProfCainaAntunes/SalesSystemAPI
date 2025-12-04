namespace BackEndAPI.DTOs;
using BackEndAPI.Models;

public class OrderDTO
{
    public int ClientId { get; set; }
    public int SellerId { get; set; }
    public OrderStatus Status { get; set; }

    public List<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
}
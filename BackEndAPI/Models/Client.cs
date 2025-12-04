namespace BackEndAPI.Models;

public class Client
{
    public int Id { get; private set; }
    public required string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public  string Phone { get; set; } = string.Empty;
}
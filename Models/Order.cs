public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Amount { get; set; }

        // Add this property to associate the Order with the User
    public User User { get; set; }
}
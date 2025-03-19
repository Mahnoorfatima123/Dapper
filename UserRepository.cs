using MySql.Data.MySqlClient;  // Use MySQL's client
using Dapper;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

public class UserRepository
{
    private readonly string _connectionString;

    private IMemoryCache _cache;

    public UserRepository(IConfiguration configuration,IMemoryCache cache)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _cache = cache;
    }

    // Fetch all of users from the database

public IEnumerable<User> GetAllUsers(){

 using (var connection = new MySqlConnection(_connectionString))  // Use MySQL connection
        {
      connection.Open();
      return connection.Query<User>("Select * FROM Users").ToList();

}
}


// Add a new user to the db 

public void AddUser(User user){
 using (var connection = new MySqlConnection(_connectionString))  // Use MySQL connection
        {
     connection.Open();
     connection.Execute("INSERT INTO Users(Name,Email) VALUES (@Name, @Email)",user);
}

}


// Get User by ID

public User GetUserById(int id){
 using (var connection = new MySqlConnection(_connectionString))  // Use MySQL connection
        {
  connection.Open();
  return connection.QueryFirstOrDefault<User>("Select * FROM Users WHERE Id=@Id", new{Id=id});

}

}

  // Get all users and their orders
public IEnumerable<UserWithOrders> GetAllUsersWithOrders()
{
    var cacheKey = "OrdersWithUsers"; // Cache key to store the result
    var cachedUsersWithOrders = _cache.Get<List<UserWithOrders>>(cacheKey); // Try to get from cache

    if (cachedUsersWithOrders == null) // If not found in cache
    {
        Console.WriteLine("Cache miss - fetching from database...");

        using (var connection = new MySqlConnection(_connectionString))  // Use MySQL connection
        {
            connection.Open();
            var sql = @"
                SELECT u.Id, u.Name, u.Email, o.Id AS OrderId, o.OrderDate, o.Amount
                FROM Users u
                LEFT JOIN Orders o ON u.Id = o.UserId";

            var userDictionary = new Dictionary<int, UserWithOrders>();

            var usersWithOrders = connection.Query<UserWithOrders, Order, UserWithOrders>(
                sql,
                (user, order) =>
                {
                    UserWithOrders userEntry;

                    if (!userDictionary.TryGetValue(user.Id, out userEntry))
                    {
                        userEntry = user;
                        userEntry.Orders = new List<Order>();
                        userDictionary.Add(user.Id, userEntry);
                    }

                    if (order != null)
                    {
                        order.User = user; // Associate the user with the order
                        userEntry.Orders.Add(order);
                    }

                    return userEntry;
                },
                splitOn: "OrderId"  // This indicates where to split the result set
            ).ToList();

            // Cache the result for 30 minutes
            _cache.Set(cacheKey, usersWithOrders, TimeSpan.FromMinutes(30)); 
            cachedUsersWithOrders = usersWithOrders;
        }
    }
    else
    {
        Console.WriteLine("Cache hit - fetching from cache...");
    }

    // Output the result
    foreach (var userWithOrders in cachedUsersWithOrders)
    {
        foreach (var order in userWithOrders.Orders)
        {
            Console.WriteLine($"Order ID: {order.Id}, Amount: {order.Amount}, " +
                              $"User: {userWithOrders.Name}, Email: {userWithOrders.Email}");
        }
    }

    return cachedUsersWithOrders;
}


public class UserWithOrders : User
{
    public List<Order> Orders { get; set; } = new List<Order>();
}
}
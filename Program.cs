// using Microsoft.Extensions.Configuration;
// using System;
// using System.IO;

// class Program
// {
//     static void Main(string[] args)
//     {
//         // Load configuration from appsettings.json
//         var configuration = new ConfigurationBuilder()
//             .SetBasePath(Directory.GetCurrentDirectory())
//             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
//             .Build();

//         var userRepository = new UserRepository(configuration);

//         // Fetch all users
//         foreach (var user in userRepository.GetAllUsers())
//         {
//             Console.WriteLine($"{user.Id}: {user.Name} - {user.Email}");
//         }

//         // Add two new users
//         var newUser1 = new User { Name = "David", Email = "david@example.com" };
//         var newUser2 = new User { Name = "Emma", Email = "emma@example.com" };

//         userRepository.AddUser(newUser1);
//         userRepository.AddUser(newUser2);

//         Console.WriteLine("\nTwo new users added successfully!");

//         // Fetch all users again to verify the new records were added
//         Console.WriteLine("\nUpdated User List:");
//         foreach (var user in userRepository.GetAllUsers())
//         {
//             Console.WriteLine($"{user.Id}: {user.Name} - {user.Email}");
//         }
//     }
//     }

using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

class Program
{
    static void Main(string[] args)
    {
        // Load configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();


        // Set up memory cache
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        // Initialize UserRepository with both configuration and memory cache
        var userRepository = new UserRepository(configuration, memoryCache);

        // Fetch all users
        foreach (var user in userRepository.GetAllUsers())
        {
            Console.WriteLine($"{user.Id}: {user.Name} - {user.Email}");
        }

        // Add two new users
        var newUser1 = new User { Name = "David", Email = "david@example.com" };
        var newUser2 = new User { Name = "Emma", Email = "emma@example.com" };

        userRepository.AddUser(newUser1);
        userRepository.AddUser(newUser2);

        Console.WriteLine("\nTwo new users added successfully!");

        // Fetch all users again to verify the new records were added
        Console.WriteLine("\nUpdated User List:");
        foreach (var user in userRepository.GetAllUsers())
        {
            Console.WriteLine($"{user.Id}: {user.Name} - {user.Email}");
        }

        // Fetch all users and their orders
        var usersWithOrders = userRepository.GetAllUsersWithOrders();

        foreach (var user2 in usersWithOrders)
        {
            Console.WriteLine($"User: {user2.Name} ({user2.Email})");

            if (user2.Orders.Count > 0)
            {
                foreach (var order in user2.Orders)
                {
                    Console.WriteLine($"  Order ID: {order.Id}, Date: {order.OrderDate}, Amount: {order.Amount}");
                }
            }
            else
            {
                Console.WriteLine("  No orders found.");
            }
        }


    }
}


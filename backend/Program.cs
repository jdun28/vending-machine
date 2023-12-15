namespace vending_machine;

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    static async Task Main()
    {
        var baseApiUrl = "http://localhost:5000/api/vendingmachine"; // Update with your API URL

        // Example: Purchase
        var purchaseTransaction = new Transaction
        {
            Items = new[] { "Soda", "Candy Bar" },
            AmountPaid = 1.55m
        };
        await MakeApiRequest($"{baseApiUrl}/purchase", HttpMethod.Post, purchaseTransaction);

        // Example: Refund
        var refundTransaction = new Transaction
        {
            Items = new[] { "Soda", "Candy Bar" },
            AmountPaid = 1.55m
        };
        await MakeApiRequest($"{baseApiUrl}/refund", HttpMethod.Post, refundTransaction);

        // Example: Get Inventory
        await MakeApiRequest($"{baseApiUrl}/inventory", HttpMethod.Get);

        // Example: Get Ledger
        await MakeApiRequest($"{baseApiUrl}/ledger", HttpMethod.Get);

        // Example: Get Transaction by ID
        await MakeApiRequest($"{baseApiUrl}/ledger/0", HttpMethod.Get);

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static async Task MakeApiRequest(string url, HttpMethod method, object content = null)
    {
        using (var client = new HttpClient())
        {
            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(url),
                Content = content != null
                    ? new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")
                    : null
            };

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Success - {result}");
            }
            else
            {
                Console.WriteLine($"Error - {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
    }
}


using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EmployeeAttendance.ConsoleClient
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static string baseUrl = "https://localhost:7001/api"; // Update with your actual API port
        private static string token = string.Empty;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Employee Attendance System - Admin Console Client");
            Console.WriteLine("==============================================");

            bool isLoggedIn = false;
            
            while (!isLoggedIn)
            {
                Console.Write("Username: ");
                var username = Console.ReadLine();
                
                Console.Write("Password: ");
                var password = ReadPassword();
                
                isLoggedIn = await Login(username!, password);
                
                if (!isLoggedIn)
                {
                    Console.WriteLine("Invalid login. Please try again.");
                }
            }

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nOptions:");
                Console.WriteLine("1. Upload Employees from CSV");
                Console.WriteLine("2. Upload Employees from JSON");
                Console.WriteLine("3. Exit");
                Console.Write("Select an option: ");

                var option = Console.ReadLine();
                
                switch (option)
                {
                    case "1":
                        await UploadEmployeesFromFile("csv");
                        break;
                    case "2":
                        await UploadEmployeesFromFile("json");
                        break;
                    case "3":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        static string ReadPassword()
        {
            var password = new StringBuilder();
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);
            
            Console.WriteLine();
            return password.ToString();
        }

        static async Task<bool> Login(string username, string password)
        {
            var loginData = new
            {
                Username = username,
                Password = password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginData),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync($"{baseUrl}/auth/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                // In a real app, we would extract the token from the response
                // For this demo, we'll just assume if login succeeded, we're good to go
                return true;
            }
            
            return false;
        }

        static async Task UploadEmployeesFromFile(string fileType)
        {
            Console.Write($"Enter path to {fileType.ToUpper()} file: ");
            var filePath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("File not found. Please check the path.");
                return;
            }

            if ((fileType == "csv" && !filePath.EndsWith(".csv")) || 
                (fileType == "json" && !filePath.EndsWith(".json")))
            {
                Console.WriteLine($"File must be a valid {fileType.ToUpper()} file.");
                return;
            }

            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
            
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                fileType == "csv" ? "text/csv" : "application/json");
            
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            try
            {
                var response = await client.PostAsync($"{baseUrl}/employee/import", form);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ImportResponse>(responseContent);
                    Console.WriteLine(result?.message);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private class ImportResponse
        {
            public string? message { get; set; }
        }
    }
}

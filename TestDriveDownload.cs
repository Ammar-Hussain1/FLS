using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0) 
        {
            Console.WriteLine("Please provide a URL.");
            return;
        }
        string url = args[0];
        Console.WriteLine($"Original URL: {url}");

        if (url.Contains("drive.google.com") && url.Contains("/d/"))
        {
            var match = Regex.Match(url, @"/d/([a-zA-Z0-9_-]+)");
            if (match.Success)
            {
                var fileId = match.Groups[1].Value;
                url = $"https://drive.google.com/uc?export=download&id={fileId}";
                Console.WriteLine($"Converted Drive URL: {url}");
            }
        }
        else if (url.Contains("docs.google.com/spreadsheets/d/"))
        {
            var match = Regex.Match(url, @"/d/([a-zA-Z0-9_-]+)");
            if (match.Success)
            {
                var fileId = match.Groups[1].Value;
                url = $"https://docs.google.com/spreadsheets/d/{fileId}/export?format=xlsx";
                Console.WriteLine($"Converted Sheets URL: {url}");
            }
        }

        using var client = new HttpClient();
        var response = await client.GetAsync(url);
        Console.WriteLine($"Status Code: {response.StatusCode}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsByteArrayAsync();
            Console.WriteLine($"Downloaded {content.Length} bytes.");
            System.IO.File.WriteAllBytes("test_download.xlsx", content);
            Console.WriteLine("Saved to test_download.xlsx");

            try
            {
                using var workbook = new XLWorkbook("test_download.xlsx");
                Console.WriteLine("Successfully opened workbook.");
                Console.WriteLine($"Worksheets: {workbook.Worksheets.Count}");
                foreach (var ws in workbook.Worksheets)
                {
                    Console.WriteLine($"- {ws.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening workbook: {ex.Message}");
            }
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error Body: {error}");
        }
    }
}

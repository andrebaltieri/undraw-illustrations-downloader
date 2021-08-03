using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace UnDrawnDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Download();
        }

        static async Task Download()
        {
            var hasMore = true;
            var page = 1;

            while (hasMore)
            {
                var url = $"https://undraw.co/api/illustrations?page={page}";
                var client = new HttpClient();
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<Root>(responseBody);

                Console.WriteLine($"Downloading page {page}");
                foreach (var item in data.illos)
                    await DownloadImage(item.slug, item.image);

                hasMore = data.hasMore;
                page = data.nextPage;
            }
        }

        static async Task DownloadImage(string title, string url)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var image = responseBody.Replace("6C63FF", "8525D2", StringComparison.OrdinalIgnoreCase);

            await using var sw = new StreamWriter($"imgs/{title.ToLower().Replace(" ", "_")}.svg");
            await sw.WriteAsync(image);
        }
    }


    public class Illo
    {
        public string _id { get; set; }
        public string title { get; set; }
        public string image { get; set; }
        public string slug { get; set; }
    }

    public class Root
    {
        public List<Illo> illos { get; set; }
        public bool hasMore { get; set; }
        public int nextPage { get; set; }
    }
}
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RedRoxxWebsite.Pages;

public class KampanyaModel : PageModel
{
    private readonly IWebHostEnvironment _environment;

    public KampanyaModel(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public Dictionary<string, List<MenuItem>> HappyHourByCategory { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public void OnGet()
    {
        var menuPath = Path.Combine(_environment.WebRootPath, "data", "menu.json");
        if (!System.IO.File.Exists(menuPath))
        {
            return;
        }

        var menuJson = System.IO.File.ReadAllText(menuPath);
        var menuItems = JsonSerializer.Deserialize<List<MenuItem>>(menuJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<MenuItem>();

        var categoryOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Mutlu Saat"] = 0,
            ["Alkollu Kokteyller"] = 4,
            ["Alkolsuz Kokteyller"] = 5,
            ["Biralar"] = 1,
            ["Shotlar"] = 6,
            ["Yemekler"] = 2,
            ["Aparatifler"] = 3,
            ["Mesrubatlar"] = 7,
            ["Tequila"] = 8,
            ["Viski"] = 9,
            ["Vodka"] = 10,
            ["Rom"] = 11,
            ["Gin"] = 12
        };

        HappyHourByCategory = menuItems
            .Where(item => item.IsHappyHour)
            .GroupBy(item => item.Category ?? "Menu")
            .OrderBy(group => categoryOrder.TryGetValue(group.Key, out var order) ? order : int.MaxValue)
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);
    }

    public class MenuItem
    {
        public string? Name { get; set; }
        public string? Category { get; set; }
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public string? Abv { get; set; }
        public bool IsHappyHour { get; set; }
        public int? Calories { get; set; }
        public int? Protein { get; set; }
        public int? Fat { get; set; }
        public int? Carbs { get; set; }
    }
}

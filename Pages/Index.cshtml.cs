using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RedRoxxWebsite.Pages;

public class IndexModel : PageModel
{
    private readonly IWebHostEnvironment _environment;

    public IndexModel(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public string BannerImage { get; private set; } = "/images/banner-default.svg";
    public List<MenuItem> HappyHourItems { get; private set; } = new();
    public Dictionary<string, List<MenuItem>> MenuByCategory { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public void OnGet()
    {
        var turkeyTimeZone = ResolveTurkeyTimeZone();
        var turkeyTime = turkeyTimeZone is null
            ? DateTime.UtcNow
            : TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);

        var dayName = turkeyTime.DayOfWeek.ToString();
        var bannerPath = Path.Combine(_environment.WebRootPath, "data", "bannerSchedule.json");
        var bannerSchedule = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (System.IO.File.Exists(bannerPath))
        {
            var bannerJson = System.IO.File.ReadAllText(bannerPath);
            bannerSchedule = JsonSerializer.Deserialize<Dictionary<string, string>>(bannerJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        if (bannerSchedule.TryGetValue(dayName, out var bannerFile))
        {
            BannerImage = $"/images/{bannerFile}";
        }
        else
        {
            BannerImage = "/images/banner-default.svg";
        }

        var menuPath = Path.Combine(_environment.WebRootPath, "data", "menu.json");
        if (System.IO.File.Exists(menuPath))
        {
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

            HappyHourItems = menuItems
                .Where(item => item.IsHappyHour)
                .OrderBy(item => categoryOrder.TryGetValue(item.Category ?? "", out var order) ? order : int.MaxValue)
                .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            MenuByCategory = menuItems
                .Where(item => !item.IsHappyHour)
                .GroupBy(item => item.Category ?? "Menu")
                .OrderBy(group => categoryOrder.TryGetValue(group.Key, out var order) ? order : int.MaxValue)
                .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);
        }
    }

    private static TimeZoneInfo? ResolveTurkeyTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return null;
            }
        }
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

using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RedRoxxWebsite.Pages;

public class MenuModel : PageModel
{
    private readonly IWebHostEnvironment _environment;

    public MenuModel(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public List<IndexModel.MenuItem> HappyHourItems { get; private set; } = new();
    public Dictionary<string, List<IndexModel.MenuItem>> MenuByCategory { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public void OnGet()
    {
        var turkeyTimeZone = ResolveTurkeyTimeZone();
        var turkeyTime = turkeyTimeZone is null
            ? DateTime.UtcNow
            : TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);

        var dayName = turkeyTime.DayOfWeek.ToString();
        var menuPath = Path.Combine(_environment.WebRootPath, "data", "menu.json");
        if (System.IO.File.Exists(menuPath))
        {
            var menuJson = System.IO.File.ReadAllText(menuPath);
            var menuItems = JsonSerializer.Deserialize<List<IndexModel.MenuItem>>(menuJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<IndexModel.MenuItem>();

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
}

using System.Text;
using System.Text.Json;

namespace RedRoxxWebsite;

public class StaticGenerator
{
    private readonly string _webRootPath;
    private readonly string _outputPath;

    public StaticGenerator(string webRootPath, string outputPath)
    {
        _webRootPath = webRootPath;
        _outputPath = outputPath;
    }

    public async Task GenerateAsync()
    {
        Console.WriteLine("🔨 Generating static site...");
        
        // Create output directory
        Directory.CreateDirectory(_outputPath);

        // Load menu data
        var menuPath = Path.Combine(_webRootPath, "data", "menu.json");
        if (!File.Exists(menuPath))
        {
            Console.WriteLine("❌ menu.json not found");
            return;
        }

        var menuJson = await File.ReadAllTextAsync(menuPath);
        var menuItems = JsonSerializer.Deserialize<List<MenuItem>>(menuJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<MenuItem>();

        // Generate index page
        await GenerateIndexPageAsync(menuItems);

        // Generate kampanya page
        await GenerateKampanyaPageAsync(menuItems);

        // Copy assets
        CopyAssets();

        Console.WriteLine("✅ Static site generated in: " + _outputPath);
    }

    private async Task GenerateIndexPageAsync(List<MenuItem> menuItems)
    {
        var categoryOrder = GetCategoryOrder();

        var happyHourItems = menuItems
            .Where(item => item.IsHappyHour)
            .OrderBy(item => categoryOrder.TryGetValue(item.Category ?? "", out var order) ? order : int.MaxValue)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var menuByCategory = menuItems
            .Where(item => !item.IsHappyHour)
            .GroupBy(item => item.Category ?? "Menu")
            .OrderBy(group => categoryOrder.TryGetValue(group.Key, out var order) ? order : int.MaxValue)
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);

        var html = new StringBuilder();
        html.Append(GetHeader());
        html.Append(GetBannerSection());
        html.Append(GetMenuSection(menuByCategory));
        html.Append(GetContactSection());
        html.Append(GetFooter());
        html.Append(GetSlideShowScript());

        await File.WriteAllTextAsync(Path.Combine(_outputPath, "index.html"), html.ToString());
        Console.WriteLine("  ✓ index.html generated");
    }

    private async Task GenerateKampanyaPageAsync(List<MenuItem> menuItems)
    {
        var categoryOrder = GetCategoryOrder();

        var happyHourByCategory = menuItems
            .Where(item => item.IsHappyHour)
            .GroupBy(item => item.Category ?? "Menu")
            .OrderBy(group => categoryOrder.TryGetValue(group.Key, out var order) ? order : int.MaxValue)
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);

        var html = new StringBuilder();
        html.Append(GetHeader("Kampanyalar"));
        html.Append(GetCampaignSection(happyHourByCategory));
        html.Append(GetContactSection());
        html.Append(GetFooter());

        await File.WriteAllTextAsync(Path.Combine(_outputPath, "kampanya.html"), html.ToString());
        Console.WriteLine("  ✓ kampanya.html generated");
    }

    private string GetHeader(string pageTitle = "RedRoxx")
    {
        return $@"<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <meta name=""description"" content=""RedRoxx, menü bölümleri, canlı müzik ve iletişim bilgileri içeren statik bir bar menü sitesi."" />
    <title>{pageTitle} - RedRoxx</title>
    <link rel=""stylesheet"" href=""css/site.css"" />
</head>
<body>
    <header class=""site-header"">
        <nav class=""site-nav"" aria-label=""Ana navigasyon"">
            <div class=""nav-group nav-left"">
                <a href=""index.html"" class=""nav-link"">Ana Sayfa</a>
                <a href=""kampanya.html"" class=""nav-link"">Kampanyalar</a>
            </div>
            <a href=""index.html"" class=""logo-link"" aria-label=""RedRoxx ana sayfa"">
                <img src=""images/logo.png"" alt=""RedRoxx logosu"" class=""site-logo"" loading=""eager"" decoding=""async"" />
            </a>
            <div class=""nav-group nav-right"">
                <a href=""index.html#menu"" class=""nav-link"">Menü</a>
                <a href=""index.html#contact"" class=""nav-link"">İletişim</a>
            </div>
        </nav>
    </header>

    <main role=""main"" class=""page-shell"">
";
    }

    private string GetBannerSection()
    {
        return @"        <section id=""live-music"" class=""banner-section"" aria-labelledby=""live-music-title"">
            <div class=""section-heading"">
                <h2 id=""live-music-title""></h2>
                <hr />
            </div>
            <div class=""slideshow"" aria-label=""Slayt gösterisi"">
                <div class=""slides"">
                    <img src=""images/slideshow1.jpg"" alt=""Slayt gösterisi 1"" class=""slide active"" loading=""lazy"" decoding=""async"" />
                    <img src=""images/slideshow2.jpg"" alt=""Slayt gösterisi 2"" class=""slide"" loading=""lazy"" decoding=""async"" />
                </div>
                <button class=""slide-control prev"" type=""button"" aria-label=""Önceki fotoğraf"">←</button>
                <button class=""slide-control next"" type=""button"" aria-label=""Sonraki fotoğraf"">→</button>
            </div>
            <div class=""banner-actions"">
                <a class=""banner-button"" href=""kampanya.html"">Kampanyalar</a>
            </div>
        </section>
";
    }

    private string GetMenuSection(Dictionary<string, List<MenuItem>> menuByCategory)
    {
        var categoryIcons = GetCategoryIcons();
        var html = new StringBuilder();
        html.Append(@"        <section id=""menu"" class=""menu-section"" aria-labelledby=""menu-title"">
            <div class=""section-heading"">
                <h2 id=""menu-title"">Menü</h2>
                <hr />
            </div>
");

        foreach (var category in menuByCategory)
        {
            var iconPath = categoryIcons.TryGetValue(category.Key, out var icon) ? icon : "/images/beer.svg";
            html.Append($@"            <article class=""category-block"">
                <div class=""category-title-row"">
                    <img src=""{iconPath}"" alt="""" class=""category-icon"" loading=""lazy"" decoding=""async"" />
                    <h3>{category.Key}</h3>
                </div>
                <div class=""menu-grid"">
");

            foreach (var item in category.Value)
            {
                html.Append(RenderMenuItem(item));
            }

            html.Append(@"                </div>
            </article>
");
        }

        html.Append("        </section>\n");
        return html.ToString();
    }

    private string GetCampaignSection(Dictionary<string, List<MenuItem>> campaignsByCategory)
    {
        var categoryIcons = GetCategoryIcons();
        var html = new StringBuilder();
        html.Append(@"        <section id=""campaigns"" class=""menu-section"" aria-labelledby=""campaigns-title"">
            <div class=""section-heading"">
                <h2 id=""campaigns-title"">Kampanyalar</h2>
                <hr />
            </div>
");

        foreach (var category in campaignsByCategory)
        {
            var iconPath = categoryIcons.TryGetValue(category.Key, out var icon) ? icon : "/images/beer.svg";
            html.Append($@"            <article class=""category-block"">
                <div class=""category-title-row"">
                    <img src=""{iconPath}"" alt="""" class=""category-icon"" loading=""lazy"" decoding=""async"" />
                    <h3>{category.Key}</h3>
                </div>
                <div class=""menu-grid"">
");

            foreach (var item in category.Value)
            {
                html.Append(RenderMenuItemWithTag(item));
            }

            html.Append(@"                </div>
            </article>
");
        }

        html.Append("        </section>\n");
        return html.ToString();
    }

    private string RenderMenuItem(MenuItem item)
    {
        var imagePath = item.ImagePath ?? "/images/beer.svg";
        var html = new StringBuilder($@"                    <div class=""menu-card"">
                        <img src=""{imagePath}"" alt=""{item.Name}"" class=""item-image"" loading=""lazy"" decoding=""async"" />
                        <div class=""menu-card-content"">
                            <div class=""menu-card-header"">
                                <h4>{item.Name}</h4>
                                <span class=""price"">{item.Price:F2} TL</span>
                            </div>
");

        if (!string.IsNullOrWhiteSpace(item.Abv))
        {
            html.Append($@"                            <p class=""detail"">ABV {item.Abv}</p>
");
        }

        if (item.Calories.HasValue || item.Protein.HasValue || item.Fat.HasValue || item.Carbs.HasValue)
        {
            html.Append(@"                            <ul class=""nutrition-list"" aria-label=""Besin bilgileri"">
");
            if (item.Calories.HasValue)
                html.Append($@"                                <li><span>Kalori</span><strong>{item.Calories}</strong></li>
");
            if (item.Protein.HasValue)
                html.Append($@"                                <li><span>Protein</span><strong>{item.Protein} g</strong></li>
");
            if (item.Fat.HasValue)
                html.Append($@"                                <li><span>Yağ</span><strong>{item.Fat} g</strong></li>
");
            if (item.Carbs.HasValue)
                html.Append($@"                                <li><span>Karbonhidrat</span><strong>{item.Carbs} g</strong></li>
");
            html.Append(@"                            </ul>
");
        }

        html.Append(@"                        </div>
                    </div>
");
        return html.ToString();
    }

    private string RenderMenuItemWithTag(MenuItem item)
    {
        var imagePath = item.ImagePath ?? "/images/beer.svg";
        var html = new StringBuilder($@"                    <div class=""menu-card"">
                        <img src=""{imagePath}"" alt=""{item.Name}"" class=""item-image"" loading=""lazy"" decoding=""async"" />
                        <div class=""menu-card-content"">
                            <div class=""menu-card-header"">
                                <h4>{item.Name}</h4>
                                <span class=""price"">{item.Price:F2} TL</span>
                            </div>
                            <span class=""tag"">Canlı müzikte geçerli değildir.</span>
");

        if (!string.IsNullOrWhiteSpace(item.Abv))
        {
            html.Append($@"                            <p class=""detail"">ABV {item.Abv}</p>
");
        }

        if (item.Calories.HasValue || item.Protein.HasValue || item.Fat.HasValue || item.Carbs.HasValue)
        {
            html.Append(@"                            <ul class=""nutrition-list"" aria-label=""Besin bilgileri"">
");
            if (item.Calories.HasValue)
                html.Append($@"                                <li><span>Kalori</span><strong>{item.Calories}</strong></li>
");
            if (item.Protein.HasValue)
                html.Append($@"                                <li><span>Protein</span><strong>{item.Protein} g</strong></li>
");
            if (item.Fat.HasValue)
                html.Append($@"                                <li><span>Yağ</span><strong>{item.Fat} g</strong></li>
");
            if (item.Carbs.HasValue)
                html.Append($@"                                <li><span>Karbonhidrat</span><strong>{item.Carbs} g</strong></li>
");
            html.Append(@"                            </ul>
");
        }

        html.Append(@"                        </div>
                    </div>
");
        return html.ToString();
    }

    private string GetContactSection()
    {
        return @"        <section id=""contact"" class=""contact-section"" aria-labelledby=""contact-title"">
            <div class=""section-heading"">
                <h2 id=""contact-title"">Rezervasyon ve fazlası için</h2>
                <hr />
            </div>
            <div class=""contact-grid"">
                <div>
                    <h3>Telefon</h3>
                    <p>0532 207 17 33</p>
                </div>
                <div>
                    <h3>Sosyal Medya</h3>
                    <p><a href=""https://www.instagram.com/redroxx_live/"" target=""_blank"" rel=""noreferrer"">Instagram</a><br /><a href=""https://www.instagram.com/redroxx_live/"" target=""_blank"" rel=""noreferrer"">X</a></p>
                </div>
            </div>
        </section>
";
    }

    private string GetFooter()
    {
        return @"    </main>

    <footer class=""site-footer"">
        <p>RedRoxx • Kazimdirik Mah. Suvari Cad. No68B • Bornova/Izmir</p>
        <p>Her aksam canlı müzik • Rezervasyon önerilir</p>
    </footer>

";
    }

    private string GetSlideShowScript()
    {
        return @"    <script>
        (() => {
            const slideshow = document.querySelector('.slideshow');
            if (!slideshow) return;

            const slides = Array.from(slideshow.querySelectorAll('.slide'));
            const prevButton = slideshow.querySelector('.slide-control.prev');
            const nextButton = slideshow.querySelector('.slide-control.next');
            let currentIndex = 0;
            let intervalId = null;

            const showSlide = (index) => {
                slides[currentIndex]?.classList.remove('active');
                currentIndex = (index + slides.length) % slides.length;
                slides[currentIndex]?.classList.add('active');
            };

            const goNext = () => showSlide(currentIndex + 1);
            const goPrev = () => showSlide(currentIndex - 1);

            prevButton?.addEventListener('click', () => {
                goPrev();
                resetAutoRotate();
            });
            nextButton?.addEventListener('click', () => {
                goNext();
                resetAutoRotate();
            });

            const resetAutoRotate = () => {
                if (intervalId) {
                    clearInterval(intervalId);
                }
                intervalId = setInterval(goNext, 5000);
            };

            resetAutoRotate();
        })();
    </script>
</body>
</html>";
    }

    private Dictionary<string, int> GetCategoryOrder()
    {
        return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
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
    }

    private Dictionary<string, string> GetCategoryIcons()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Bira"] = "images/beer.svg",
            ["Biralar"] = "images/beer.svg",
            ["Kokteyller"] = "images/cocktail.svg",
            ["Alkollu Kokteyller"] = "images/cocktail.svg",
            ["Alkolsuz Kokteyller"] = "images/mocktail.svg",
            ["Shotlar"] = "images/cocktail.svg",
            ["Viski"] = "images/cocktail.svg",
            ["Gin"] = "images/cocktail.svg",
            ["Tequila"] = "images/cocktail.svg",
            ["Vodka"] = "images/cocktail.svg",
            ["Rom"] = "images/cocktail.svg",
            ["Yiyecek"] = "images/food-burger.svg",
            ["Mesrubatlar"] = "images/mocktail.svg",
            ["Meşrubat"] = "images/mocktail.svg"
        };
    }

    private void CopyAssets()
    {
        var cssSource = Path.Combine(_webRootPath, "css");
        var cssTarget = Path.Combine(_outputPath, "css");
        CopyDirectory(cssSource, cssTarget);

        var imagesSource = Path.Combine(_webRootPath, "images");
        var imagesTarget = Path.Combine(_outputPath, "images");
        CopyDirectory(imagesSource, imagesTarget);

        var jsSource = Path.Combine(_webRootPath, "js");
        var jsTarget = Path.Combine(_outputPath, "js");
        CopyDirectory(jsSource, jsTarget);

        Console.WriteLine("  ✓ Assets copied (css, images, js)");
    }

    private void CopyDirectory(string source, string target)
    {
        if (!Directory.Exists(source)) return;

        Directory.CreateDirectory(target);

        foreach (var file in Directory.GetFiles(source))
        {
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
        }

        foreach (var dir in Directory.GetDirectories(source))
        {
            CopyDirectory(dir, Path.Combine(target, Path.GetFileName(dir)));
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

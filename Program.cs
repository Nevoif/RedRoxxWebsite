using RedRoxxWebsite;

// Generate static site if --generate argument is passed
if (args.Contains("--generate"))
{
    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "docs");
    
    var generator = new StaticGenerator(webRootPath, outputPath);
    await generator.GenerateAsync();
    return;
}

// Otherwise run the web app
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

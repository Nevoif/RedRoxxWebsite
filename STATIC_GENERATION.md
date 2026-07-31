# RedRoxx Website - Static Generation

This website can be deployed as a **completely static site** on GitHub Pages while keeping everything in **C# and Razor**.

## How It Works

1. **Source Code**: All pages are defined in C# and Razor (`Pages/*.cshtml`)
2. **Menu Data**: Items are stored in `wwwroot/data/menu.json`
3. **Static Generator**: A C# tool pre-generates HTML files at build time
4. **GitHub Pages**: The generated HTML is served from the `docs/` folder

## Generate Static Site

### Option 1: Using the Script (Linux/macOS)
```bash
chmod +x generate-static.sh
./generate-static.sh
```

### Option 2: Manual Command
```bash
dotnet build
dotnet run -- --generate
```

This will:
- Read `menu.json`
- Generate `index.html` and `kampanya.html`
- Copy all assets (CSS, JS, images)
- Output to the `docs/` folder

## Deploy to GitHub Pages

1. **Generate static site** (see above)
2. **Commit and push**:
   ```bash
   git add docs/
   git commit -m "Update static site"
   git push origin master
   ```

3. **Enable GitHub Pages** in repository settings:
   - Go to **Settings → Pages**
   - **Source**: Select "Deploy from a branch"
   - **Branch**: `master`
   - **Folder**: `/docs`
   - Save

Your site will be live at: `https://yourusername.github.io/RedRoxxWebsite/`

## Development

To develop locally:
```bash
dotnet run
```
Visit: `http://localhost:5248`

## Architecture

```
RedRoxxWebsite/
├── Pages/
│   ├── Index.cshtml        (still works for local dev)
│   ├── Index.cshtml.cs
│   ├── Kampanya.cshtml     (still works for local dev)
│   └── Kampanya.cshtml.cs
├── Services/
│   └── StaticGenerator.cs  (generates HTML from C# and menu.json)
├── wwwroot/
│   ├── data/
│   │   └── menu.json       (menu items data)
│   ├── css/
│   ├── js/
│   └── images/
├── docs/                   (generated static HTML - deployed to GitHub Pages)
│   ├── index.html
│   ├── kampanya.html
│   ├── css/
│   ├── js/
│   └── images/
└── Program.cs              (entry point - detects --generate flag)
```

## Benefits

✅ **No JavaScript needed** - stays pure C# and Razor  
✅ **Free hosting** on GitHub Pages  
✅ **Fast static HTML** - no server processing  
✅ **Easy to maintain** - keep using Razor and C#  
✅ **Automatic updates** - regenerate anytime menu.json changes  

## Troubleshooting

**Q: I don't see the generated files**  
A: Make sure you ran `dotnet run -- --generate` (note the `--` separator)

**Q: Assets aren't showing**  
A: Make sure CSS, JS, and images exist in `wwwroot/` before generating

**Q: GitHub Pages isn't working**  
A: Check Settings → Pages and verify:
- Source is set to "Deploy from a branch"
- Branch is `master` and folder is `/docs`

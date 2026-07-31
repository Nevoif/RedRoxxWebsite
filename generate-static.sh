#!/bin/bash
# Generate static site and push to GitHub Pages

echo "🔨 Building and generating static site..."

# Build the project
dotnet build

# Generate static files
dotnet run --project RedRoxxWebsite.csproj -- --generate

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Static site generated in 'docs/' folder"
    echo ""
    echo "📋 To deploy:"
    echo "   git add docs/"
    echo "   git commit -m 'Update static site'"
    echo "   git push origin master"
    echo ""
    echo "📚 Enable GitHub Pages on your repository:"
    echo "   Settings → Pages → Source: Deploy from branch → docs/"
else
    echo "❌ Failed to generate static site"
    exit 1
fi

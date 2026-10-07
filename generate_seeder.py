import os
import re
import json

registry_file = r"e:\Willovate_store\willovate-store-ui\src\templates\registry.ts"
templates_dir = r"e:\Willovate_store\willovate-store-ui\src\templates\food-restaurants"
output_file = r"e:\Willovate_store\willovate-store-api\src\Willovate.Store.Api\Data\TemplateSeeder.cs"

# Parse registry
categories = {}
with open(registry_file, 'r', encoding='utf-8') as f:
    content = f.read()
    cat_blocks = re.findall(r"createCategory\('([^']+)',\s*'([^']+)',\s*'([^']+)',.*?\[(.*?)\]\)", content, re.DOTALL)
    for cat_id, cat_name, cat_slug, tpl_list in cat_blocks:
        tpls = re.findall(r"\['([^']+)',\s*'([^']+)',\s*'([^']+)'", tpl_list)
        categories[cat_id] = tpls

def extract_config(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
        match = re.search(r"const theme: RestaurantThemeConfig = ({.*});\s*export default", content, re.DOTALL)
        if match:
            # We will just treat it as a raw string for now since it's TS, it's hard to parse to JSON.
            # But we can extract basic details to store in the DB.
            return True
    return False

csharp_code = """using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Data;

public static class TemplateSeeder
{
    public static async Task SeedAsync(StoreDbContext dbContext)
    {
        if (await dbContext.Templates.AnyAsync()) return;

        var templates = new List<Template>
        {
"""

display_order = 1
for cat_id, tpls in categories.items():
    for name, slug, filename in tpls:
        csharp_code += f"""
            new Template
            {{
                Id = Guid.NewGuid(),
                TemplateId = "{slug}",
                Name = "{name}",
                Category = "{cat_id}",
                Description = "{name} template for {cat_id}",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = {display_order},
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{{}}",
                SectionConfiguration = "{{}}",
                ImageConfiguration = "{{}}"
            }},"""
        display_order += 1

csharp_code += """
        };

        dbContext.Templates.AddRange(templates);
        await dbContext.SaveChangesAsync();
    }
}
"""

with open(output_file, 'w', encoding='utf-8') as f:
    f.write(csharp_code)

print("Generated TemplateSeeder.cs")

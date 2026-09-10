using System.Text.RegularExpressions;
using Willovate.Store.Api.Controllers;

namespace Willovate.Store.Api.Services;

public sealed class AiService : IAiService
{
    // Preset banner image URLs for fashion/lifestyle stores
    private static readonly string[] BannerImages =
    [
        "https://images.unsplash.com/photo-1441984904996-e0b6ba687e04?w=1200&q=80",
        "https://images.unsplash.com/photo-1445205170230-053b83016050?w=1200&q=80",
        "https://images.unsplash.com/photo-1469334031218-e382a71b716b?w=1200&q=80",
        "https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=1200&q=80",
        "https://images.unsplash.com/photo-1525507119028-ed4c629a60a3?w=1200&q=80",
        "https://images.unsplash.com/photo-1490481651871-ab68de25d43d?w=1200&q=80",
        "https://images.unsplash.com/photo-1558769132-cb1aea458c5e?w=1200&q=80",
        "https://images.unsplash.com/photo-1600185365483-26d7a4cc7519?w=1200&q=80",
        "https://images.unsplash.com/photo-1491553895911-0055eca6402d?w=1200&q=80",
        "https://images.unsplash.com/photo-1483985988355-763728e1935b?w=1200&q=80",
        "https://images.unsplash.com/photo-1485230895905-ef082490cc32?w=1200&q=80",
    ];

    private static readonly Random Rng = new();

    public AiChatResponse ProcessMessage(string message, string? context)
    {
        var lower = message.ToLowerInvariant();

        // --- HEADING / TITLE commands ---
        if (ContainsAny(lower, "heading", "title", "headline", "h1"))
        {
            if (ContainsAny(lower, "change", "set", "make", "update", "write", "generate", "give"))
            {
                // Try to extract explicit text from quotes
                var quoted = ExtractQuoted(message);
                string newHeading = quoted ?? PickHeading(lower);

                return new AiChatResponse(
                    $"Great choice! I'll update the heading to: \"{newHeading}\". Click **Apply** to set it.",
                    new AiAction("update_heading", newHeading, null, "heading"));
            }
            return new AiChatResponse(
                "To change the heading, tell me what you'd like it to say! For example:\n\"Change heading to: Summer Must-Haves\"\n\nOr I can suggest one — just say \"Write a heading for me\".",
                null);
        }

        // --- DESCRIPTION / SUBTITLE / TEXT commands ---
        if (ContainsAny(lower, "description", "subtitle", "subheading", "text", "paragraph", "content"))
        {
            if (ContainsAny(lower, "change", "set", "make", "update", "write", "generate", "give", "improve"))
            {
                var quoted = ExtractQuoted(message);
                string newDesc = quoted ?? PickDescription(lower);

                return new AiChatResponse(
                    $"Here's a compelling description for your store:\n\n\"{newDesc}\"\n\nClick **Apply** to use it.",
                    new AiAction("update_description", newDesc, null, "text"));
            }
            return new AiChatResponse(
                "What kind of description would you like? I can write one for you — just say \"Write a description\" and I'll create something compelling for your store.",
                null);
        }

        // --- BUTTON commands ---
        if (ContainsAny(lower, "button", "cta", "call to action", "shop now", "click"))
        {
            if (ContainsAny(lower, "change", "set", "make", "update", "write", "generate"))
            {
                var quoted = ExtractQuoted(message);
                string newBtn = quoted ?? PickButton();

                return new AiChatResponse(
                    $"I'll update your button text to: \"{newBtn}\". Click **Apply** to set it.",
                    new AiAction("update_button", newBtn, null, "button"));
            }
            return new AiChatResponse(
                "I can suggest a button label for you! Say something like:\n\"Change the button to: Shop the Collection\"\n\nOr say \"Give me a button idea\" and I'll suggest one.",
                null);
        }

        // --- BANNER / BACKGROUND IMAGE commands ---
        if (ContainsAny(lower, "banner", "background", "image", "photo", "picture", "hero image", "cover"))
        {
            if (ContainsAny(lower, "change", "set", "update", "use", "apply", "pick", "choose", "random", "new", "different"))
            {
                var imageUrl = BannerImages[Rng.Next(BannerImages.Length)];
                return new AiChatResponse(
                    "I've picked a beautiful banner image for your Summer Collection! Click **Apply** to set it as the background.",
                    new AiAction("update_banner", null, imageUrl, "hero"));
            }
            return new AiChatResponse(
                "I can change your banner/background image! Just say:\n• \"Change the banner image\"\n• \"Pick a new background photo\"\n• \"Use a different hero image\"\n\nI'll select a beautiful lifestyle image for you.",
                null);
        }

        // --- IMPROVE commands ---
        if (ContainsAny(lower, "improve", "better", "enhance", "rewrite", "optimize", "fix"))
        {
            if (ContainsAny(lower, "heading", "title", "h1"))
            {
                var heading = PickHeading(lower);
                return new AiChatResponse(
                    $"Here's an improved heading:\n\n\"{heading}\"\n\nClick **Apply** to use it on your page.",
                    new AiAction("update_heading", heading, null, "heading"));
            }
            var desc = PickDescription(lower);
            return new AiChatResponse(
                $"Here's improved content for your store:\n\n\"{desc}\"\n\nClick **Apply** to update your page.",
                new AiAction("update_description", desc, null, "text"));
        }

        // --- LAYOUT / DESIGN advice ---
        if (ContainsAny(lower, "layout", "design", "structure", "look", "feel", "style"))
        {
            return new AiChatResponse(
                "Here are some design tips for your store:\n\n• **Clear hero section** – Keep the heading bold and brief (4–7 words)\n• **Strong CTA** – Use action words: Shop Now, Explore, Discover\n• **White space** – Don't crowd elements; breathing room improves trust\n• **Consistent colors** – Stick to 2–3 brand colors throughout\n• **Mobile-first** – Test using the Mobile/Tablet view buttons at the bottom\n\nWould you like me to suggest specific changes for any section?",
                null);
        }

        // --- HELP commands ---
        if (ContainsAny(lower, "help", "what can you do", "how", "guide", "tutorial"))
        {
            return new AiChatResponse(
                "I can help you with:\n\n• **Change heading** – \"Change heading to: New Title\"\n• **Write description** – \"Write a description for my store\"\n• **Update button** – \"Change the button to: Shop Now\"\n• **Change banner image** – \"Change the banner image\"\n• **Improve content** – \"Improve the heading\"\n• **Design tips** – \"Give me design advice\"\n\nJust tell me what you need!",
                null);
        }

        // --- Default / fallback ---
        return new AiChatResponse(
            $"I'm here to help! Here are some things you can ask me:\n\n• \"Write a heading for my store\"\n• \"Write a description\"\n• \"Change the banner image\"\n• \"Improve the content\"\n• \"Give me design tips\"\n\nWhat would you like to change?",
            null);
    }

    private static bool ContainsAny(string text, params string[] keywords)
        => keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));

    private static string? ExtractQuoted(string message)
    {
        // Match text in double or single quotes, or after "to:" pattern
        var doubleQuote = Regex.Match(message, "\"([^\"]+)\"");
        if (doubleQuote.Success) return doubleQuote.Groups[1].Value.Trim();

        var singleQuote = Regex.Match(message, "'([^']+)'");
        if (singleQuote.Success) return singleQuote.Groups[1].Value.Trim();

        // Match "to: Some text" pattern
        var toPattern = Regex.Match(message, @"(?:to|into)\s*:\s*(.+)", RegexOptions.IgnoreCase);
        if (toPattern.Success) return toPattern.Groups[1].Value.Trim().Trim('"', '\'');

        return null;
    }

    private static string PickHeading(string context)
    {
        if (context.Contains("summer")) return "Summer Collection 2025";
        if (context.Contains("luxury") || context.Contains("premium")) return "Elevate Your Everyday";
        if (context.Contains("sale") || context.Contains("discount")) return "Shop the Season's Best";
        if (context.Contains("new")) return "New Arrivals — Just Dropped";

        string[] headings =
        [
            "Timeless Style, Modern Edge",
            "Curated for the Discerning You",
            "Where Fashion Meets Elegance",
            "Discover Your Signature Style",
            "Effortless Luxury, Every Day",
            "Bold Choices. Beautiful Results.",
            "The New Standard in Style",
        ];
        return headings[Rng.Next(headings.Length)];
    }

    private static string PickDescription(string context)
    {
        if (context.Contains("summer")) return "Light fabrics, vibrant colors, and modern cuts — everything you need for a perfect summer wardrobe.";
        if (context.Contains("luxury") || context.Contains("premium")) return "Handcrafted with the finest materials, each piece tells a story of timeless elegance and exceptional quality.";

        string[] descriptions =
        [
            "We believe fashion is more than clothing — it's a statement of who you are. Explore our curated collection and find pieces that speak to your style.",
            "From everyday essentials to statement pieces, our collection is thoughtfully curated to help you dress with intention and confidence.",
            "Quality craftsmanship meets contemporary design. Every item in our store is chosen for its beauty, durability, and the confidence it brings.",
            "Discover a world of carefully selected fashion that blends classic sophistication with modern flair. Your perfect look is just a click away.",
        ];
        return descriptions[Rng.Next(descriptions.Length)];
    }

    private static string PickButton()
    {
        string[] buttons =
        [
            "Shop the Collection",
            "Explore Now →",
            "Discover More",
            "Shop Now",
            "View All Styles",
            "Find Your Look",
            "Get Started",
        ];
        return buttons[Rng.Next(buttons.Length)];
    }
}

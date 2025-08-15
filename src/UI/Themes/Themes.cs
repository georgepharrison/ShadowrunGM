using MudBlazor;

namespace ShadowrunGM.UI;

public static class Themes
{
    #region Public Members

    public static readonly MudTheme MinimalistTheme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#4A90E2",
            Secondary = "#7B8A8B",
            Background = "#F9FAFB",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#111827",
            DrawerBackground = "#FFFFFF",
            TextPrimary = "#111827",
            TextSecondary = "#6B7280",
            Divider = "#E5E7EB",
        },
        PaletteDark = new PaletteDark
        {
            // ChatGPT-like neutrals
            Background = "#212121",
            Surface = "#2A2A2A",
            AppbarBackground = "#212121",
            DrawerBackground = "#212121",

            // Primary = ChatGPT green
            Primary = "#10A37F",
            // Keep a neutral secondary for subtle outlines/text
            Secondary = "#8A8A8A",

            // Text
            TextPrimary = "#E5E7EB",
            TextSecondary = "#B0B0B0",

            // Optional: crisper dividers without bluish tint
            Divider = "#3A3A3A",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px",
            AppbarHeight = "48px",
            DrawerWidthRight = "360px"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Inter", "Roboto", "system-ui", "-apple-system", "Segoe UI", "sans-serif"], FontSize = "0.95rem" },
            H1 = new H1Typography { FontSize = "2rem", FontWeight = "600" },
            H2 = new H2Typography { FontSize = "1.5rem", FontWeight = "500" },
            H3 = new H3Typography { FontSize = "1.25rem", FontWeight = "500" },
            Body1 = new Body1Typography { FontSize = "0.95rem" },
            Body2 = new Body2Typography { FontSize = "0.85rem" }
        }
    };

    #endregion Public Members
}
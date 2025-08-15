using MudBlazor;

namespace ShadowrunGM.UI;

public static class Themes
{
    #region Public Members

    // Single theme object with both palettes; we’ll toggle dark mode on the provider.
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
            Primary = "#4A90E2",
            Secondary = "#9CA3AF",
            Background = "#111827",
            Surface = "#1F2937",
            AppbarBackground = "#1F2937",
            AppbarText = "#F9FAFB",
            DrawerBackground = "#1F2937",
            TextPrimary = "#F9FAFB",
            TextSecondary = "#D1D5DB",
            Divider = "#374151",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px",
            AppbarHeight = "48px",
            DrawerWidthRight = "360px"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Inter", "Segoe UI", "Roboto", "sans-serif"], FontSize = "0.95rem" },
            H1 = new H1Typography { FontSize = "2rem", FontWeight = "600" },
            H2 = new H2Typography { FontSize = "1.5rem", FontWeight = "500" },
            H3 = new H3Typography { FontSize = "1.25rem", FontWeight = "500" },
            Body1 = new Body1Typography { FontSize = "0.95rem" },
            Body2 = new Body2Typography { FontSize = "0.85rem" }
        }
    };

    #endregion Public Members
}
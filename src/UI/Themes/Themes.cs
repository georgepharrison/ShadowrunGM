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
        PaletteDark = new()
        {
            Background = "#0B0F17",
            Surface = "#111827",
            Primary = "#2563EB",
            Secondary = "#64748B",
            AppbarBackground = "#0B0F17",
            DrawerBackground = "#0B0F17",
            TextPrimary = "#E5E7EB",
            TextSecondary = "#9CA3AF",
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
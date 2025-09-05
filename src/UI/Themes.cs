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
            DefaultBorderRadius = "6px",
            // Mobile-optimized heights - larger touch targets
            AppbarHeight = "56px", // Increased from 48px for better touch
            DrawerWidthRight = "360px",
            DrawerWidthLeft = "280px" // Narrower left drawer for mobile
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Inter", "Roboto", "system-ui", "-apple-system", "Segoe UI", "sans-serif"], FontSize = "0.95rem" },
            // Mobile-optimized typography - larger sizes for readability
            H1 = new H1Typography { FontSize = "1.75rem", FontWeight = "600" }, // Reduced for mobile screens
            H2 = new H2Typography { FontSize = "1.375rem", FontWeight = "500" }, // Reduced for mobile screens
            H3 = new H3Typography { FontSize = "1.125rem", FontWeight = "500" }, // Reduced for mobile screens
            H4 = new H4Typography { FontSize = "1rem", FontWeight = "500" }, // Added for better hierarchy
            H5 = new H5Typography { FontSize = "0.875rem", FontWeight = "500" }, // Added for better hierarchy
            H6 = new H6Typography { FontSize = "0.75rem", FontWeight = "500" }, // Added for better hierarchy
            Body1 = new Body1Typography { FontSize = "1rem" }, // Slightly larger for mobile readability
            Body2 = new Body2Typography { FontSize = "0.875rem" }, // Adjusted for consistency
            Button = new ButtonTypography { FontSize = "0.875rem", FontWeight = "500" }, // Optimized button text
            Caption = new CaptionTypography { FontSize = "0.75rem" } // For small text like labels
        }
    };

    #endregion Public Members
}
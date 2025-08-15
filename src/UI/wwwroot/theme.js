// wwwroot/theme.js
window.themeStorage = {
    // Returns true if dark
    init() {
        // If the pre-head script already chose a class, honor that.
        const isDark = document.documentElement.classList.contains('dark');
        try {
            // Ensure storage matches whatever class we landed on
            localStorage.setItem('theme', isDark ? 'dark' : 'light');
        } catch { /* ignore */ }
        return isDark;
    },
    // Returns true if dark
    get() {
        try {
            const t = localStorage.getItem('theme');
            return t ? t === 'dark' : document.documentElement.classList.contains('dark');
        } catch {
            return document.documentElement.classList.contains('dark');
        }
    },
    // Sets html class + storage
    set(isDark) {
        document.documentElement.classList.toggle('dark', isDark);
        document.documentElement.classList.toggle('light', !isDark);
        try { localStorage.setItem('theme', isDark ? 'dark' : 'light'); } catch { /* ignore */ }
    }
};
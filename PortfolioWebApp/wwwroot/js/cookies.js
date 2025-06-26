// wwwroot/js/cookies.js
window.cookieHelper = {
    setDarkMode: function (isDark) {
        document.cookie = "darkMode=" + isDark + "; path=/; max-age=31536000"; // 1 Jahr
    }
};
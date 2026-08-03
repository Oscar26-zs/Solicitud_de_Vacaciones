(function () {
  var themeKey = 'vacaciones-theme';

  function applyTheme(theme) {
    document.body.classList.toggle('dark', theme === 'dark');
    localStorage.setItem(themeKey, theme);
  }

  var saved = localStorage.getItem(themeKey);
  if (saved) {
    applyTheme(saved);
  } else if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
    applyTheme('dark');
  }

  var themeToggle = document.querySelector('[data-theme-toggle]');
  if (themeToggle) {
    themeToggle.addEventListener('click', function () {
      applyTheme(document.body.classList.contains('dark') ? 'light' : 'dark');
    });
  }

  var userMenuTrigger = document.querySelector('[data-user-menu-trigger]');
  var userMenuDropdown = document.querySelector('[data-user-menu-dropdown]');
  if (userMenuTrigger && userMenuDropdown) {
    userMenuTrigger.addEventListener('click', function (e) {
      e.stopPropagation();
      var open = userMenuDropdown.classList.toggle('user-menu-dropdown--open');
      userMenuTrigger.setAttribute('aria-expanded', open ? 'true' : 'false');
    });
    document.addEventListener('click', function () {
      userMenuDropdown.classList.remove('user-menu-dropdown--open');
      userMenuTrigger.setAttribute('aria-expanded', 'false');
    });
  }
})();
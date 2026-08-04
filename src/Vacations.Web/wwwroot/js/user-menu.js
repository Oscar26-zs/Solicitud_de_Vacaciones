/* user-menu.js — Menú desplegable de usuario */
(function () {
  function init() {
    var triggers = document.querySelectorAll('[data-user-menu-trigger]');

    triggers.forEach(function (trigger) {
      trigger.addEventListener('click', function (e) {
        e.stopPropagation();
        var menu = trigger.nextElementSibling;
        var isOpen = trigger.getAttribute('aria-expanded') === 'true';

        closeAll();
        if (!isOpen) {
          trigger.setAttribute('aria-expanded', 'true');
          if (menu) {
            menu.classList.add('user-menu-dropdown--open');
          }
        }
      });
    });

    // Cerrar al hacer clic fuera
    document.addEventListener('click', function (e) {
      if (!e.target.closest('.user-menu')) {
        closeAll();
      }
    });

    // Cerrar con Escape
    document.addEventListener('keydown', function (e) {
      if (e.key === 'Escape') {
        closeAll();
      }
    });
  }

  function closeAll() {
    document.querySelectorAll('[data-user-menu-trigger]').forEach(function (t) {
      t.setAttribute('aria-expanded', 'false');
      var menu = t.nextElementSibling;
      if (menu) {
        menu.classList.remove('user-menu-dropdown--open');
      }
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
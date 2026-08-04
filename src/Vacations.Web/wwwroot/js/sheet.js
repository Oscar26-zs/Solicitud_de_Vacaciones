/* sheet.js — Sheets / Drawers (crear y editar solicitud) */
(function () {
  function getContainer(el) {
    return el.closest('[data-sheet-container]');
  }

  function setOpen(container, open) {
    if (!container) return;
    container.classList.toggle('sheet-container--open', open);
    var overlay = container.querySelector('.sheet-overlay');
    var sheet = container.querySelector('.sheet');
    if (overlay) overlay.classList.toggle('sheet-overlay--open', open);
    if (sheet) sheet.classList.toggle('sheet--open', open);
    if (open) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
  }

  document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-sheet-trigger]').forEach(function (trigger) {
      trigger.addEventListener('click', function (e) {
        e.preventDefault();
        var selector = trigger.getAttribute('data-sheet-trigger');
        var container = document.querySelector(selector);
        setOpen(container, true);
        var firstInput = container ? container.querySelector('input:not([type="hidden"]), textarea') : null;
        if (firstInput) firstInput.focus();
      });
    });

    document.querySelectorAll('[data-sheet-close]').forEach(function (btn) {
      btn.addEventListener('click', function (e) {
        var isLink = btn.tagName === 'A';
        if (isLink) return;
        e.preventDefault();
        setOpen(getContainer(btn), false);
      });
    });

    document.querySelectorAll('[data-sheet-overlay]').forEach(function (overlay) {
      overlay.addEventListener('click', function () {
        setOpen(getContainer(overlay), false);
      });
    });

    document.addEventListener('keydown', function (e) {
      if (e.key === 'Escape') {
        var openContainer = document.querySelector('.sheet-container--open');
        setOpen(openContainer, false);
      }
    });
  });
})();

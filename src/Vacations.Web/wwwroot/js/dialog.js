/* dialog.js — Diálogos modales genéricos */
(function () {
  function getContainer(el) {
    return el.closest('[data-dialog-container]') || document.querySelector(el.getAttribute('data-dialog-target'));
  }

  function setOpen(container, open) {
    if (!container) return;
    container.classList.toggle('dialog-overlay--open', open);
    container.setAttribute('aria-hidden', open ? 'false' : 'true');
    document.body.style.overflow = open ? 'hidden' : '';
    if (open) {
      var focusable = container.querySelector('input:not([type="hidden"]), textarea, button, select, a[href]');
      if (focusable) focusable.focus();
    } else {
      var trigger = container.__lastTrigger;
      if (trigger) trigger.focus();
    }
  }

  function isOpen(container) {
    return container && container.classList.contains('dialog-overlay--open');
  }

  document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-dialog-open]').forEach(function (trigger) {
      trigger.addEventListener('click', function (e) {
        e.preventDefault();
        var container = getContainer(trigger);
        if (container) container.__lastTrigger = trigger;
        setOpen(container, true);
      });
    });

    document.querySelectorAll('[data-dialog-close]').forEach(function (btn) {
      btn.addEventListener('click', function (e) {
        e.preventDefault();
        setOpen(getContainer(btn), false);
      });
    });

    document.querySelectorAll('[data-dialog-overlay]').forEach(function (overlay) {
      overlay.addEventListener('click', function () {
        setOpen(overlay, false);
      });
    });

    document.addEventListener('keydown', function (e) {
      if (e.key === 'Escape') {
        document.querySelectorAll('[data-dialog-container].dialog-overlay--open').forEach(function (container) {
          setOpen(container, false);
        });
      }
    });
  });
})();

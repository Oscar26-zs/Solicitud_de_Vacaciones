/* toast.js — Notificaciones toast */
(function () {
  window.showToast = function (message, type, title) {
    var container = document.querySelector('.toast-container');
    if (!container) {
      container = document.createElement('div');
      container.className = 'toast-container';
      document.body.appendChild(container);
    }

    type = type || 'info';
    title = title || (type === 'success' ? 'Operación exitosa' : type === 'error' ? 'Error' : type === 'warning' ? 'Aviso' : 'Información');

    var toast = document.createElement('div');
    toast.className = 'toast toast--' + type;
    toast.setAttribute('role', 'alert');

    var icons = {
      success: '<path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><path d="m9 11 3 3L22 4"/>',
      error: '<circle cx="12" cy="12" r="10"/><path d="m15 9-6 6"/><path d="m9 9 6 6"/>',
      warning: '<path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3"/><path d="M12 9v4"/><path d="M12 17h.01"/>',
      info: '<circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/>'
    };

    toast.innerHTML =
      '<svg class="toast-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">' +
      (icons[type] || icons.info) +
      '</svg>' +
      '<div class="toast-content">' +
      '<p class="toast-title">' + title + '</p>' +
      '<p class="toast-message">' + message + '</p>' +
      '</div>' +
      '<button type="button" class="toast-close" aria-label="Cerrar">' +
      '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M18 6 6 18"/><path d="m6 6 12 12"/></svg>' +
      '</button>';

    container.appendChild(toast);

    var closeBtn = toast.querySelector('.toast-close');
    closeBtn.addEventListener('click', closeToast);

    var timer = setTimeout(closeToast, 4000);

    function closeToast() {
      clearTimeout(timer);
      toast.classList.add('toast--closing');
      toast.addEventListener('animationend', function () {
        toast.remove();
      });
    }
  };

  // Auto-mostrar toasts renderizados en servidor
  document.addEventListener('DOMContentLoaded', function () {
    var serverToast = document.getElementById('server-toast');
    if (serverToast) {
      var message = serverToast.getAttribute('data-message');
      var type = serverToast.getAttribute('data-type') || 'info';
      var title = serverToast.getAttribute('data-title') || undefined;
      if (message) {
        showToast(message, type, title);
      }
    }
  });
})();
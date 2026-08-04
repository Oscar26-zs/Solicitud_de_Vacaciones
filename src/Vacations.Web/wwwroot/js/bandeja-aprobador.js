/* bandeja-aprobador.js — Detalle de aprobación en modal y acciones aprobar/rechazar */
(function () {
  var dialogContainer;
  var content;
  var currentId = null;

  function getToken() {
    var el = dialogContainer && dialogContainer.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : null;
  }

  function openDialog() {
    if (!dialogContainer) return;
    dialogContainer.classList.add('dialog-overlay--open');
    dialogContainer.setAttribute('aria-hidden', 'false');
    document.body.style.overflow = 'hidden';
  }

  function closeDialog() {
    if (!dialogContainer) return;
    dialogContainer.classList.remove('dialog-overlay--open');
    dialogContainer.setAttribute('aria-hidden', 'true');
    document.body.style.overflow = '';
    content.innerHTML = '';
    currentId = null;
  }

  function setupRejectMode() {
    if (!content) return;
    var formRechazo = content.querySelector('#form-rechazo');
    var btnRechazar = content.querySelector('[data-abrir-rechazo]');
    var btnVolver = content.querySelector('[data-rechazo-cancelar]');
    var btnEnviar = content.querySelector('[data-rechazo-enviar]');
    var btnAprobar = content.querySelector('[data-aprobar]');
    if (!formRechazo) return;

    btnRechazar.addEventListener('click', function () {
      formRechazo.hidden = false;
      btnRechazar.hidden = true;
      btnVolver.hidden = false;
      btnEnviar.hidden = false;
      if (btnAprobar) btnAprobar.hidden = true;
      var textarea = formRechazo.querySelector('textarea');
      if (textarea) textarea.focus();
    });

    btnVolver.addEventListener('click', function () {
      formRechazo.hidden = true;
      btnRechazar.hidden = false;
      btnVolver.hidden = true;
      btnEnviar.hidden = true;
      if (btnAprobar) btnAprobar.hidden = false;
    });

    btnEnviar.addEventListener('click', function () {
      var textarea = formRechazo.querySelector('textarea');
      var comentario = (textarea && textarea.value.trim()) || '';
      if (!comentario) {
        showToast('El comentario es obligatorio al rechazar.', 'error');
        if (textarea) textarea.focus();
        return;
      }
      postAction('/BandejaAprobador/Rechazar', {
        SolicitudId: currentId,
        Comentario: comentario
      });
    });
  }

  function setupApprove() {
    if (!content) return;
    var btnAprobar = content.querySelector('[data-aprobar]');
    if (!btnAprobar) return;
    btnAprobar.addEventListener('click', function () {
      postAction('/BandejaAprobador/Aprobar', { id: currentId });
    });
  }

  function postAction(url, data) {
    var formData = new FormData();
    Object.keys(data).forEach(function (key) {
      formData.append(key, data[key]);
    });
    var token = getToken();
    if (token) formData.append('__RequestVerificationToken', token);

    fetch(url, {
      method: 'POST',
      body: formData,
      headers: { 'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json' }
    })
      .then(function (res) {
        if (res.status === 404) {
          showToast('La solicitud no fue encontrada.', 'error');
          closeDialog();
          return null;
        }
        return res.json();
      })
      .then(function (data) {
        if (!data) return;
        if (data.ok) {
          closeDialog();
          showToast(data.message, 'success');
          setTimeout(function () { location.reload(); }, 400);
        } else {
          showToast(data.message, 'error');
        }
      })
      .catch(function () {
        showToast('Ocurrió un error. Intente de nuevo.', 'error');
      });
  }

  window.openDetalleAprobacion = function (id) {
    fetch('/BandejaAprobador/DetalleModal?id=' + encodeURIComponent(id), {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
      .then(function (res) {
        if (res.status === 404) {
          showToast('La solicitud no fue encontrada.', 'error');
          return null;
        }
        return res.text();
      })
      .then(function (html) {
        if (html === null) return;
        currentId = id;
        content.innerHTML = html;
        openDialog();
        setupRejectMode();
        setupApprove();
      })
      .catch(function () {
        showToast('No se pudo cargar el detalle.', 'error');
      });
  };

  document.addEventListener('DOMContentLoaded', function () {
    dialogContainer = document.getElementById('detalle-aprobacion');
    content = document.getElementById('detalle-aprobacion-content');

    document.addEventListener('click', function (e) {
      var btn = e.target.closest('[data-abrir-detalle]');
      if (btn) {
        window.openDetalleAprobacion(btn.getAttribute('data-abrir-detalle'));
        return;
      }
      var closeBtn = e.target.closest('[data-dialog-close]');
      if (closeBtn) {
        closeDialog();
      }
    });

    if (dialogContainer) {
      dialogContainer.addEventListener('click', function (e) {
        if (e.target === dialogContainer) closeDialog();
      });

      document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeDialog();
      });
    }
  });
})();

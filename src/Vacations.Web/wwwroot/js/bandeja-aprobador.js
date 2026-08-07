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

    var btnRechazar = content.querySelector('[data-abrir-rechazo]');
    var rechazoSection = content.querySelector('[data-rechazo-section]');

    if (!btnRechazar || !rechazoSection) return;

    var textarea = rechazoSection.querySelector('textarea[name="Comentario"]');
    var charCount = content.querySelector('#char-count');
    var modoRechazo = false;

    function updateState() {
      var hasText = textarea && textarea.value.trim().length > 0;
      btnRechazar.disabled = !hasText;
      if (charCount) charCount.textContent = textarea ? textarea.value.length : 0;
    }

    function armarMensajeRechazo(motivo) {
      var empleado = btnRechazar.getAttribute('data-empleado') || 'el empleado';
      var folio = btnRechazar.getAttribute('data-folio') || '';
      var fechaInicio = btnRechazar.getAttribute('data-fecha-inicio') || '';
      var fechaFin = btnRechazar.getAttribute('data-fecha-fin') || '';

      var detalles = [];
      if (folio) detalles.push(folio);
      if (fechaInicio && fechaFin) detalles.push(fechaInicio + ' – ' + fechaFin);

      var detalleTexto = detalles.length > 0 ? ' (' + detalles.join(' · ') + ')' : '';
      return 'Se rechazará la solicitud de ' + empleado + detalleTexto + '. Motivo: "' + motivo + '". Esta acción no se puede deshacer.';
    }

    btnRechazar.addEventListener('click', function () {
      if (!modoRechazo) {
        rechazoSection.hidden = false;
        modoRechazo = true;
        btnRechazar.textContent = 'Confirmar rechazo';
        btnRechazar.classList.remove('btn--outline');
        btnRechazar.classList.add('btn--destructive');
        btnRechazar.disabled = true;
        if (textarea) {
          textarea.focus();
          updateState();
        }
        return;
      }

      var comentario = (textarea && textarea.value.trim()) || '';
      if (!comentario) {
        showToast('El comentario es obligatorio al rechazar.', 'error');
        if (textarea) textarea.focus();
        return;
      }

      if (typeof window.showConfirmDialog !== 'function') {
        showToast('No se pudo abrir el diálogo de confirmación.', 'error');
        return;
      }

      window.showConfirmDialog({
        title: 'Confirmar rechazo',
        message: armarMensajeRechazo(comentario),
        confirmText: 'Sí, rechazar',
        cancelText: 'Volver',
        destructive: true
      }).then(function (confirmed) {
        if (confirmed) {
          postAction('/BandejaAprobador/Index?handler=Reject', {
            SolicitudId: currentId,
            Comentario: comentario
          });
        }
      });
    });

    if (textarea) {
      textarea.addEventListener('input', updateState);
    }
  }

  function setupApprove() {
    if (!content) return;
    var btnAprobar = content.querySelector('[data-aprobar]');
    if (!btnAprobar) return;
    btnAprobar.addEventListener('click', function () {
      postAction('/BandejaAprobador/Index?handler=Approve', { id: currentId });
    });
  }

  function armarMensajeCancelacion(btnCancelar, motivo) {
    var empleado = btnCancelar.getAttribute('data-empleado') || 'el empleado';
    var folio = btnCancelar.getAttribute('data-folio') || '';
    var fechaInicio = btnCancelar.getAttribute('data-fecha-inicio') || '';
    var fechaFin = btnCancelar.getAttribute('data-fecha-fin') || '';

    var detalles = [];
    if (folio) detalles.push(folio);
    if (fechaInicio && fechaFin) detalles.push(fechaInicio + ' – ' + fechaFin);

    var detalleTexto = detalles.length > 0 ? ' (' + detalles.join(' · ') + ')' : '';
    return 'Se cancelará la solicitud aprobada de ' + empleado + detalleTexto + '. Motivo: "' + motivo + '". Los días serán devueltos al saldo disponible.';
  }

  function setupCancelMode() {
    if (!content) return;

    var footer = content.querySelector('[data-cancel-footer]');
    var cancelSection = content.querySelector('[data-cancel-section]');
    if (!footer || !cancelSection) return;

    var btnCancelar = footer.querySelector('[data-cancelar-aprobada]');
    var btnVolver = footer.querySelector('[data-dialog-close]');
    var textarea = cancelSection.querySelector('textarea[name="Motivo"]');
    var charCount = content.querySelector('#cancel-char-count');
    var modoCancelacion = false;

    if (!btnCancelar || !btnVolver || !textarea) return;

    function updateState() {
      var hasText = textarea.value.trim().length > 0;
      btnCancelar.disabled = !hasText;
      if (charCount) charCount.textContent = textarea.value.length;
    }

    function salirModoCancelacion() {
      modoCancelacion = false;
      cancelSection.hidden = true;
      textarea.value = '';
      btnCancelar.textContent = 'Cancelar solicitud';
      btnCancelar.disabled = false;
      btnVolver.textContent = 'Cerrar';
      btnVolver.setAttribute('data-dialog-close', '');
      btnVolver.removeAttribute('data-cancel-volver');
      updateState();
    }

    btnCancelar.addEventListener('click', function () {
      if (!modoCancelacion) {
        modoCancelacion = true;
        cancelSection.hidden = false;
        btnCancelar.textContent = 'Confirmar cancelación';
        btnCancelar.disabled = true;
        btnVolver.textContent = 'Volver';
        btnVolver.removeAttribute('data-dialog-close');
        btnVolver.setAttribute('data-cancel-volver', '');
        textarea.focus();
        updateState();
        return;
      }

      var motivo = textarea.value.trim();
      if (!motivo) {
        showToast('El motivo de la cancelación es obligatorio.', 'error');
        textarea.focus();
        return;
      }

      if (typeof window.showConfirmDialog !== 'function') {
        showToast('No se pudo abrir el diálogo de confirmación.', 'error');
        return;
      }

      var id = btnCancelar.getAttribute('data-cancelar-aprobada') || currentId;
      window.showConfirmDialog({
        title: 'Cancelar solicitud',
        message: armarMensajeCancelacion(btnCancelar, motivo),
        confirmText: 'Sí, cancelar',
        cancelText: 'Volver',
        destructive: true
      }).then(function (confirmed) {
        if (confirmed) {
          postAction('/BandejaAprobador/CancelarAprobada', { id: id, motivo: motivo });
        }
      });
    });

    btnVolver.addEventListener('click', function (e) {
      if (btnVolver.hasAttribute('data-cancel-volver')) {
        e.preventDefault();
        e.stopPropagation();
        salirModoCancelacion();
      }
    });

    textarea.addEventListener('input', updateState);
  }

  function bindCancelButtons(scope) {
    if (!scope) return;
    scope.querySelectorAll('[data-abrir-cancelacion]').forEach(function (btn) {
      if (btn.dataset.cancelBound === 'true') return;
      btn.dataset.cancelBound = 'true';
      btn.addEventListener('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        openDetalleAprobacion(btn.getAttribute('data-abrir-cancelacion'), true);
      });
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
        if (res.status === 429) {
          showToast('Demasiadas solicitudes. Espera unos segundos antes de reintentar.', 'error');
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

  window.openDetalleAprobacion = function (id, activarCancelacion) {
    fetch('/BandejaAprobador/DetalleModal?id=' + encodeURIComponent(id), {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
      .then(function (res) {
        if (res.status === 404) {
          showToast('La solicitud no fue encontrada.', 'error');
          return null;
        }
        if (res.status === 429) {
          showToast('Demasiadas solicitudes. Espera unos segundos antes de reintentar.', 'error');
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
        setupCancelMode();
        if (activarCancelacion) {
          var footer = content.querySelector('[data-cancel-footer]');
          if (footer) {
            var btn = footer.querySelector('[data-cancelar-aprobada]');
            if (btn) btn.click();
          }
        }
      })
      .catch(function () {
        showToast('No se pudo cargar el detalle.', 'error');
      });
  };

  document.addEventListener('DOMContentLoaded', function () {
    dialogContainer = document.getElementById('detalle-aprobacion');
    content = document.getElementById('detalle-aprobacion-content');

    bindCancelButtons(document);

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

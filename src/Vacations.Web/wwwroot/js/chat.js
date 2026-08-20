/* chat.js — Widget flotante del asistente de vacaciones */
(function () {
  var widget, messages, form, input, submitBtn;

  function isOpen() {
    return widget && widget.classList.contains('chat-widget--open');
  }

  function setOpen(open) {
    if (!widget) return;
    widget.classList.toggle('chat-widget--open', open);
    var fab = widget.querySelector('[data-chat-toggle].chat-widget-fab');
    var panel = widget.querySelector('[data-chat-panel]');
    if (fab) fab.setAttribute('aria-expanded', open ? 'true' : 'false');
    if (panel) panel.setAttribute('aria-hidden', open ? 'false' : 'true');
    if (open && input) {
      setTimeout(function () { input.focus(); }, 150);
      scrollToBottom();
    }
  }

  function scrollToBottom() {
    if (messages) messages.scrollTop = messages.scrollHeight;
  }

  function appendMessage(text, variant) {
    var el = document.createElement('div');
    el.className = 'chat-widget-message chat-widget-message--' + variant;
    el.textContent = text;
    messages.appendChild(el);
    scrollToBottom();
    return el;
  }

  function showTyping() {
    var el = document.createElement('div');
    el.className = 'chat-widget-typing';
    el.setAttribute('data-chat-typing', '');
    el.innerHTML =
      '<span class="chat-widget-typing-dot"></span>' +
      '<span class="chat-widget-typing-dot"></span>' +
      '<span class="chat-widget-typing-dot"></span>';
    messages.appendChild(el);
    scrollToBottom();
    return el;
  }

  function autoResize() {
    input.style.height = 'auto';
    input.style.height = Math.min(input.scrollHeight, 96) + 'px';
  }

  function getToken() {
    var el = form.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : null;
  }

  function enviarMensaje(mensaje) {
    var typingEl = showTyping();
    submitBtn.disabled = true;
    input.disabled = true;

    var formData = new FormData();
    formData.append('mensaje', mensaje);
    var token = getToken();
    if (token) formData.append('__RequestVerificationToken', token);

    fetch('/Chat/Enviar', {
      method: 'POST',
      body: formData,
      headers: { 'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json' }
    })
      .then(function (res) { return res.json(); })
      .then(function (data) {
        typingEl.remove();
        if (data && data.ok) {
          appendMessage(data.respuesta, 'bot');
        } else {
          appendMessage((data && data.respuesta) || 'Ocurrió un error inesperado.', 'error');
        }
      })
      .catch(function () {
        typingEl.remove();
        appendMessage('No se pudo conectar con el asistente. Intenta de nuevo.', 'error');
      })
      .finally(function () {
        submitBtn.disabled = false;
        input.disabled = false;
        input.focus();
      });
  }

  document.addEventListener('DOMContentLoaded', function () {
    widget = document.querySelector('[data-chat-widget]');
    if (!widget) return;

    messages = widget.querySelector('[data-chat-messages]');
    form = widget.querySelector('[data-chat-form]');
    input = widget.querySelector('[data-chat-input]');
    submitBtn = widget.querySelector('[data-chat-submit]');

    widget.querySelectorAll('[data-chat-toggle]').forEach(function (btn) {
      btn.addEventListener('click', function () {
        setOpen(!isOpen());
      });
    });

    input.addEventListener('input', autoResize);

    input.addEventListener('keydown', function (e) {
      if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        form.requestSubmit();
      }
    });

    form.addEventListener('submit', function (e) {
      e.preventDefault();
      var mensaje = input.value.trim();
      if (!mensaje) return;

      appendMessage(mensaje, 'user');
      input.value = '';
      autoResize();
      enviarMensaje(mensaje);
    });

    document.addEventListener('keydown', function (e) {
      if (e.key === 'Escape' && isOpen()) {
        setOpen(false);
      }
    });
  });
})();

/* validation.js — Validación en cliente (vanilla) para Data Annotations */
(function () {
  document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('form[data-client-validate="true"]').forEach(function (form) {
      var submitBtn = form.querySelector('[type="submit"]');

      form.querySelectorAll('input, select, textarea').forEach(function (field) {
        field.addEventListener('input', validateField);
        field.addEventListener('change', validateField);
      });

      form.addEventListener('submit', function (e) {
        var fields = form.querySelectorAll('input, select, textarea');
        var valid = true;
        fields.forEach(function (f) {
          var fieldValid = validateField(f);
          if (!fieldValid) valid = false;
        });
        if (!valid) {
          e.preventDefault();
          var firstError = form.querySelector('[aria-invalid="true"]');
          if (firstError) firstError.focus();
        }
      });

      function validateField(field) {
        if (field.type === 'hidden' || field.type === 'submit' || field.type === 'button') {
          return true;
        }

        var value = field.value.trim();
        var valid = true;
        var message = '';

        if (field.hasAttribute('required') && value === '') {
          valid = false;
          message = field.getAttribute('data-val-required') || 'Este campo es requerido.';
        } else if (field.hasAttribute('data-val-maxlength') && value.length > parseInt(field.getAttribute('data-val-maxlength-max') || '0', 10)) {
          valid = false;
          message = field.getAttribute('data-val-maxlength') || 'El campo excede la longitud máxima.';
        } else if (field.hasAttribute('data-val-minlength') && value !== '' && value.length < parseInt(field.getAttribute('data-val-minlength-min') || '0', 10)) {
          valid = false;
          message = field.getAttribute('data-val-minlength') || 'El campo es demasiado corto.';
        } else if (field.type === 'email' && value !== '' && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
          valid = false;
          message = field.getAttribute('data-val-email') || 'El formato del correo electrónico no es válido.';
        }

        field.setAttribute('aria-invalid', valid ? 'false' : 'true');

        var span = form.querySelector('[data-valmsg-for="' + field.name + '"]');
        if (span) {
          span.textContent = valid ? '' : message;
          span.style.color = 'var(--destructive)';
          span.style.fontSize = '12px';
        }

        return valid;
      }
    });
  });
})();
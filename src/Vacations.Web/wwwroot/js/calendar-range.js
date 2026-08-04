/* calendar-range.js — Selección de rango de fechas para solicitar vacaciones */
(function () {
  var WEEKDAYS = ['Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sá', 'Do'];
  var MONTHS = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];

  function parseDate(str) {
    if (!str) return null;
    var parts = str.split('-');
    return new Date(+parts[0], +parts[1] - 1, +parts[2]);
  }

  function fmt(date) {
    var m = String(date.getMonth() + 1).padStart(2, '0');
    var d = String(date.getDate()).padStart(2, '0');
    return date.getFullYear() + '-' + m + '-' + d;
  }

  function isWeekend(d) {
    var day = d.getDay();
    return day === 0 || day === 6;
  }

  function businessDaysBetween(start, end) {
    var count = 0;
    for (var d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
      if (!isWeekend(d)) count++;
    }
    return count;
  }

  function init(container) {
    var balance = parseInt(container.dataset.availableBalance || '0', 10);
    var form = container.closest('form');
    var root = form || container;
    var inicioInput = root.querySelector('[data-fecha-inicio]');
    var finInput = root.querySelector('[data-fecha-fin]');
    var countEl = root.querySelector('[data-selected-count]');
    var warningEl = root.querySelector('[data-exceeding-warning]');
    var noRangeEl = root.querySelector('[data-no-range-warning]');
    var daysEl = container.querySelector('[data-calendar-days]');
    var labelEl = container.querySelector('[data-calendar-month]');

    var today = new Date();
    today.setHours(0, 0, 0, 0);

    var rangeStart = parseDate(inicioInput.value);
    var rangeEnd = parseDate(finInput.value);
    if (rangeStart && rangeStart.getFullYear() < 2000) rangeStart = null;
    if (rangeEnd && rangeEnd.getFullYear() < 2000) rangeEnd = null;
    if (!rangeStart && rangeEnd) rangeStart = rangeEnd;
    if (!rangeStart) {
      inicioInput.value = '';
      finInput.value = '';
    }
    var viewYear = rangeStart ? rangeStart.getFullYear() : today.getFullYear();
    var viewMonth = rangeStart ? rangeStart.getMonth() : today.getMonth();

    function renderCalendar() {
      labelEl.textContent = MONTHS[viewMonth] + ' ' + viewYear;
      daysEl.innerHTML = '';

      var first = new Date(viewYear, viewMonth, 1);
      var startOffset = (first.getDay() + 6) % 7;
      var daysInMonth = new Date(viewYear, viewMonth + 1, 0).getDate();

      for (var i = 0; i < startOffset; i++) {
        var empty = document.createElement('span');
        empty.style.height = '32px';
        daysEl.appendChild(empty);
      }

      for (var d = 1; d <= daysInMonth; d++) {
        let date = new Date(viewYear, viewMonth, d);
        var btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'calendar-day';
        btn.dataset.date = fmt(date);
        btn.textContent = d;

        if (date < today) {
          btn.classList.add('calendar-day--disabled');
          btn.disabled = true;
        }
        if (isWeekend(date)) {
          btn.classList.add('calendar-day--weekend');
          btn.disabled = true;
        }
        if (fmt(date) === fmt(today)) btn.classList.add('calendar-day--today');

        if (rangeStart && rangeEnd && !isWeekend(date)) {
          if (fmt(date) === fmt(rangeStart)) btn.classList.add('calendar-day--range-start');
          else if (fmt(date) === fmt(rangeEnd)) btn.classList.add('calendar-day--range-end');
          else if (date > rangeStart && date < rangeEnd) btn.classList.add('calendar-day--in-range');
        }

        btn.addEventListener('click', function () { onDayClick(date, btn); });
        daysEl.appendChild(btn);
      }

      updateSummary();
    }

    function onDayClick(date) {
      if (isWeekend(date)) return;
      if (!rangeStart || (rangeStart && rangeEnd)) {
        rangeStart = date;
        rangeEnd = null;
      } else {
        if (date < rangeStart) {
          rangeEnd = rangeStart;
          rangeStart = date;
        } else {
          rangeEnd = date;
        }
      }
      inicioInput.value = fmt(rangeStart);
      finInput.value = rangeEnd ? fmt(rangeEnd) : fmt(rangeStart);
      renderCalendar();
    }

    function updateSummary() {
      var hasRange = !!(rangeStart && rangeEnd);
      var selected = hasRange ? businessDaysBetween(rangeStart, rangeEnd) : 0;
      if (countEl) countEl.textContent = selected;
      if (warningEl) warningEl.style.display = selected > balance ? 'block' : 'none';
      if (noRangeEl) noRangeEl.style.display = hasRange ? 'none' : 'block';

      daysEl.querySelectorAll('.calendar-day--exceeding').forEach(function (b) {
        b.classList.remove('calendar-day--exceeding');
      });

      if (hasRange && selected > balance) {
        var exceeded = selected - balance;
        var marked = 0;
        for (var d = new Date(rangeEnd); d >= rangeStart && marked < exceeded; d.setDate(d.getDate() - 1)) {
          if (isWeekend(d)) continue;
          var b = daysEl.querySelector('[data-date="' + fmt(d) + '"]');
          if (b) { b.classList.add('calendar-day--exceeding'); marked++; }
        }
      }
    }

    if (form) {
      form.addEventListener('submit', function (e) {
        if (!(rangeStart && rangeEnd)) {
          e.preventDefault();
          if (noRangeEl) noRangeEl.style.display = 'block';
          container.scrollIntoView({ block: 'center' });
        }
      });
    }

    container.querySelector('[data-calendar-prev]').addEventListener('click', function () {
      viewMonth--;
      if (viewMonth < 0) { viewMonth = 11; viewYear--; }
      renderCalendar();
    });
    container.querySelector('[data-calendar-next]').addEventListener('click', function () {
      viewMonth++;
      if (viewMonth > 11) { viewMonth = 0; viewYear++; }
      renderCalendar();
    });

    renderCalendar();
  }

  document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-calendar]').forEach(init);
  });
})();

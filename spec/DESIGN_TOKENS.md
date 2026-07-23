# Guía de Diseño — Gestión de Permisos y Vacaciones (ASP.NET Core MVC)

Este documento describe con precisión el diseño del sistema **Gestión de Permisos y Vacaciones** para ser implementado en **ASP.NET Core MVC clásico** (Razor Views `.cshtml`, sin Blazor). Cada token, componente y pantalla se traduce a valores concretos (px, oklch, radios, sombras) y se mapea a la arquitectura MVC: Partial Views, View Components, Controllers, ViewModels y JavaScript vanilla en el cliente.

> **Regla de oro n.º 1 — Es un diseño MONOCROMÁTICO.** El color "primario" NO es azul ni morado: es **gris muy oscuro / casi negro** en modo claro y **gris muy claro / casi blanco** en modo oscuro. Toda la interfaz es en escala de grises. El único color con matiz son: (a) los estados de solicitud (ámbar, esmeralda, rojo, gris), (b) los colores de avatar de cada persona, y (c) el rojo destructivo.

> **Regla de oro n.º 2 — Usa siempre tokens semánticos.** Nunca escribas `color: black` o `background: white` directo. Usa las variables CSS (`--background`, `--foreground`, `--card`, `--primary`, …) para que el modo claro/oscuro funcione automáticamente.

---

## 1. Fundamentos

### 1.1 Tipografía

Dos fuentes de la familia **Geist** (Google Fonts):

| Uso            | Fuente        | `font-family` CSS                                              | Dónde se usa                          |
| -------------- | ------------- | -------------------------------------------------------------- | ------------------------------------- |
| Cuerpo / UI    | Geist Sans    | `'Geist', ui-sans-serif, system-ui, sans-serif`                | Todo el texto por defecto             |
| Monoespaciada  | Geist Mono    | `'Geist Mono', ui-monospace, monospace`                        | Folios/IDs (`SOL-0001`), contraseña demo, títulos de diálogo de detalle |

- El `<body>` lleva `font-family: var(--font-sans)` y `-webkit-font-smoothing: antialiased`.
- Interlineado de cuerpo: relajado (~1.5). Los títulos usan `text-wrap: balance` / `text-wrap: pretty`.
- Números tabulares: las cifras (días, contadores, saldos) usan `font-variant-numeric: tabular-nums` para que no "bailen".

**Escala tipográfica real usada en el sistema:**

| Nombre        | Tamaño | Peso            | Clase CSS             | Uso                                                |
| ------------- | ------ | --------------- | --------------------- | -------------------------------------------------- |
| Título página | 24px   | 600 (semibold)  | `.text-2xl`           | H1 de Bandeja, RRHH                             |
| Título vista  | 20px   | 600             | `.text-xl`              | "Hola, {nombre}" (empleado)                     |
| Cifra grande  | 30px   | 600             | `.text-3xl`               | Progreso "días consumidos"                      |
| Cifra stat    | 24px   | 600             | `.text-2xl`               | Valor de StatCard                               |
| Título card   | 16px   | 500 (medium)    | `.text-base`              | `CardTitle`                                     |
| Cuerpo        | 14px   | 400             | `.text-sm`                | Texto general, tablas, inputs                   |
| Secundario    | 12px   | 400             | `.text-xs`                | Ayudas, metadatos, fechas, "hint" de StatCard   |
| Micro         | 10px   | 500             | `.text-\[10px\]`          | Badge de rol en el menú de usuario              |

### 1.2 Paleta de color (tokens)

Colores declarados en OKLCH. Se incluye el **equivalente hex aproximado** para HTML/CSS. Cada par surface/foreground se usa junto (texto `*-foreground` sobre el fondo del mismo nombre).

#### Modo claro (`:root`)

| Token                    | OKLCH                        | Hex aprox.   | Rol                                              |
| ------------------------ | ---------------------------- | ------------ | ------------------------------------------------ |
| `--background`           | `oklch(1 0 0)`               | `#ffffff`    | Fondo de la app / página                         |
| `--foreground`           | `oklch(0.145 0 0)`           | `#252525`    | Texto principal (casi negro)                     |
| `--card`                 | `oklch(1 0 0)`               | `#ffffff`    | Fondo de tarjetas                                |
| `--card-foreground`      | `oklch(0.145 0 0)`           | `#252525`    | Texto sobre tarjetas                             |
| `--popover`              | `oklch(1 0 0)`               | `#ffffff`    | Popovers, menús, calendario                      |
| `--popover-foreground`   | `oklch(0.145 0 0)`           | `#252525`    | Texto sobre popover                              |
| `--primary`              | `oklch(0.205 0 0)`           | `#343434`    | Acción primaria (gris muy oscuro)                |
| `--primary-foreground`   | `oklch(0.985 0 0)`           | `#fbfbfb`    | Texto sobre primary (casi blanco)                |
| `--secondary`            | `oklch(0.97 0 0)`            | `#f7f7f7`    | Botón/chip secundario, fondo tenue               |
| `--secondary-foreground` | `oklch(0.205 0 0)`           | `#343434`    | Texto sobre secondary                            |
| `--muted`                | `oklch(0.97 0 0)`            | `#f7f7f7`    | Fondos sutiles, franjas de tabla, avatar fallback|
| `--muted-foreground`     | `oklch(0.556 0 0)`           | `#8e8e8e`    | Texto secundario / de ayuda                      |
| `--accent`               | `oklch(0.97 0 0)`            | `#f7f7f7`    | Hover sutil                                      |
| `--accent-foreground`    | `oklch(0.205 0 0)`           | `#343434`    | Texto sobre accent                               |
| `--destructive`          | `oklch(0.577 0.245 27.325)`  | `#e5484d`    | Rojo (rechazar, cancelar, error)                 |
| `--border`               | `oklch(0.922 0 0)`           | `#ebebeb`    | Bordes / divisores                               |
| `--input`                | `oklch(0.922 0 0)`           | `#ebebeb`    | Borde de campos de formulario                    |
| `--ring`                 | `oklch(0.708 0 0)`           | `#b5b5b5`    | Anillo de foco                                   |

#### Modo oscuro (`.dark`)

| Token                    | OKLCH                         | Hex/aprox.              | Rol                                   |
| ------------------------ | ----------------------------- | ----------------------- | ------------------------------------- |
| `--background`           | `oklch(0.145 0 0)`            | `#252525`               | Fondo de la app                       |
| `--foreground`           | `oklch(0.985 0 0)`            | `#fbfbfb`               | Texto principal (casi blanco)         |
| `--card`                 | `oklch(0.205 0 0)`            | `#343434`               | Fondo de tarjetas                     |
| `--card-foreground`      | `oklch(0.985 0 0)`            | `#fbfbfb`               | Texto sobre tarjetas                  |
| `--popover`              | `oklch(0.205 0 0)`            | `#343434`               | Popovers/menús                        |
| `--popover-foreground`   | `oklch(0.985 0 0)`            | `#fbfbfb`               | Texto sobre popover                   |
| `--primary`              | `oklch(0.922 0 0)`            | `#ebebeb`               | Acción primaria (¡claro en oscuro!)   |
| `--primary-foreground`   | `oklch(0.205 0 0)`            | `#343434`               | Texto sobre primary                   |
| `--secondary`            | `oklch(0.269 0 0)`            | `#434343`               | Secundario                            |
| `--secondary-foreground` | `oklch(0.985 0 0)`            | `#fbfbfb`               | Texto sobre secondary                 |
| `--muted`                | `oklch(0.269 0 0)`            | `#434343`               | Fondos sutiles                        |
| `--muted-foreground`     | `oklch(0.708 0 0)`           | `#b5b5b5`               | Texto secundario                      |
| `--accent`               | `oklch(0.269 0 0)`            | `#434343`               | Hover sutil                           |
| `--accent-foreground`    | `oklch(0.985 0 0)`            | `#fbfbfb`               | Texto sobre accent                    |
| `--destructive`          | `oklch(0.704 0.191 22.216)`  | `#ff6b6b`               | Rojo (más claro en oscuro)            |
| `--border`               | `oklch(1 0 0 / 10%)`         | `rgba(255,255,255,.10)` | Bordes translúcidos                   |
| `--input`                | `oklch(1 0 0 / 15%)`         | `rgba(255,255,255,.15)` | Borde de campos                       |
| `--ring`                 | `oklch(0.556 0 0)`           | `#8e8e8e`               | Anillo de foco                        |

#### Tokens no utilizados en este prototipo

Las variables `--chart-1` a `--chart-5` y toda la familia `--sidebar-*` están declaradas en el CSS pero **no se utilizan** en ningún componente de este prototipo. Pueden omitirse.

#### Colores semánticos de estado (fijos, NO cambian por tema)

| Significado | Fondo                | Texto claro   | Texto oscuro       | Hex texto (claro/oscuro) |
| ----------- | -------------------- | ------------- | ------------------ | ------------------------ |
| Pendiente   | `amber-500 / 15%`    | `amber-700`   | `amber-400`        | `#b45309` / `#fbbf24`    |
| Aprobada    | `emerald-500 / 15%`  | `emerald-700` | `emerald-400`      | `#047857` / `#34d399`    |
| Rechazada   | `destructive / 15%`  | `destructive` | `destructive`      | `#e5484d`                |
| Cancelada   | `muted`              | `muted-foreground` | `muted-foreground` | `#8e8e8e`           |
| Info (timeline "creada") | `sky-500 / 15%` | `sky-600` | `sky-400`        | `#0284c7` / `#38bdf8`    |

En CSS: `background: rgba(r,g,b,0.15)` para cada color.

#### Colores de avatar (uno por persona)

| Persona           | Iniciales | Hex        |
| ----------------- | --------- | ---------- |
| Ana Torres        | AT        | `#f43f5e`  |
| Diego Fuentes     | DF        | `#0ea5e9`  |
| Sofía Herrera     | SH        | `#8b5cf6`  |
| Marta Ríos        | MR        | `#14b8a6`  |
| Pedro Salas       | PS        | `#f97316`  |
| Carlos Ramírez    | CR        | `#6366f1`  |
| Laura Méndez      | LM        | `#c026d3`  |

### 1.3 Radios de esquina

Base: `--radius: 0.625rem` (**10px**). Escala derivada:

| Token         | Cálculo                | Valor  | Se usa en                                  |
| ------------- | ---------------------- | ------ | ------------------------------------------ |
| `rounded-sm`  | `radius * 0.6`         | 6px    | Elementos pequeños                         |
| `rounded-md`  | `radius * 0.8`         | 8px    | Botones sm/icon-sm, algunos controles      |
| `rounded-lg`  | `radius`               | 10px   | Botones, inputs, contenedores con borde    |
| `rounded-xl`  | `radius * 1.4`         | 14px   | **Tarjetas (Card)**, footer de diálogos    |
| `rounded-2xl` | `radius * 1.8`         | 18px   | (reservado)                                |
| `rounded-4xl` | `radius * 2.6`         | 26px   | **Badges** (píldora muy redondeada)        |
| `rounded-full`| —                      | 9999px | Avatares, badges de estado (píldora)       |

### 1.4 Espaciado

- Escala de 4px: `gap-1`=4px, `gap-2`=8px, `gap-3`=12px, `gap-4`=16px, `gap-6`=24px.
- Separación entre hijos con **`gap`** en flex/grid.
- **Espaciado interno de tarjetas:** `--card-spacing: 16px`.

### 1.5 Sombras y elevación

Las tarjetas **NO usan `box-shadow`**, usan un **anillo sutil** de 1px: `box-shadow: 0 0 0 1px rgba(37,37,37,0.10)` (claro) / `rgba(251,251,251,0.10)` (oscuro).

Las sombras proyectadas solo aparecen en capas flotantes:

| Nivel  | CSS                                        | Componentes                       |
| ------ | ------------------------------------------ | --------------------------------- |
| `sm`   | `0 1px 2px rgba(0,0,0,.05)`                | (poco usado)                      |
| `md`   | `0 4px 6px rgba(0,0,0,.1)`                 | Popover, dropdown, calendario     |
| `lg`   | `0 10px 15px rgba(0,0,0,.1)`               | Diálogos (modal), sheets          |

### 1.6 Foco (accesibilidad)

```css
:focus-visible {
  outline: none;
  border-color: var(--ring);
  box-shadow: 0 0 0 3px color-mix(in oklch, var(--ring) 50%, transparent);
}
```

Estado inválido (`aria-invalid="true"`): borde `--destructive` + anillo `rgba(destructive, .2)`.

### 1.7 Transiciones

| Duración | Easing        | Uso                                          |
| -------- | ------------- | -------------------------------------------- |
| 150ms    | `ease-in-out` | Colores de botón, hover, foco de campos      |
| 200ms    | `ease-out`    | Entrada de modales/sheets                    |
| 150ms    | `ease-out`    | Salida de overlays                           |

Botón: al pulsar (`:active`) desciende 1px (`translateY(1px)`) salvo si abre un menú.

### 1.8 Z-index

| Capa                 | z-index |
| -------------------- | ------- |
| Header (sticky)      | 40      |
| Popover / dropdown   | 40      |
| Tooltip              | 45      |
| Overlay de sheet/dialog | 50   |
| Contenido de sheet/dialog | 51 |
| Toast                | 60      |

### 1.9 Breakpoints (mobile-first)

| Prefijo | Ancho    | Efecto principal                                            |
| ------- | -------- | ----------------------------------------------------------- |
| base    | <640px   | 1 columna; sheet sube desde abajo; menús compactos          |
| `sm`    | ≥640px   | Grids 2 cols (Jefe/RRHH) / 3 cols (Empleado); sheet desde la derecha |
| `md`    | ≥768px   | Filtros y grids intermedios                                 |
| `lg`    | ≥1024px  | Grids 4 cols (Jefe/RRHH); layout completo                   |

Contenido centrado en `max-width: 72rem (1152px)` con padding lateral 16px.

### 1.10 Fechas — formato de visualización

Todas las fechas se muestran en formato español corto: `"10 mar 2025"` (locale `es-ES`, `day: "2-digit"`, `month: "short"`, `year: "numeric"`). Las fechas con hora incluyen hora y minutos: `"10 mar 2025, 14:30"`.

### 1.11 Detalles de interacción menores

- **Scrollbar en sheets y diálogos:** ancho 10px, thumb `rounded-full` con color `--border`, track con padding `p-px`.
- **Tablas responsivas:** contenedor `overflow-x: auto` sin indicador visual adicional.

---

## 2. Arquitectura MVC del proyecto

```
/Controllers/
  AccountController.cs        # Login / logout / cambio de rol
  EmployeeController.cs       # Dashboard, crear/editar/cancelar solicitudes
  ManagerController.cs        # Bandeja de aprobaciones, aprobar/rechazar
  HRController.cs             # Historial, filtros, exportar CSV

/Models/
  /ViewModels/
    LoginViewModel.cs          # Usuario/correo + contraseña
    EmployeeDashboardViewModel.cs  # Saldo, consumido, disponible, solicitudes
    CreateRequestViewModel.cs  # Fecha inicio, fecha fin, motivo
    RequestListViewModel.cs    # Lista paginada + filtros
    ManagerInboxViewModel.cs   # Solicitudes + conteos por estado
    RequestDetailViewModel.cs  # Solicitud + empleado + traslapes + saldo
    ManagerDecisionViewModel.cs # Aprobar/rechazar + comentario
    HRHistorialViewModel.cs    # Solicitudes filtradas + conteos
    ChangeRoleViewModel.cs     # Rol activo seleccionado
  /Entities/
    Employee.cs                # Id, Name, Email, AvatarColor, Initials, Roles, AnnualBalance
    LeaveRequest.cs            # Id, EmployeeId, StartDate, EndDate, Days, Reason, Status, History, etc.
    RequestEvent.cs            # Id, Type, Timestamp, Actor, Note
    UserSession.cs             # CurrentUserId, ActiveRole, AvailableRoles

/Views/
  /Account/
    Login.cshtml               # Pantalla de inicio de sesión
    AccessDenied.cshtml
  /Employee/
    Index.cshtml               # Dashboard del empleado (usa ViewComponent)
  /Manager/
    Index.cshtml               # Bandeja de aprobaciones (usa ViewComponent)
  /HR/
    Index.cshtml               # Panel de RRHH (usa ViewComponent)
  /Shared/
    _Layout.cshtml             # Layout global con AppShell (header sticky + main)
    _AppShell.cshtml           # Header con logo, ThemeToggle, UserMenu
    _StatusBadge.cshtml        # Badge de estado (puramente visual)
    _StatCard.cshtml           # Tarjeta de estadística (puramente visual)
    _RequestTimeline.cshtml    # Timeline de eventos (puramente visual)
    _RequestDetailDialog.cshtml# Diálogo de detalle (usa JS para abrir/cerrar)
    _UserAvatar.cshtml         # Círculo con iniciales
    _UserMenu.cshtml           # Dropdown de usuario
    _ThemeToggle.cshtml        # Botón de tema claro/oscuro
    _TablePagination.cshtml    # Paginación de tabla

/ViewComponents/
  EmployeeDashboardViewComponent.cs     # Calcula saldos y renderiza dashboard
  EmployeeRequestsViewComponent.cs      # Lista de solicitudes del empleado
  NewRequestSheetViewComponent.cs       # Sheet de nueva solicitud (valida saldo en servidor)
  ManagerInboxViewComponent.cs          # Bandeja con filtros y búsqueda
  ManagerRequestDetailViewComponent.cs  # Detalle con aprobar/rechazar
  HRHistorialViewComponent.cs           # Historial con filtros y exportación

/wwwroot/
  /css/
    tokens.css                 # Variables CSS globales + estilos base
    components.css             # Estilos de componentes (Button, Card, Input, Table, etc.)
    utilities.css              # Clases utilitarias (gap-*, text-*, etc.)
  /js/
    sheet.js                   # Abrir/cerrar sheet, slide animation
    calendar-range.js          # Selección de rango, días excedentes en rojo
    toast.js                   # Notificaciones toast
    dialog.js                  # Modal open/close con fade+scale
    toggle-group.js            # Filtros tipo toggle
    tooltip.js                 # Tooltip en hover/foco
    theme.js                   # Toggle claro/oscuro, localStorage, prefers-color-scheme
    validation.js              # Validación en cliente, deshabilitar botón envío
    export-csv.js              # Descarga CSV desde el cliente
    user-menu.js               # Menú desplegable de usuario
    pagination.js              # Cambio de página sin recargar
```

---

## 3. Componentes base (patrones CSS)

Todos los componentes base se implementan como marcado HTML directo (sin Partial Views ni View Components) dentro de las vistas. Los estilos se definen en `wwwroot/css/components.css` con clases planas.

### 3.1 Botón (`.btn`)

```css
.btn {
  display: inline-flex; align-items: center; justify-content: center;
  gap: 6px; height: 32px; padding: 0 10px;
  border: 1px solid transparent; border-radius: 10px;
  font-size: 14px; font-weight: 500; white-space: nowrap;
  transition: all 150ms ease-in-out; cursor: pointer; user-select: none;
}
.btn--default { background: var(--primary); color: var(--primary-foreground); }
.btn--default:hover { background: color-mix(in oklch, var(--primary) 80%, transparent); }
.btn--outline { background: var(--background); color: var(--foreground); border-color: var(--border); }
.btn--outline:hover { background: var(--muted); }
.btn--ghost { background: transparent; color: var(--foreground); }
.btn--ghost:hover { background: var(--muted); }
.btn--destructive { background: rgba(229,72,77,.10); color: var(--destructive); }
.btn--destructive:hover { background: rgba(229,72,77,.20); }
.btn:disabled { opacity: .5; pointer-events: none; }
.btn:active:not([aria-haspopup]) { transform: translateY(1px); }
```

Tamaños: `default` (32px), `sm` (28px), `lg` (36px), `icon` (32×32), `icon-sm` (28×28).

### 3.2 Tarjeta (`.card`)

```css
.card {
  background: var(--card); border-radius: 14px;
  box-shadow: 0 0 0 1px rgba(37,37,37,.10);
  overflow: hidden; display: flex; flex-direction: column; gap: 16px; padding: 16px 0;
}
.card-header { padding: 0 16px; display: flex; flex-direction: column; gap: 4px; }
.card-title { font-size: 16px; font-weight: 500; line-height: 1.375; }
.card-description { font-size: 14px; color: var(--muted-foreground); }
.card-content { padding: 0 16px; }
.card-footer { border-top: 1px solid var(--border); background: color-mix(in oklch, var(--muted) 50%, transparent); padding: 16px; border-radius: 0 0 14px 14px; }
```

### 3.3 Input / Textarea / Select

| Propiedad     | Input / Select trigger        | Textarea                    |
| ------------- | ----------------------------- | --------------------------- |
| Altura        | 32px                          | mín. 64px (auto-crece)      |
| Padding       | `4px 10px`                    | `8px 12px`                  |
| Borde         | 1px `--input`                 | 1px `--input`               |
| Radio         | 10px                          | 10px                        |
| Fondo         | transparente                  | transparente                |
| Texto         | 14px, `--foreground`          | 14px                        |
| Placeholder   | `--muted-foreground`          | `--muted-foreground`        |

Placeholder textarea "Motivo": `"Escribe brevemente el motivo de tu solicitud"`.
Placeholder textarea rechazo: `"Indica el motivo del rechazo…"`.

**Regla dark mode para `<select>` / `<option>`:** Los navegadores NO heredan `background-color` / `color` de CSS variables en el menú desplegable nativo. Por tanto, todo `<select>` (y `.form-select`) debe tener CSS explícito:
```css
select, .form-select { background-color: var(--background); color: var(--foreground); }
select option, .form-select option { background-color: var(--popover); color: var(--popover-foreground); }
```
Sin esto, el texto del `<option>` es invisible en modo oscuro (blanco sobre blanco).

### 3.4 Badge (`.badge`)

```css
.badge {
  display: inline-flex; align-items: center; gap: 4px;
  height: 20px; padding: 2px 8px; border-radius: 26px;
  font-size: 12px; font-weight: 500;
}
.badge--secondary { background: var(--secondary); color: var(--secondary-foreground); }
.badge--default { background: var(--primary); color: var(--primary-foreground); }
```

### 3.5 Badge de estado (`.status-badge`)

| Estado    | Etiqueta    | Clase CSS                                |
| --------- | ----------- | ---------------------------------------- |
| pendiente | "Pendiente" | `border-transparent bg-\[rgba(245,158,11,.15)\] text-\[#b45309\]` (claro) / `text-\[#fbbf24\]` (oscuro) |
| aprobada  | "Aprobada"  | `border-transparent bg-\[rgba(16,185,129,.15)\] text-\[#047857\]` (claro) / `text-\[#34d399\]` (oscuro) |
| rechazada | "Rechazada" | `border-transparent bg-\[rgba(229,72,77,.15)\] text-\[#e5484d\]` |
| cancelada | "Cancelada" | `border-transparent bg-muted text-muted-foreground` |

### 3.6 Tabla (`.table`)

- Ancho completo, `font-size: 14px`.
- `th`: texto `--muted-foreground`, sin peso extra.
- `tr`: borde inferior 1px `--border`. Filas clicables: `cursor: pointer`, hover `background: color-mix(in oklch, var(--muted) 50%, transparent)`.
- Fuente mono en columna de folios.

### 3.7 Paginación (`.table-pagination`)

Fila flex con `justify-content: space-between`, gap 12px:
- Izquierda: "Mostrando **X**–**Y** de **Z**" (números en `--foreground` peso 500).
- Derecha: botones "‹ Anterior" + "página / total" + "Siguiente ›".

### 3.8 Diálogo (`.dialog`) y Sheet (`.sheet`)

**Diálogo (modal centrado):**
- Overlay: `position:fixed; inset:0; background: rgba(0,0,0,.5); backdrop-filter: blur(4px); -webkit-backdrop-filter: blur(4px); z-index:50;`. El blur es **obligatorio** en todos los overlays del sistema.
- Contenido: centrado, `max-width: 448px` (variante `.dialog-content--sm`: `max-width: 400px`), `width: ~90vw` en móvil, `background: var(--background)`, `border-radius: 14px`, `box-shadow: 0 10px 15px rgba(0,0,0,.1)`, `z-index:51`.
- Animaciones manejadas por JS (fade + scale).
- Header con título + descripción. Body: `max-height: 55–60vh; overflow-y: auto`.
- Footer: barra a sangre, borde superior, fondo `muted/50`.

**Modal de confirmación destructiva — patrón reutilizable:**
Usado para acciones destructivas (cancelar solicitud, etc.). **Nunca usar `confirm()` nativo del navegador.**
- Diálogo pequeño (`.dialog-content--sm`, `max-width: 400px`) centrado.
- Header con título en negrita ("¿Cancelar esta solicitud?") + descripción breve ("Esta acción no se puede deshacer.").
- Body (opcional): referencia del ítem afectado (folio, nombre, etc.) centrado, con clase `.mono` y `color: var(--muted-foreground)`.
- Footer con 2 botones centrados: "Volver" (`.btn--outline`, cierra el modal sin acción) + botón de acción destructiva (`.btn--destructive`, ejecuta la acción vía fetch).
- Al confirmar, se llama `closeDialog()` del modal + `showToast()` de éxito/error + `window.location.reload()` si corresponde.
- El overlay usa el mismo `backdrop-filter: blur(4px)` estándar.

**Diálogo de detalle (solo lectura):**
Reutiliza la misma estructura que el modal de detalle de revisión del Aprobador, pero **sin los botones de acción** (Rechazar/Aprobar/Confirmar rechazo) y **sin la sección de rechazo** (`#reject-section`). El footer contiene únicamente un botón "Cerrar" (`.btn--outline` con `data-dialog-close`). Usado en:
- Empleado: botón "Ver" en la tabla "Mis solicitudes" → `openDetailDialog()`.
- RRHH: clic en fila de tabla → `openHRDetail()`.
- Aprobador: cuando el estado no es "pendiente", el botón "Ver" también abre el modo solo lectura del mismo diálogo.

**Sheet (panel deslizante):**
- Escritorio (≥640px): desde la **derecha**, `width: 100%` hasta `max-width: 448px`, alto completo, borde izquierdo.
- Móvil (<640px): desde **abajo**, `width: 100%` (sin max-width), sin border-radius.
- Animaciones: slide + fade controladas por JS.
- Header con título + descripción; body scrollable; footer con borde superior.

### 3.9 Calendario de rango

Rejilla de 7 columnas (días de semana), locale español. Cada día es un botón de 32px. Navegación entre meses con flechas. Días anteriores a hoy deshabilitados. Rango seleccionado se resalta con color primario. Días que exceden el saldo se pintan con fondo `--destructive` y texto blanco.

**Comportamiento:** al hacer clic en una nueva fecha con rango completo, la selección se reinicia. Cuando un día es simultáneamente rango y excede saldo, prevalece el rojo.

### 3.10 Tooltip

```css
.tooltip-content {
  z-index: 50; display: inline-flex; align-items: center;
  width: fit-content; max-width: 288px;
  gap: 6px; padding: 6px 12px; border-radius: 8px;
  font-size: 12px; background: var(--foreground); color: var(--background);
}
.tooltip-arrow { width: 10px; height: 10px; background: var(--foreground); transform: rotate(45deg); border-radius: 2px; }
```

### 3.11 ToggleGroup (filtros)

```css
.toggle-group { display: flex; flex-direction: row; width: fit-content; gap: 2px; border-radius: 10px; overflow-x: auto; }
.toggle-item { display: inline-flex; align-items: center; justify-content: center; height: 28px; padding: 0 10px; border: 1px solid var(--input); border-radius: 8px; font-size: 0.8rem; font-weight: 500; background: transparent; color: var(--foreground); cursor: pointer; }
.toggle-item:hover { background: var(--muted); }
.toggle-item.active { background: var(--muted); }
```

### 3.12 Barra de progreso (`.progress`)

```css
.progress-track { position: relative; height: 4px; width: 100%; border-radius: 9999px; background: var(--muted); overflow: hidden; }
.progress-indicator { height: 100%; border-radius: 9999px; background: var(--primary); transition: all 150ms; }
```

---

## 4. Partial Views (bloques puramente visuales)

Estos archivos `.cshtml` no contienen lógica de datos propia. Reciben el modelo ya calculado desde el Controller o View Component y solo renderizan HTML.

### `_StatusBadge.cshtml`

```razor
@model string @* "pendiente" | "aprobada" | "rechazada" | "cancelada" *@
@{
  var (label, cls) = Model switch {
    "pendiente" => ("Pendiente", "badge--pending"),
    "aprobada"  => ("Aprobada", "badge--approved"),
    "rechazada" => ("Rechazada", "badge--rejected"),
    "cancelada" => ("Cancelada", "badge--canceled"),
    _ => (Model, "")
  };
}
<span class="badge @cls">@label</span>
```

### `_StatCard.cshtml`

```razor
@model StatCardViewModel
<div class="card">
  <div class="card-content flex items-center gap-4">
    <div class="stat-icon @Model.AccentClass">
      @* SVG inline del ícono *@
    </div>
    <div class="flex flex-col gap-0.5">
      <span class="text-sm text-muted-foreground">@Model.Label</span>
      <span class="text-2xl font-semibold tabular-nums">@Model.Value</span>
      @if (!string.IsNullOrEmpty(Model.Hint))
      {
        <span class="text-xs text-muted-foreground">@Model.Hint</span>
      }
    </div>
  </div>
</div>
```

### `_RequestTimeline.cshtml`

```razor
@model IEnumerable<RequestEventViewModel>
<ol class="timeline">
  @foreach (var evt in Model.OrderBy(e => e.Timestamp))
  {
    <li class="timeline-item">
      <div class="timeline-marker @evt.CssClass">
        @* SVG según tipo *@
      </div>
      <div class="timeline-body">
        <span class="text-sm font-medium">@evt.Label</span>
        <span class="text-xs text-muted-foreground">@evt.Actor · @evt.FormattedTimestamp</span>
        @if (!string.IsNullOrEmpty(evt.Note))
        {
          <p class="text-sm text-muted-foreground">@evt.Note</p>
        }
      </div>
    </li>
  }
</ol>
```

### `_RequestDetailDialog.cshtml`

Renderiza el HTML del diálogo (inicialmente oculto con `display:none`). El JS lo muestra/oculta y rellena datos vía fetch o datos embebidos en `data-*` atributos.

---

## 5. View Components (bloques con lógica de datos)

Se usan para cualquier bloque que necesite lógica propia (calcular saldo, consultar datos, validar).

### `EmployeeDashboardViewComponent`

- **InvokeAsync:** recibe `employeeId`, calcula saldo anual, días consumidos (solo aprobadas), saldo disponible, porcentaje de uso, conteo por estado.
- **View:** `Views/Shared/Components/EmployeeDashboard/Default.cshtml` — renderiza grid de StatCards + tarjeta de uso + resumen.

### `NewRequestSheetViewComponent`

- **InvokeAsync:** recibe `employeeId` y opcionalmente `editingRequestId`. Calcula saldo disponible para validación del lado servidor.
- **View:** renderiza el HTML del Sheet (inicialmente oculto). El JS del cliente maneja la selección de rango, validación en vivo, y el envío vía `fetch` a `EmployeeController.Create` / `EmployeeController.Update`.

### `ManagerRequestDetailViewComponent`

- **InvokeAsync:** recibe `requestId`, determina traslapes con solicitudes aprobadas/pendientes del mismo empleado, calcula saldo post-aprobación.
- **View:** renderiza el diálogo de detalle con botones de aprobar/rechazar. El JS maneja el flujo de confirmación de rechazo y envía decisiones vía fetch.

### `ManagerInboxViewComponent`

- **InvokeAsync:** recibe filtro y búsqueda opcionales, devuelve lista paginada con conteos por estado.
- **View:** renderiza tabla + toggle group de filtros + paginación.

### `HRHistorialViewComponent`

- **InvokeAsync:** recibe filtros (empleado, estado, búsqueda), devuelve lista paginada + conteos.
- **View:** renderiza tabla + filtros + botón exportar CSV.

---

## 6. Controllers y Actions

### `AccountController`

| Action       | Método | URL                          | Descripción                                    |
| ------------ | ------ | ---------------------------- | ---------------------------------------------- |
| `Login`      | GET    | `/account/login`             | Muestra la pantalla de login                   |
| `Login`      | POST   | `/account/login`             | Valida credenciales, inicia sesión             |
| `Logout`     | POST   | `/account/logout`            | Cierra sesión, redirige a login                |
| `ChangeRole` | POST   | `/account/change-role`       | Cambia el rol activo en sesión, redirige       |

### `EmployeeController`

| Action        | Método | URL                          | Descripción                                    |
| ------------- | ------ | ---------------------------- | ---------------------------------------------- |
| `Index`       | GET    | `/employee`                  | Dashboard del empleado                         |
| `Create`      | POST   | `/employee/requests/create`  | Crea nueva solicitud (JSON)                    |
| `Update`      | POST   | `/employee/requests/update`  | Edita solicitud pendiente (JSON)               |
| `Cancel`      | POST   | `/employee/requests/cancel`  | Cancela solicitud pendiente (JSON)             |
| `List`        | GET    | `/employee/requests/list`    | Lista paginada de solicitudes (JSON o Partial) |
| `Validate`    | POST   | `/employee/requests/validate`| Valida campos sin persistir (JSON)             |

### `ManagerController`

| Action          | Método | URL                          | Descripción                                    |
| --------------- | ------ | ---------------------------- | ---------------------------------------------- |
| `Index`         | GET    | `/manager`                   | Bandeja de aprobaciones                        |
| `Detail`        | GET    | `/manager/requests/detail`   | Detalle de solicitud (JSON o Partial)          |
| `Approve`       | POST   | `/manager/requests/approve`  | Aprueba solicitud (JSON)                       |
| `Reject`        | POST   | `/manager/requests/reject`   | Rechaza solicitud con comentario (JSON)        |

### `HRController`

| Action      | Método | URL                          | Descripción                                    |
| ----------- | ------ | ---------------------------- | ---------------------------------------------- |
| `Index`     | GET    | `/rrhh`                      | Panel de RRHH                                  |
| `List`      | GET    | `/rrhh/history/list`         | Lista paginada con filtros (JSON o Partial)    |
| `ExportCsv` | GET    | `/rrhh/history/export-csv`   | Descarga CSV con todas las solicitudes filtradas |

---

## 7. Modelo de datos (ViewModels en C#)

```csharp
// Models/Entities/Employee.cs
public class Employee
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string AvatarColor { get; set; }  // hex o clase CSS
    public string Initials { get; set; }
    public string Role { get; set; }         // "empleado" | "jefe" | "rrhh"
    public List<string> Roles { get; set; }  // todos los roles asignados
    public int AnnualBalance { get; set; }   // días de saldo anual
}

// Models/Entities/LeaveRequest.cs
public class LeaveRequest
{
    public string Id { get; set; }           // "SOL-0001"
    public string EmployeeId { get; set; }
    public string StartDate { get; set; }    // "YYYY-MM-DD"
    public string EndDate { get; set; }
    public int Days { get; set; }
    public string Reason { get; set; }
    public string Status { get; set; }       // "pendiente" | "aprobada" | "rechazada" | "cancelada"
    public DateTime CreatedAt { get; set; }
    public DateTime? DecisionAt { get; set; }
    public string DecisionBy { get; set; }
    public string ManagerComment { get; set; }
    public List<RequestEvent> History { get; set; }
}

// Models/ViewModels/CreateRequestViewModel.cs
public class CreateRequestViewModel
{
    [Required(ErrorMessage = "Debes seleccionar la fecha de inicio.")]
    public string StartDate { get; set; }

    [Required(ErrorMessage = "Debes seleccionar la fecha de fin.")]
    public string EndDate { get; set; }

    [Required(ErrorMessage = "Debes ingresar un motivo para la solicitud.")]
    public string Reason { get; set; }

    public string EditingRequestId { get; set; }  // si es edición
}

// Models/ViewModels/ManagerDecisionViewModel.cs
public class ManagerDecisionViewModel
{
    [Required]
    public string RequestId { get; set; }

    [StringLength(500, ErrorMessage = "El comentario no puede exceder 500 caracteres.")]
    public string Comment { get; set; }  // obligatorio para rechazo
}

// Models/ViewModels/LoginViewModel.cs
public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresa tu usuario o correo.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Ingresa tu contraseña.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}

// Models/ViewModels/ChangeRoleViewModel.cs
public class ChangeRoleViewModel
{
    [Required]
    public string Role { get; set; }
}
```

---

## 8. Interactividad (JavaScript vanilla)

Dado que Razor MVC es renderizado en servidor sin reactividad automática, toda la interactividad del lado del cliente se implementa con JavaScript vanilla en archivos separados bajo `wwwroot/js/`.

### 8.1 `sheet.js` — Abrir/cerrar el Sheet (nueva/editar solicitud)

- Escucha clics en `[data-sheet-trigger]` para abrir y `[data-sheet-close]` para cerrar.
- Maneja animaciones CSS: añade/remueve clases `.sheet--open`, `.sheet--closing`.
- En móvil (<640px) el sheet entra desde abajo (`translateY`); en escritorio desde la derecha (`translateX`).

### 8.2 `calendar-range.js` — Selección de rango y días excedentes

- Renderiza el calendario como grid de botones.
- Maneja clics para seleccionar rango (inicio → fin; si hay rango completo, reinicia).
- Calcula días seleccionados, consulta `data-available-balance` del empleado.
- Pinta en rojo (clase `day--exceeding`) los días que exceden el saldo.
- Actualiza el resumen de selección: "Has seleccionado X días · Saldo disponible: Y días".
- Envía petición `POST /employee/requests/validate` (JSON) para validación del lado servidor antes de habilitar el botón de envío.

### 8.3 `toast.js` — Notificaciones toast

- Función `showToast(message, type)` donde type es `"success"`, `"error"`, `"warning"`, `"info"`.
- Crea un elemento toast posicionado `fixed top-4 right-4 z-60`.
- Auto-destrucción a los 4 segundos.
- Colores: success (esmeralda), error (rojo/destructive), warning (ámbar), info (sky).

### 8.4 `dialog.js` — Modal open/close

- Escucha `[data-dialog-trigger]` y `[data-dialog-close]`.
- Overlay: fade in/out. Contenido: fade + scale (0.95 → 1).
- Cierra al hacer clic fuera del contenido o presionar Escape.

### 8.5 `toggle-group.js` — Filtros de estado

- Escucha clics en `[data-toggle-item]`.
- Alterna clase `.active` y actualiza `data-filter` en un campo oculto.
- Dispara recarga de la tabla vía fetch (o recarga de página con query params).

### 8.6 `tooltip.js` — Tooltips

- Muestra tooltip al hacer hover/foco en `[data-tooltip-trigger]`.
- Posiciona `data-tooltip-content` relativo al trigger con `sideOffset=4`.
- Flecha con rotación 45°, fondo `--foreground`.

### 8.7 `validation.js` — Validación en cliente del formulario

- Escucha `input` y `change` en los campos del formulario de solicitud.
- Deshabilita el botón de envío (`[data-submit-btn]`) hasta que:
  - Haya una fecha de inicio y fin seleccionadas.
  - El motivo no esté vacío.
  - Los días solicitados no excedan el saldo disponible.
- Muestra `FieldError` inline para campos inválidos.
- La validación del lado servidor se ejecuta al enviar (POST) y devuelve errores JSON si falla.

### 8.8 `theme.js` — Modo claro/oscuro

- Detecta `prefers-color-scheme` del SO al cargar.
- Aplica clase `.dark` en `<html>` si corresponde.
- Al hacer clic en el toggle, cambia la clase y guarda preferencia en `localStorage`.
- El toggle muestra luna (sugiere "modo oscuro") en modo claro, y sol (sugiere "modo claro") en modo oscuro.

### 8.9 `user-menu.js` — Menú de usuario

- Abre/cierra dropdown al hacer clic en el trigger.
- Al seleccionar un rol diferente, envía `POST /account/change-role` con el nuevo rol y recarga la página.
- Al hacer clic en "Cerrar sesión", envía `POST /account/logout`.

### 8.10 Flujo de creación/edición de solicitud (ejemplo completo)

1. Usuario hace clic en "Crear solicitud" → JS abre el sheet (`sheet.js`).
2. Sheet muestra calendario (`calendar-range.js`). Usuario selecciona rango.
3. JS calcula días, compara con `data-available-balance`, pinta excedentes en rojo, actualiza resumen.
4. Usuario escribe motivo. JS valida en cliente, deshabilita botón si hay error.
5. Usuario hace clic en "Enviar solicitud" → JS recoge datos, hace `fetch POST /employee/requests/create` con JSON `{ startDate, endDate, reason }`.
6. Servidor valida (`[Required]` + lógica de negocio: saldo suficiente, sin traslape), devuelve `{ ok: true }` o `{ ok: false, error: "..." }`.
7. JS: si `ok` → `showToast("success", ...)` + cierra sheet; si no → `showToast("error", error)`.

### 8.11 Flujo de aprobación/rechazo (ejemplo completo)

1. Jefe hace clic en "Revisar" en la tabla → JS abre diálogo de detalle (`dialog.js`), carga datos vía `fetch GET /manager/requests/detail?id=X` como HTML parcial o JSON.
2. Jefe ve detalle con saldo, traslapes, historial.
3. Jefe hace clic en "Aprobar" → `fetch POST /manager/requests/approve` con `{ requestId }`.
4. Servidor valida (traslape con aprobadas al momento de aprobar), devuelve `{ ok }`.
5. JS: si ok → toast éxito + cierra diálogo + refresca la tabla; si no → toast error.
6. Jefe hace clic en "Rechazar" → se muestra textarea de comentario (JS, sin recargar).
7. Jefe escribe comentario (obligatorio), hace clic en "Confirmar rechazo" → `fetch POST /manager/requests/reject` con `{ requestId, comment }`.

---

## 9. Estructura global (AppShell)

Presente en todas las vistas tras iniciar sesión, implementado en `_Layout.cshtml`.

- **Contenedor raíz:** `display:flex; flex-direction:column; min-height:100vh; background: var(--background);`
- **Header (barra superior):** `position: sticky; top:0; z-index:40; border-bottom:1px solid var(--border); background: rgba(fondo, .8); backdrop-filter: blur;`
  - Interior centrado en `max-width:1152px`, padding `12px 16px`, `display:flex; justify-content:space-between; align-items:center;`
  - **Izquierda (logo):** cuadro de 32px `rounded-lg` con fondo `--primary` e ícono calendario (`CalendarCheck`) en `--primary-foreground` (16px); al lado (oculto en móvil) el texto "**PermisosApp**" (14px/600) y debajo "Permisos y Vacaciones" (12px `--muted-foreground`).
  - **Derecha:** `_ThemeToggle` + `_UserMenu`.
- **Main:** contenedor centrado `max-width:1152px`, padding `24px 16px`, `flex: 1`.

El estado de "usuario logueado y rol activo" vive en la sesión del servidor (`HttpContext.Session` o cookie de autenticación). El cambio de rol dispara una petición `POST /account/change-role` que actualiza la sesión y redirige.

---

## 10. Módulo: Login (`Account/Login.cshtml`)

Pantalla previa a la sesión. Layout centrado en una columna estrecha `max-width: 28rem (448px)`.

- **Estructura:** `<main>` a pantalla completa, columna centrada con padding `32px 16px`.
- **Cabecera superior:** logo (cuadro 36px `--primary` con ícono calendario 20px) + "PermisosApp"; a la derecha el `ThemeToggle`.
- **Bloque central (centrado verticalmente):**
  - Badge secundario "Demo interactivo".
  - H1 (24px/600): "Gestión de Permisos y Vacaciones".
  - Párrafo `--muted-foreground` (14px): invitación a iniciar sesión.
  - **Tarjeta "Iniciar sesión":**
    - Campo "Usuario o correo" (input texto).
    - Campo "Contraseña" (input password con botón mostrar/ocultar).
    - Botón primario ancho completo "Iniciar sesión".
    - **Caja de credenciales demo:** usuarios clickeables que autorrellenan el formulario.
- Al iniciar sesión con éxito → redirige a `/employee` (o la vista del primer rol). El JS muestra toast de éxito antes de la redirección (opcional).
- El formulario se envía por POST tradicional (no fetch) o puede ser AJAX con redirección en el cliente.

---

## 11. Módulo: Empleado (`Employee/Index.cshtml`)

### 11.1 Encabezado de vista

Fila (columna en móvil) con `justify-content: space-between` y `margin-bottom: 0` explícito:
- Izquierda: `<div>` con H1 "Hola, {primer nombre}" (20px/600) + párrafo descriptivo obligatorio debajo: "Consulta tu saldo, registra permisos y da seguimiento a tus solicitudes." (14px/`text-muted-foreground`, `margin-top: 2px`).
- Derecha: botón primario "Crear solicitud" que abre el Sheet.
- **Regla de spacing:** `.page-header` tiene `margin-bottom: 0`. El espaciado entre el header y el bloque siguiente se controla exclusivamente mediante el `gap` del contenedor padre (`<div class="flex flex-col gap-6">`), que provee 24px uniformes entre todos los bloques verticales de la página.

### 11.2 Dashboard (`EmployeeDashboardViewComponent`) — Layout de grids

**REGLAS DE LAYOUT (obligatorias):**

1. **Fila de 3 StatCards:** "Saldo anual", "Días consumidos" (acento ámbar), "Saldo disponible" (acento esmeralda).
   - Grid exacto: `display: grid; grid-template-columns: repeat(3, 1fr); gap: 24px`.
   - Clases CSS: `grid grid-cols-1 sm:grid-cols-3 gap-6`.
   - **Nunca apilar verticalmente** — los 3 StatCards siempre van en una fila de 3 columnas iguales en desktop. En móvil (<640px) colapsan a 1 columna.
   - Cada StatCard se renderiza con `_StatCard.cshtml`.

2. **Fila inferior (grid 2 columnas, 2fr / 1fr):**
   - A la izquierda: tarjeta **"Uso del saldo anual"** con cifra 30px, barra de progreso, días restantes. Ocupa 2/3 del ancho (2fr).
   - A la derecha: tarjeta **"Resumen de solicitudes"** con conteo por estado (Pendiente/Aprobada/Rechazada/Cancelada). Ocupa 1/3 (1fr).
   - Grid exacto: `display: grid; grid-template-columns: 2fr 1fr; gap: 24px`.
   - Clases CSS: `grid grid-cols-1 lg:grid-cols-3 gap-6` con la tarjeta izquierda en `grid-column: span 2`.
   - En móvil (<640px) colapsa a 1 columna (todo apilado).

### 11.3 Mis solicitudes (`EmployeeRequestsViewComponent`) — Layout de tabla

**REGLAS DE LAYOUT (obligatorias):**

1. **Header de la tarjeta:**
   - Título "Mis solicitudes" a la izquierda.
   - Select de ordenamiento a la derecha (Más recientes / Más antiguas / Por estado).

2. **Tabla** — columnas en este orden exacto:
   | ID (mono) | Fechas | Días | Estado | Creada | Acciones |

3. **Acciones por fila:**
   - "Ver" siempre visible (btn ghost).
   - Solo si `Estado == "pendiente"`: "Editar" (btn outline) + "Cancelar" (btn destructive).

4. **Pie de tarjeta (card-footer), contenido en dos filas:**
   - **Fila superior:** paginación con texto "Mostrando X–Y de Z" a la izquierda y botones "‹ Anterior · N/N · Siguiente ›" a la derecha.
   - **Fila inferior:** botón "Crear solicitud" de ancho completo (btn btn--default btn--full) que abre el Sheet. Este botón es adicional al que ya existe en el encabezado de la página.

5. **Paginación:** 8 registros por página. La paginación se muestra incluso en página 1 de N (no ocultarla cuando totalPages <= 1).

**Regla de layout vertical:** Los 3 bloques de la página Empleado (dashboard, tabla mis solicitudes, sheet) van envueltos en un contenedor `<div class="flex flex-col gap-6">` para garantizar un gap uniforme de **24px (`gap-6`)** entre cada bloque, incluyendo el espacio antes de "Mis solicitudes". Nunca usar `margin-bottom` independiente en cada componente.

### 11.4 Nueva/Editar solicitud (`NewRequestSheetViewComponent` + JS)

Panel lateral (Sheet) que sirve tanto para crear como para editar.
- Header: título "Nueva solicitud" / "Editar solicitud".
- Campos: calendario de rango, resumen de selección, textarea de motivo.
- Footer: "Cancelar" + "Enviar solicitud" / "Guardar cambios".
- Validación: Data Annotations en servidor + JS en cliente para feedback inmediato (deshabilitar botón).
- Al enviar con éxito → toast + cierre de sheet + refresco de tabla.

---

## 12. Módulo: Jefe / Aprobador (`Manager/Index.cshtml`)

### 12.1 Fila de StatCards — Layout de grids

**REGLAS DE LAYOUT (obligatorias):**

Las 4 StatCards ("Pendientes" (ámbar), "Aprobadas" (esmeralda), "Colaboradores", "Días aprobados") van en un grid de una sola fila:
- Desktop (≥1024px): `grid-template-columns: repeat(4, 1fr); gap: 24px`.
- Tablet (≥640px): `grid-template-columns: repeat(2, 1fr); gap: 24px`.
- Móvil (<640px): `grid-template-columns: 1fr;`.
- Clases CSS: `grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6`.
- **Nunca apilar las 4 verticalmente en desktop** — siempre deben mostrar 4 por fila.

### 12.2 Bandeja de aprobaciones — Layout de tabla

**REGLAS DE LAYOUT (obligatorias):**

1. **Fila de controles arriba de la tabla:**
   - ToggleGroup de filtros: "Pendientes (n)", "Aprobadas (n)", "Rechazadas (n)", "Todas (n)".
   - Buscador por nombre o folio (input de búsqueda con ícono lupa).
   - Ambos en una misma fila, sin labels.

2. **Tabla** — columnas en este orden exacto:
   | Colaborador (avatar + nombre + rol) | Periodo | Días | Estado | Acción |

3. **Acción por fila:**
   - Si `Estado == "pendiente"`: botón "Revisar" (btn outline).
   - Si otro estado: botón "Ver" (btn outline).
   - El botón usa `event.stopPropagation()` en el `<td>` contenedor para evitar doble apertura.

4. **Filas clicables:**
   - El `<tr>` completo tiene `onclick="openManagerDetail('@r.Id')"`.
   - El `<td>` que contiene el botón tiene `onclick="event.stopPropagation()"` para que el clic en el botón no dispare dos veces la apertura del modal.
   - CSS base ya define `cursor: pointer` y `hover: background` en `tbody tr`.

5. **Paginación:** 10 registros por página. Misma estructura que Employee: "Mostrando X–Y de Z" + botones.

### 12.3 Detalle de revisión (JS + diálogo modal)

Diálogo modal:

**Layout:**
- Header con folio (`.mono font-semibold`) + StatusBadge.
- Caja de empleado (avatar + nombre + email).

**Mini-tarjetas de saldo (`.detail-balance-grid`):**
```html
<div class="detail-balance-grid">
  <div class="mini-card"><span class="mini-card-value">N</span><span class="mini-card-label">Disponible</span></div>
  ...
</div>
```
- Grid de 3 columnas: `grid-template-columns: repeat(3, 1fr); gap: 12px`.
- Cada tarjeta: `border: 1px solid var(--border); border-radius: var(--radius); padding: 12px 8px; text-align: center;`.
- **Valor numérico arriba** (20px, 600, tabular-nums), **label debajo** (12px, muted-foreground).
- `postApproval` negativo: clase `.negative` → `color: var(--destructive)`.

**Alertas de traslape:**
- Generadas desde el servidor (HTML en `alerts` del JSON) con estructura:
  ```html
  <div class="alert alert--destructive">
    <svg><!-- icono triángulo warning --></svg>
    <div>
      <strong>Traslape con una solicitud aprobada</strong>
      <p style="margin:2px 0 0;font-size:12px;color:var(--destructive)">No es posible aprobar: …</p>
    </div>
  </div>
  ```
- Destructive: `background: rgba(229,72,77,0.1); border: 1px solid rgba(229,72,77,0.4)`.
- Si hay traslape con aprobada: botón "Aprobar" se deshabilita (`disabled`) con tooltip explicativo.

**Filas informativas:** Fechas, Motivo, Comentario (— si vacío).

**Historial de la solicitud (`.detail-timeline-section`):**
- Título: `<h4 style="font-size:14px;font-weight:600;margin-bottom:8px">Historial de la solicitud</h4>`.
- Timeline renderizado desde server (`timelineHtml`): lista `<ol class="timeline">` con items `<li class="timeline-item">` que contienen marcador circular (`.timeline-marker`) + cuerpo (actor · fecha, y nota opcional).

**Footer:**
- Solo 2 botones (sin "Cerrar"):
  - "Rechazar" (`.btn--destructive`) — al hacer clic se muestra textarea de comentario + botón "Confirmar rechazo".
  - "Aprobar" (`.btn--default`) — deshabilitado si `hasOverlapApproved === true`.
- El diálogo se cierra haciendo clic en el overlay o pulsando Escape (manejado por `dialog.js`).

**Modo rechazo:**
- textarea de comentario (obligatorio, máx. 500 caracteres, placeholder `"Indica el motivo del rechazo…"`).
- Botón "Confirmar rechazo" (`.btn--destructive`) deshabilitado hasta que el comentario tenga texto.

**Acciones vía fetch:** `POST /Manager/Index?handler=Approve` / `POST /Manager/Index?handler=Reject`.

---

## 13. Módulo: Recursos Humanos (`HR/Index.cshtml`)

### 13.1 Vista de consulta e informes — Layout de grids

**REGLAS DE LAYOUT (obligatorias):**

1. **Fila de 4 StatCards:** "Total solicitudes", "Pendientes" (ámbar), "Aprobadas" (esmeralda), "Empleados".
   - Mismo grid que Aprobador: `grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6`.
   - **Nunca apilar verticalmente en desktop.**

2. **Fila de filtros (dentro de un card propio, no en el header de la tabla):**
   - Una sola fila con 3 controles del mismo tamaño, sin labels visibles:
     - Buscador (input con ícono lupa, placeholder: "Buscar por nombre o folio…")
     - Select de empleado (placeholder: "Todos los empleados")
     - Select de estado (placeholder: "Todos los estados")
   - Grid exacto: `display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 12px`.
   - Clases CSS: `grid grid-cols-1 sm:grid-cols-3 gap-3`.
   - En móvil los 3 campos se apilan verticalmente.
   - **El botón "Exportar CSV" NO va en esta fila** — va en el header de la tarjeta de la tabla.

3. **Historial de solicitudes (card):**
   - **Header:** título "Historial de solicitudes" a la izquierda + conteo de registros + botón "Exportar CSV" (btn btn--outline btn--sm) alineados a la derecha.
    - **Tabla** — columnas en este orden exacto:
      | Colaborador (avatar + nombre + email) | Folio (mono) | Periodo | Días | Estado | Creada |
    - **Filas clicables:** igual que Aprobador — `<tr onclick="openHRDetail('@r.Id')">`, el modal se abre vía fetch a `HR/Index?handler=Detail&id=...`. No tiene botones de acción, solo "Cerrar".
    - **Paginación:** 10 registros por página. Misma estructura que Employee.

### 13.2 Exportar CSV

La exportación es del lado servidor: el Handler `HR/Index.cshtml.cs.OnGetExportCsv` genera un archivo CSV con BOM y cabeceras: Folio, Empleado, Correo, Inicio, Fin, Días, Estado, Motivo, Decidido por. Se invoca con `GET /HR/Index?handler=ExportCsv` (descarga directa, no AJAX).

---

## 14. Notas de arquitectura MVC

1. **Razor puro, sin Blazor.** Todo el sitio se renderiza en el servidor. No hay componentes `.razor`, no hay `[Parameter]`/`EventCallback`, no hay render modes interactivos. Cada petición produce HTML completo.

2. **El CSS global vive en `wwwroot/css/tokens.css`.** Se referencia desde `_Layout.cshtml` con `<link rel="stylesheet" href="~/css/tokens.css" asp-append-version="true" />`. Contiene todos los tokens de color (OKLCH light/dark), tipografía, spacing, radios, sombras, z-index y breakpoints. Los estilos de componentes (`.btn`, `.card`, `.table`) van en `components.css`.

3. **El JS de interacción vive en `wwwroot/js/`.** Un archivo por patrón: `sheet.js`, `calendar-range.js`, `toast.js`, `dialog.js`, `toggle-group.js`, `tooltip.js`, `theme.js`, `validation.js`, `user-menu.js`, `pagination.js`. Se cargan desde `_Layout.cshtml` con `defer`.

4. **El estado de sesión (usuario logueado, rol activo) vive en el servidor** (`HttpContext.Session` o cookie de autenticación). El cambio de rol dispara una petición al servidor (`POST /account/change-role`) que actualiza la sesión y redirige. No es un cambio puramente de cliente como en el prototipo original.

5. **Partial Views vs View Components:**
   - **Partial Views** (`_StatusBadge.cshtml`, `_StatCard.cshtml`, `_RequestTimeline.cshtml`, `_UserAvatar.cshtml`, `_ThemeToggle.cshtml`, `_UserMenu.cshtml`, `_TablePagination.cshtml`) para bloques puramente visuales sin lógica de datos propia. Reciben el modelo ya calculado.
   - **View Components** (`EmployeeDashboardViewComponent`, `NewRequestSheetViewComponent`, `ManagerInboxViewComponent`, `ManagerRequestDetailViewComponent`, `HRHistorialViewComponent`) para cualquier bloque que necesite lógica de datos (calcular saldo, determinar traslapes, consultar repositorio).

6. **Validación de formularios:** Data Annotations en los ViewModels de C# (`[Required]` para motivo y comentario de rechazo, `[StringLength(500)]` para comentario) para la validación en servidor. JavaScript en cliente para feedback inmediato (deshabilitar el botón de envío hasta que todos los campos sean válidos). La validación del lado servidor siempre se ejecuta al enviar, incluso si el JS falla.

7. **Interactividad que dependía de React/estado reactivo** (abrir/cerrar Sheet, seleccionar rango en Calendar, pintar en rojo los días que exceden el saldo, validación en vivo, cambio de rol, toasts) se resuelve con JavaScript vanilla, llamando a endpoints de los Controllers vía `fetch` cuando se necesita persistir datos, y manejando el estado puramente visual en JS del lado del cliente.

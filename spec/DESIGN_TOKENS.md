# Guía de Diseño — Gestión de Permisos y Vacaciones

Este documento describe **con precisión** el diseño del sistema **Gestión de Permisos y Vacaciones**, módulo por módulo, para que otra IA (o persona) pueda **recrearlo en HTML + CSS puro** obteniendo un resultado visualmente idéntico.

El proyecto original está hecho con Next.js + Tailwind CSS v4 + componentes basados en Base UI (estilo shadcn). Aquí se traduce cada token, componente y pantalla a valores concretos (px, hex, radios, sombras) para que no dependas del framework.

> **Regla de oro nº 1 — Es un diseño MONOCROMÁTICO.** El color "primario" NO es azul ni morado: es **gris muy oscuro / casi negro** en modo claro y **gris muy claro / casi blanco** en modo oscuro. Toda la interfaz es en escala de grises. El único color con matiz son: (a) los estados de solicitud (ámbar, esmeralda, rojo, gris), (b) los colores de avatar de cada persona, y (c) el rojo destructivo.

> **Regla de oro nº 2 — Usa siempre tokens semánticos.** Nunca escribas `color: black` o `background: white` directo. Usa las variables CSS (`--background`, `--foreground`, `--card`, `--primary`, …) para que el modo claro/oscuro funcione automáticamente.

> **Regla de oro nº 3 — Todos los colores deben usar CSS variables.** No uses valores hardcodeados (`#xxx`, `rgba(r,g,b,a)`, `white`, `black`) en ningún archivo CSS, HTML o JS. Si necesitas un color nuevo, agrégalo como variable en `tokens.css` primero, con su correspondiente valor en modo claro, `.dark` y `@media (prefers-color-scheme: dark)`. Esto asegura que los cambios de tema solo requieran modificar `tokens.css` y que los componentes nunca tengan colores sueltos.

> **Regla de oro nº 4 — Las clases CSS usan tokens, no colores directos.** Ninguna clase debe tener `background: #xxx` ni `color: #xxx`. Cada clase referencia tokens semánticos (`var(--background)`, `var(--card)`, `var(--muted)`, `var(--muted-alpha-40)`, etc.). Así, para cambiar el fondo del sistema solo editas `--background` en `tokens.css` y todas las clases que heredan ese fondo se actualizan automáticamente, sin tener que ir clase por clase.

---

## 1. Fundamentos

### 1.1 Tipografía

Dos fuentes de la familia **Geist** (Google Fonts):

| Uso            | Fuente        | `font-family` CSS                                              | Dónde se usa                          |
| -------------- | ------------- | -------------------------------------------------------------- | ------------------------------------- |
| Cuerpo / UI    | Geist Sans    | `'Geist', ui-sans-serif, system-ui, sans-serif`                | Todo el texto por defecto             |
| Monoespaciada  | Geist Mono    | `'Geist Mono', ui-monospace, monospace`                        | Folios/IDs (`SOL-0001`), contraseña demo, títulos de diálogo de detalle |

- El `<body>` lleva `font-family: var(--font-sans)` y `-webkit-font-smoothing: antialiased`.
- Interlineado de cuerpo: relajado (~1.5). Los títulos usan `text-balance` / `text-pretty` (equivalente CSS: `text-wrap: balance` / `text-wrap: pretty`).
- Números tabulares: las cifras (días, contadores, saldos) usan `font-variant-numeric: tabular-nums` para que no "bailen". 

**Escala tipográfica real usada en el sistema:**

| Nombre        | Tamaño | Peso            | Clase Tailwind        | Uso                                                |
| ------------- | ------ | --------------- | --------------------- | -------------------------------------------------- |
| Título página | 24px   | 600 (semibold)  | `text-2xl font-semibold` | H1 de Bandeja, RRHH                             |
| Título vista  | 20px   | 600             | `text-xl font-semibold`  | "Hola, {nombre}" (empleado)                     |
| Cifra grande  | 30px   | 600             | `text-3xl`               | Progreso "días consumidos"                      |
| Cifra stat    | 24px   | 600             | `text-2xl`               | Valor de StatCard                               |
| Título card   | 16px   | 500 (medium)    | `text-base font-medium`  | `CardTitle`                                     |
| Cuerpo        | 14px   | 400             | `text-sm`                | Texto general, tablas, inputs                   |
| Secundario    | 12px   | 400             | `text-xs`                | Ayudas, metadatos, fechas, "hint" de StatCard   |
| Micro         | 10px   | 500             | `text-[10px]`            | Badge de rol en el menú de usuario              |

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
| `--primary-foreground`   | `oklch(0.985 0 0)`           | `#fbfbfb`    | Texto sobre primary (casi blanco)               |
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
| `--popover`              | `oklch(0.205 0 0)`            | `#343434`               | Popovers/menús                        |
| `--primary`              | `oklch(0.922 0 0)`            | `#ebebeb`               | Acción primaria (¡claro en oscuro!)   |
| `--primary-foreground`   | `oklch(0.205 0 0)`            | `#343434`               | Texto sobre primary                   |
| `--secondary`            | `oklch(0.269 0 0)`            | `#434343`               | Secundario                            |
| `--muted`                | `oklch(0.269 0 0)`            | `#434343`               | Fondos sutiles                        |
| `--muted-foreground`     | `oklch(0.708 0 0)`           | `#b5b5b5`               | Texto secundario                      |
| `--accent`               | `oklch(0.269 0 0)`            | `#434343`               | Hover sutil                           |
| `--destructive`          | `oklch(0.704 0.191 22.216)`  | `#ff6b6b`               | Rojo (más claro en oscuro)            |
| `--border`               | `oklch(1 0 0 / 10%)`         | `rgba(255,255,255,.10)` | Bordes translúcidos                   |
| `--input`                | `oklch(1 0 0 / 15%)`         | `rgba(255,255,255,.15)` | Borde de campos                       |
| `--ring`                 | `oklch(0.556 0 0)`           | `#8e8e8e`               | Anillo de foco                        |

#### Tokens no utilizados en este prototipo

Las variables `--chart-1`, `--chart-2`, `--chart-3`, `--chart-4`, `--chart-5` y toda la familia `--sidebar-*` (`--sidebar`, `--sidebar-foreground`, `--sidebar-primary`, `--sidebar-primary-foreground`, `--sidebar-accent`, `--sidebar-accent-foreground`, `--sidebar-border`, `--sidebar-ring`) están declaradas en el CSS pero **no se utilizan** en ningún componente de este prototipo. Pueden omitirse por completo al recrear el sistema en HTML/CSS.

#### Colores semánticos de estado (fijos, NO cambian por tema)

Estos usan escalas fijas de color con opacidad para el fondo, para que "pendiente = ámbar", "aprobada = verde" se reconozcan igual en claro y oscuro.

| Significado | Fondo                | Texto claro   | Texto oscuro       | Hex texto (claro/oscuro) |
| ----------- | -------------------- | ------------- | ------------------ | ------------------------ |
| Pendiente   | `amber-500 / 15%`    | `amber-700`   | `amber-400`        | `#b45309` / `#fbbf24`    |
| Aprobada    | `emerald-500 / 15%`  | `emerald-700` | `emerald-400`      | `#047857` / `#34d399`    |
| Rechazada   | `destructive / 15%`  | `destructive` | `destructive`      | `#e5484d`                |
| Cancelada   | `muted`              | `muted-foreground` | `muted-foreground` | `#8e8e8e`           |
| Info (timeline "creada") | `sky-500 / 15%` | `sky-600` | `sky-400`        | `#0284c7` / `#38bdf8`    |

> Nota: `amber-500/15%` significa el color ámbar 500 (`#f59e0b`) con **15% de opacidad** como fondo. En CSS: `background: rgba(245, 158, 11, 0.15)`.

#### Colores de avatar (uno por persona)

El avatar es un círculo de color sólido con las iniciales en blanco. Cada empleado tiene su color fijo:

| Persona           | Iniciales | Clase           | Hex        |
| ----------------- | --------- | --------------- | ---------- |
| Ana Torres        | AT        | `bg-rose-500`   | `#f43f5e`  |
| Diego Fuentes     | DF        | `bg-sky-500`    | `#0ea5e9`  |
| Sofía Herrera     | SH        | `bg-violet-500` | `#8b5cf6`  |
| Marta Ríos        | MR        | `bg-teal-500`   | `#14b8a6`  |
| Pedro Salas       | PS        | `bg-orange-500` | `#f97316`  |
| Carlos Ramírez    | CR        | `bg-indigo-500` | `#6366f1`  |
| Laura Méndez      | LM        | `bg-fuchsia-600`| `#c026d3`  |

### 1.3 Radios de esquina

Base: `--radius: 0.625rem` (**10px**) — se define en `rem` para que escale con el tamaño de fuente raíz. Escala derivada:

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

- Se usa la escala de Tailwind (múltiplos de 4px): `gap-1`=4px, `gap-2`=8px, `gap-3`=12px, `gap-4`=16px, `gap-6`=24px.
- Separación entre hijos con **`gap`** en flex/grid (nunca `margin` mezclado con `gap`, nunca `space-*`).
- **Espaciado interno de tarjetas:** variable `--card-spacing: 16px` (padding vertical/horizontal y gap entre secciones de la tarjeta). En tarjetas `size="sm"` es 12px.

### 1.5 Sombras y elevación

> **Importante:** las tarjetas de este sistema **NO usan `box-shadow`**, usan un **anillo sutil** de 1px: `ring-1 ring-foreground/10` → en CSS `box-shadow: 0 0 0 1px rgba(37,37,37,0.10)`. En oscuro es `rgba(251,251,251,0.10)`. Esto da un contorno muy tenue en vez de sombra proyectada. Este anillo se aplica a **todas** las tarjetas del sistema (`Card`, `StatCard`, inclusive el contenedor de login).

Las sombras proyectadas solo aparecen en capas flotantes (menús, popovers, diálogos, sheets, toasts):

| Nivel  | CSS                                        | Componentes                       |
| ------ | ------------------------------------------ | --------------------------------- |
| `sm`   | `0 1px 2px rgba(0,0,0,.05)`                | (poco usado)                      |
| `md`   | `0 4px 6px rgba(0,0,0,.1)`                 | Popover, dropdown, calendario     |
| `lg`   | `0 10px 15px rgba(0,0,0,.1)`               | Diálogos (modal), sheets          |

### 1.6 Foco (accesibilidad)

Todos los controles interactivos muestran un anillo de foco visible al navegar con teclado. Patrón real (Base UI):

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

Detalle de botón: al pulsar (`:active`) desciende 1px (`translate-y-px`) salvo si abre un menú.

### 1.8 Z-index

| Capa                 | z-index | Nota                                              |
| -------------------- | ------- | ------------------------------------------------- |
| Header (sticky)      | 40      |                                                   |
| Popover / dropdown   | 40      | Se renderiza fuera del header (portal), aparece sobre él aunque tengan el mismo z-index |
| Tooltip              | 45      |                                                   |
| Overlay de sheet/dialog | 50   |                                                   |
| Contenido de sheet/dialog | 51 |                                                   |
| Toast                | 60      |                                                   |

### 1.9 Breakpoints (mobile-first)

| Prefijo | Ancho    | Efecto principal                                            |
| ------- | -------- | ----------------------------------------------------------- |
| base    | <640px   | 1 columna; sheet sube desde abajo; menús compactos          |
| `sm`    | ≥640px   | Grids de stats a 2 columnas (Jefe/RRHH) / 3 columnas (Empleado); sheet entra desde la derecha |
| `md`    | ≥768px   | Filtros y grids intermedios                                 |
| `lg`    | ≥1024px  | Grids de stats a 4 columnas (Jefe/RRHH); layout completo    |

El contenido se centra en un contenedor `max-width: 72rem (1152px)` (`max-w-6xl`) con padding lateral de 16px.

### 1.10 Fechas — formato de visualización

Todas las fechas se muestran en formato español corto: `"10 mar 2025"` (`day: "2-digit", month: "short", year: "numeric"` con locale `es-ES`). Las fechas con hora incluyen hora y minutos: `"10 mar 2025, 14:30"`.

### 1.11 Detalles de interacción menores

- **`::selection`:** No hay personalización explícita. Usa el color de selección por defecto del navegador.
- **Scrollbar en sheets y diálogos:** Se usa `ScrollArea` con barra de desplazamiento personalizada: ancho 10px (`w-2.5`), thumb `rounded-full` con color `--border`, y track con padding `p-px`. El scroll es nativo (overflow) pero estilizado.
- **Indicador de scroll horizontal en tablas:** Las tablas responsivas se envuelven en un contenedor `overflow-x: auto` sin indicador visual adicional. El usuario descubre el scroll al interactuar con la tabla. En móvil, el desplazamiento horizontal está siempre disponible si la tabla excede el ancho.

---

## 2. Componentes base

### 2.1 Botón (`Button`)

Forma base: inline-flex centrado, `border-radius: 10px` (`rounded-lg`), `font-size: 14px`, `font-weight: 500`, sin salto de línea, `transition: all 150ms`. Los íconos SVG internos miden 16px (`size-4`) por defecto.

**Tamaños de ícono por contexto:**

| Contexto                          | Tamaño | Nota                                         |
| --------------------------------- | ------ | -------------------------------------------- |
| Botón default / icon (16px)       | 16px   | `[&_svg]:size-4`                             |
| Botón icon-sm / tabla acciones    | 14px   | `[&_svg]:size-3.5`                           |
| Login — logo en cabecera          | 20px   | `[&_svg]:size-5` en contenedor 36px          |
| AppShell — logo en header         | 16px   | `[&_svg]:size-4` en contenedor 32px          |
| StatCard — ícono                  | 20px   | `[&_svg]:size-5` dentro del cuadro de 40px   |
| Timeline — ícono de evento        | 14px   | `[&_svg]:size-3.5` dentro del círculo 28px   |
| Buscadores (search)               | 16px   | `size-4`                                     |
| "Cambiar rol activo"              | 14px   | `size-3.5`                                   |
| Chevrons en paginación            | 14px   | `size-3.5`                                   |
| Chevrons en UserMenu              | 16px   | `size-4`                                     |

**Tamaños (¡ojo, son compactos!):**

| Tamaño     | Altura | Padding horizontal | Radio | Íconos | Uso                          |
| ---------- | ------ | ------------------ | ----- | ------ | ---------------------------- |
| `default`  | 32px   | 10px               | 10px  | 16px   | Acciones generales           |
| `sm`       | 28px   | 10px               | 8px   | 14px   | Acciones en tablas/cards     |
| `lg`       | 36px   | 10px               | 10px  | 16px   | (poco usado)                 |
| `icon`     | 32×32  | —                  | 10px  | 16px   | Botón solo-ícono (theme)     |
| `icon-sm`  | 28×28  | —                  | 8px   | 14px   | Íconos en filas de tabla     |

**Variantes:**

| Variante      | Fondo                    | Texto                 | Borde              | Hover                                  | Uso                              |
| ------------- | ------------------------ | --------------------- | ------------------ | -------------------------------------- | -------------------------------- |
| `default`     | `--primary` (`#343434`)  | `--primary-foreground`| ninguno            | `primary` al 80% de opacidad           | Acción principal (Enviar, Aprobar, Revisar) |
| `secondary`   | `--secondary` (`#f7f7f7`)| `--secondary-foreground` | ninguno         | mezcla secondary + 5% foreground       | Acción secundaria                |
| `outline`     | `--background`           | `--foreground`        | 1px `--border`     | fondo `--muted`                        | Cancelar, Editar, Exportar, paginación |
| `ghost`       | transparente             | `--foreground`        | ninguno            | fondo `--muted`                        | Botones-ícono en filas, mostrar/ocultar contraseña |
| `destructive` | `destructive / 10%`      | `--destructive`       | ninguno            | `destructive / 20%`                    | Rechazar, Cancelar solicitud (¡fondo ROJO SUAVE, no sólido!) |
| `link`        | transparente             | `--primary`           | ninguno (subrayado)| subrayado                              | Enlaces de texto                 |

Deshabilitado: `opacity: .5; pointer-events: none`.

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

### 2.2 Tarjeta (`Card`)

Contenedor blanco (o `--card` en oscuro) con esquinas `rounded-xl` (14px), **sin sombra**, con anillo de 1px muy tenue. Estructura vertical con `gap` y padding de 16px.

- Contenedor: `background: var(--card); border-radius: 14px; box-shadow: 0 0 0 1px rgba(37,37,37,.10); overflow: hidden; display:flex; flex-direction:column; gap:16px; padding: 16px 0;`
- `CardHeader`: padding horizontal 16px, grid con gap 4px (título + descripción). Si lleva borde inferior, `padding-bottom: 16px`.
- `CardTitle`: 16px / peso 500 / `line-height: 1.375`.
- `CardDescription`: 14px, color `--muted-foreground`.
- `CardContent`: padding horizontal 16px.
- `CardFooter`: borde superior, fondo `muted/50`, padding 16px, esquinas inferiores redondeadas.

### 2.3 Campos de formulario (Input / Textarea / Select)

| Propiedad     | Input / Select trigger        | Textarea                    |
| ------------- | ----------------------------- | --------------------------- |
| Altura        | 32px (`h-8`)                  | mín. 64px (auto-crece)      |
| Padding       | `4px 10px` (py-1 px-2.5)      | `8px 12px`                  |
| Borde         | 1px `--input`                 | 1px `--input`               |
| Radio         | 10px (`rounded-lg`)           | 10px                        |
| Fondo         | transparente (oscuro: `input/30`) | igual                   |
| Texto         | 14px, color `--foreground`    | 14px                        |
| Placeholder   | `--muted-foreground`          | `--muted-foreground`        |
| Límite caracteres | —                         | 500 (solo en rechazo de jefe, con contador `N / 500`). En "Motivo" de solicitud no tiene límite. |

**Placeholder de textarea "Motivo" en solicitud:** `"Escribe brevemente el motivo de tu solicitud"`.
**Placeholder de textarea "Comentario" en rechazo:** `"Indica el motivo del rechazo…"`.

#### Menú desplegado de Select (`SelectContent` / `SelectItem`)

El contenido del Select (la lista que se abre al hacer clic en el trigger) tiene su propio estilo diferenciado:

| Propiedad                | Valor                                                |
| ------------------------ | ---------------------------------------------------- |
| Fondo (popup)            | `--popover`                                          |
| Texto (popup)            | `--popover-foreground`                               |
| Sombra                   | `md`: `0 4px 6px rgba(0,0,0,.1)` + `ring-1 ring-foreground/10` |
| Border-radius            | 10px (`rounded-lg`)                                  |
| Padding interior         | 4px (`p-1`)                                          |
| Ancho                    | Mínimo 144px (`min-w-36`), igual al trigger (`w-(--anchor-width)`) |
| Altura máxima            | Controlada por `max-h-(--available-height)` con overflow-y auto |
| Ítem — padding           | `4px 32px 4px 6px` (`py-1 pr-8 pl-1.5`)             |
| Ítem — radio             | 6px (`rounded-md`)                                   |
| Ítem — hover/foco        | `background: var(--accent); color: var(--accent-foreground)` |
| Ítem — check seleccionado | Ícono `CheckIcon` de 16px, posicionado `absolute right-2`, color heredado |
| Separador                | `height: 1px; background: var(--border); margin: 4px -4px` |
| Grupo — label            | `font-size: 12px; color: var(--muted-foreground); padding: 4px 6px` |

```css
.select-content {
  background: var(--popover); color: var(--popover-foreground);
  border-radius: 10px; padding: 4px;
  box-shadow: 0 4px 6px rgba(0,0,0,.1), 0 0 0 1px rgba(37,37,37,.1);
  max-height: var(--available-height); overflow-y: auto;
}
.select-item {
  display: flex; align-items: center; gap: 6px;
  padding: 4px 32px 4px 6px;
  border-radius: 6px; font-size: 14px; cursor: default;
  position: relative;
}
.select-item:hover, .select-item:focus { background: var(--accent); }
.select-item[data-selected] .select-check {
  position: absolute; right: 8px;
  display: flex; align-items: center; justify-content: center;
}
```

Estados:
- Foco: `border-color: var(--ring); box-shadow: 0 0 0 3px rgba(181,181,181,.5)`.
- Inválido (`aria-invalid`): `border-color: var(--destructive); box-shadow: 0 0 0 3px rgba(229,72,77,.2)`.
- Deshabilitado: `opacity: .5; cursor: not-allowed; background: input/50`.

**Agrupación de campos (`Field` / `FieldGroup`):**

```css
.field-group { display: flex; flex-direction: column; gap: 16px; }  /* entre campos */
.field       { display: flex; flex-direction: column; gap: 6px; }   /* label→control→ayuda */
.field-label { font-size: 14px; font-weight: 500; }
.field-description { font-size: 12px; color: var(--muted-foreground); line-height: 1.4; }
.field-error       { font-size: 12px; color: var(--destructive); line-height: 1.4; }
```

### 2.4 Checkbox y Radio (selector de roles)

Usados en formularios y en el menú de usuario para cambiar rol activo.

**Checkbox** (no usado directamente en este prototipo, pero disponible en la librería — el radio del menú de usuario usa `CheckIcon` como indicador):

| Propiedad                  | Checkbox                                          |
| -------------------------- | ------------------------------------------------- |
| Tamaño                     | 16×16px (`size-4`)                                |
| Radio                      | 4px (`rounded-[4px]`)                             |
| Borde sin seleccionar      | 1px solid `--input`                               |
| Fondo seleccionado         | `--primary`                                       |
| Borde seleccionado         | `--primary`                                       |
| Check (ícono)              | `CheckIcon` de 14px, color `--primary-foreground` |
| Foco visible               | `ring-3 ring-ring/50`                            |

**Radio del selector de rol en menú de usuario:**

No es un radio circular clásico. El `DropdownMenuRadioItem` muestra un **`CheckIcon`** como indicador de selección, no un círculo relleno.

| Propiedad                  | Valor                                               |
| -------------------------- | --------------------------------------------------- |
| Indicador                  | `CheckIcon` de 16px, posicionado `absolute right-2` |
| Padding del ítem           | `4px 32px 4px 6px` (`py-1 pr-8 pl-1.5`)            |
| Radio                      | 6px (`rounded-md`)                                  |
| Hover/foco                 | `background: var(--accent)`                         |
| Texto                      | 14px, `--popover-foreground`                        |

```css
.radio-item {
  position: relative; cursor: default;
  display: flex; align-items: center; gap: 6px;
  padding: 4px 32px 4px 6px;
  border-radius: 6px; font-size: 14px; outline: none;
}
.radio-item:hover, .radio-item:focus { background: var(--accent); }
.radio-indicator {
  position: absolute; right: 8px;
  display: flex; align-items: center; justify-content: center;
  width: 16px; height: 16px;
}
```

### 2.5 Badge (píldora)

Usado para etiquetas ("Demo interactivo", rol) y como base del badge de estado.

- Base: `display:inline-flex; align-items:center; gap:4px; height:20px; padding:2px 8px; border-radius:26px (rounded-4xl); font-size:12px; font-weight:500;`
- Variante `secondary`: fondo `--secondary`, texto `--secondary-foreground`.
- Variante `default`: fondo `--primary`, texto `--primary-foreground`.

### 2.6 Badge de estado (`StatusBadge`)

Badge con `border: transparent` y color por estado (ver §1.2). Texto = etiqueta en español.

| Estado    | Etiqueta    | Fondo               | Texto (claro / oscuro)    |
| --------- | ----------- | ------------------- | ------------------------- |
| pendiente | "Pendiente" | `rgba(245,158,11,.15)`  | `#b45309` / `#fbbf24` |
| aprobada  | "Aprobada"  | `rgba(16,185,129,.15)`  | `#047857` / `#34d399` |
| rechazada | "Rechazada" | `rgba(229,72,77,.15)`   | `#e5484d`             |
| cancelada | "Cancelada" | `var(--muted)`          | `var(--muted-foreground)` |

### 2.7 Avatar (`UserAvatar`)

Círculo con iniciales en blanco sobre color sólido de la persona (§1.2). Tamaños: `sm`=24px, `default`=32px, `lg`=40px. Texto `sm`=12px, resto 14px, peso 500. Lleva un `::after` con borde sutil (`border: 1px solid var(--border)`) con `mix-blend-mode: darken` en modo claro y `mix-blend-mode: lighten` en modo oscuro, para dar un contorno tenue sin añadir opacidad extra.

### 2.8 Tabla (`Table`)

- Ancho completo, `font-size: 14px`.
- `TableHead` (encabezado): texto `--muted-foreground`, alto compacto, alineado a la izquierda (o `text-right` para columnas numéricas/acciones).
- `TableRow`: borde inferior 1px `--border`. Filas clicables llevan `cursor: pointer` y hover con fondo `muted/50`.
- Celdas numéricas: `text-right` + `tabular-nums`. Folios: `font-family: mono; font-size: 12px`.
- La tabla suele ir dentro de un contenedor `overflow: hidden; border: 1px solid var(--border); border-radius: 10px;` o con `overflow-x: auto` en móvil.

### 2.9 Paginación de tabla (`TablePagination`)

Fila flex con `justify-content: space-between`, gap 12px, padding-top 4px:
- Izquierda: texto 14px `--muted-foreground` → "Mostrando **X**–**Y** de **Z**" (números en `--foreground` peso 500).
- Derecha: botón `outline sm` "‹ Anterior" + texto "página / total" (tabular) + botón `outline sm` "Siguiente ›". Se deshabilitan en los extremos.

### 2.10 Estado vacío (`Empty`)

Centrado vertical, con un ícono dentro de un contenedor redondeado tenue (`EmptyMedia variant="icon"` → cuadro `--muted` redondeado con ícono `--muted-foreground`), un título (peso 500) y una descripción `--muted-foreground` de 14px. Usado cuando no hay solicitudes/resultados.

### 2.11 Timeline de historial (`RequestTimeline`)

Lista vertical (`<ol>`). Cada evento:
- A la izquierda, un círculo de 28px (`rounded-full`) con ícono 14px y color según tipo de evento; debajo, una línea vertical de 1px `--border` que conecta con el siguiente (salvo el último).
- A la derecha: etiqueta del evento (14px, peso 500), luego "actor · fecha/hora" (12px `--muted-foreground`) y, si existe, una nota (14px `--muted-foreground`, margen superior 4px).

Colores por tipo de evento:

| Tipo       | Etiqueta              | Ícono         | Fondo círculo / texto        |
| ---------- | --------------------- | ------------- | ---------------------------- |
| created    | "Solicitud creada"    | file-plus     | `sky-500/15` · `sky-600/400` |
| edited     | "Solicitud editada"   | pencil        | `amber-500/15` · `amber-600/400` |
| approved   | "Solicitud aprobada"  | check         | `emerald-500/15` · `emerald-600/400` |
| rejected   | "Solicitud rechazada" | x             | `destructive/15` · `destructive` |
| cancelled  | "Solicitud cancelada" | undo          | `muted` · `muted-foreground` |

### 2.12 Diálogo (modal) y Sheet (panel lateral)

**Dialog (modal centrado):**
- Overlay: `position:fixed; inset:0; background: rgba(0,0,0,.5); z-index:50; animation: fade-in 150ms;`
- Contenido: centrado, `max-width: 448px` (`sm:max-w-md`), `width: ~90vw` en móvil, `background: var(--background)`, `border-radius: 14px`, `box-shadow: 0 10px 15px rgba(0,0,0,.1)`, `z-index:51`, entra con fade + `scale(.95→1)` en 200ms. Sale con la animación inversa (fade-out + scale) en 150ms.
- Header: título (mono si es folio) + descripción. Body: `ScrollArea` con `max-height: 55–60vh`. Footer (opcional): barra a sangre (`-mx-4 -mb-4`), borde superior, fondo `muted/50`, botones alineados a la derecha con gap 8px.

**Sheet (panel deslizante):**
- Escritorio: entra desde la **derecha**, `width: 100%` hasta `max-width: 448px`, alto completo, borde izquierdo, entra con slide `translateX(100%→0)` + fade 200ms. Sale con slide `translateX(0→100%)` + fade 150ms.
- Móvil (<640px): entra desde **abajo**, ocupa `width: 100%` (sin max-width), con slide `translateY(100%→0)` + fade 200ms. Sin border-radius. El overlay semitransparente se mantiene igual que en desktop.
- Header con título 18px/600 + descripción; body scrollable con padding 16px; footer con borde superior y botones a la derecha. Botón de cierre (X) arriba a la derecha.

### 2.13 Alertas y Toasts

**Alert (en línea, dentro de formularios/diálogos):** caja con `display:flex; gap:12px; padding:12px; border:1px solid; border-radius:8px; font-size:14px;` con ícono a la izquierda, título (peso 500) y descripción.
- `default`: fondo `muted/40`, borde `--border`.
- `destructive`: fondo `destructive/10`, borde `destructive/40`, texto `--destructive`.

**Toast (Sonner):** aparece **arriba a la derecha** (`position="top-right"`, `richColors`), se autocierra a los 4 segundos (comportamiento por defecto de Sonner). Colores por tipo: success (esmeralda), error (rojo/destructive), warning (ámbar), info (sky). z-index 60. En móvil, la posición sigue siendo arriba a la derecha (Sonner se adapta internamente).

### 2.14 Calendario de rango (`Calendar`, en Nueva solicitud)

Rejilla de 7 columnas (días de semana), locale español, cada día es un botón de 32px. Incluye navegación entre meses (flechas < >) para poder seleccionar rangos que crucen dos meses. Los días **anteriores a hoy** están deshabilitados (`disabled: { before: hoy }`). **Hoy** es seleccionable. Los días del rango seleccionado se resaltan con el color primario; los días que **exceden el saldo disponible** se pintan con fondo `--destructive` y texto blanco (`!bg-destructive !text-white`). Va dentro de un contenedor con borde y padding: `border:1px solid var(--border); border-radius:10px; padding:8px;`.

**Comportamiento al hacer clic con rango existente:** Al hacer clic en una nueva fecha cuando ya hay un rango completo (inicio + fin), la selección se reinicia: la nueva fecha se convierte en el nuevo inicio y se borra el fin.

**Resolución de conflicto de estilos:** Cuando un día es simultáneamente parte del rango seleccionado (inicio/fin/rango) **y** excede el saldo disponible, **prevalece el rojo de "excede saldo"** (`!bg-destructive !text-white`) sobre el color de rango. Esto se implementa mediante el modificador `exceeding` que se pasa al `DayButton` y anula los estilos del día seleccionado.

### 2.15 Tooltip

Tooltip flotante que aparece al hacer hover/foco sobre un elemento. Usado en el botón "Aprobar" deshabilitado por traslape con solicitud aprobada (ver §6.3).

| Propiedad        | Valor                                              |
| ---------------- | -------------------------------------------------- |
| Fondo            | `--foreground` (invertido: casi negro en claro, casi blanco en oscuro) |
| Texto            | `--background` (invertido: blanco en claro, casi negro en oscuro) |
| Tamaño fuente    | 12px (`text-xs`)                                   |
| Padding          | `6px 12px` (`px-3 py-1.5`)                         |
| Border-radius    | 8px (`rounded-md`)                                 |
| Sombra           | ninguna (usa z-index: 50 para superposición)        |
| Flecha (arrow)   | Cuadrado de 10px (`size-2.5`) rotado 45°, con `rounded-[2px]`, fondo `--foreground`. Se posiciona automáticamente según el lado (`side="top"` por defecto). |
| Gap con trigger  | 4px (`sideOffset=4`)                                |
| Animación        | Entrada: `fade-in + zoom-in 95%`; salida: `fade-out + zoom-out 95%` |
| Ancho máximo     | 288px (`max-w-xs`)                                  |

```css
.tooltip-content {
  z-index: 50;
  display: inline-flex; align-items: center;
  width: fit-content; max-width: 288px;
  gap: 6px; padding: 6px 12px;
  border-radius: 8px;
  font-size: 12px;
  background: var(--foreground);
  color: var(--background);
}
.tooltip-arrow {
  width: 10px; height: 10px;
  background: var(--foreground);
  transform: rotate(45deg);
  border-radius: 2px;
}
```

### 2.16 ToggleGroup (filtros de estado)

Grupo de botones tipo toggle usado en la bandeja de aprobaciones (§6.2) para filtrar por estado: "Pendientes", "Aprobadas", "Rechazadas", "Todas".

| Propiedad              | Valor                                                |
| ---------------------- | ---------------------------------------------------- |
| Contenedor             | `display:flex; flex-direction:row; border-radius:10px; width:fit-content; gap:2px` |
| Variante usada         | `outline` (cada ítem tiene `border: 1px solid var(--input)`) |
| Ítem — inactivo        | `background: transparent; color: var(--foreground)`  |
| Ítem — activo (`data-[state=on]`) | `background: var(--muted)` (en ambas variantes)  |
| Ítem — hover           | `background: var(--muted)`                           |
| Ítem — radio           | 8px (`rounded-md`) por separado, pero cuando `gap=0` se fusionan (el primero y último conservan `rounded-lg` en sus extremos) |
| Tamaño                 | `sm`: altura 28px, texto `0.8rem`, íconos 14px       |
| Scroll horizontal      | El contenedor lleva `overflow-x: auto; justify-content: flex-start` para desplazarse en móvil si hay muchos filtros |

```css
.toggle-group {
  display: flex; flex-direction: row;
  width: fit-content; gap: 2px;
  border-radius: 10px;
  overflow-x: auto; /* en móvil */
}
.toggle-item {
  display: inline-flex; align-items: center; justify-content: center;
  height: 28px; padding: 0 10px;
  border: 1px solid var(--input);
  border-radius: 8px;
  font-size: 0.8rem; font-weight: 500; white-space: nowrap;
  background: transparent; color: var(--foreground);
  transition: all 150ms; cursor: pointer;
}
.toggle-item:hover { background: var(--muted); }
.toggle-item[data-state="on"] { background: var(--muted); }
```

### 2.17 Barra de progreso (`Progress`)

Usada en el dashboard del empleado (§5.2) para visualizar el uso del saldo anual.

| Propiedad        | Valor                              |
| ---------------- | ---------------------------------- |
| Track (fondo)    | `height: 4px; border-radius: 9999px; background: var(--muted)` |
| Fill (relleno)   | `height: 100%; border-radius: 9999px; background: var(--primary); transition: all 150ms` |
| Estructura       | Track contiene al Indicator. El ancho del Indicator se controla con CSS `width: ${value}%`. |

```css
.progress-track {
  position: relative; height: 4px; width: 100%;
  border-radius: 9999px; background: var(--muted);
  overflow: hidden;
}
.progress-indicator {
  height: 100%; border-radius: 9999px;
  background: var(--primary);
  transition: all 150ms;
}
```

---

## 3. Estructura global (App Shell)

Presente en todas las vistas tras iniciar sesión.

- **Contenedor raíz:** `display:flex; flex-direction:column; min-height:100vh; background: var(--background);`
- **Header (barra superior):** `position: sticky; top:0; z-index:40; border-bottom:1px solid var(--border); background: rgba(fondo, .8); backdrop-filter: blur;`
  - Interior centrado en `max-width:1152px`, padding `12px 16px`, `display:flex; justify-content:space-between; align-items:center;`
  - **Izquierda (logo):** cuadro de 32px `rounded-lg` con fondo `--primary` y ícono calendario (`CalendarCheck`) en `--primary-foreground` (16px); al lado (oculto en móvil) el texto "**PermisosApp**" (14px/600) y debajo "Permisos y Vacaciones" (12px `--muted-foreground`).
  - **Derecha:** botón de tema (`ThemeToggle`, botón `outline icon` con luna/sol) + menú de usuario (`UserMenu`). El `ThemeToggle` muestra **luna** cuando está en modo claro (sugiere "cambiar a oscuro") y **sol** cuando está en modo oscuro (sugiere "cambiar a claro").
- **Main:** contenedor centrado `max-width:1152px`, padding `24px 16px`, `flex: 1`.

### 3.1 Menú de usuario (`UserMenu`)

- **Disparador:** botón `outline` compacto que muestra el avatar (sm), y en pantallas ≥640px el nombre (14px/500) + rol activo (12px `--muted-foreground`), con un ícono chevrons a la derecha.
- **Contenido (dropdown, ancho 256px, alineado a la derecha):**
  - Cabecera: avatar (default) + nombre + email (`--muted-foreground`, truncados).
  - Separador.
  - Si el usuario tiene **varios roles**: encabezado "Cambiar rol activo" (con ícono repeat) y un radio-group con los roles disponibles (Empleado / Jefe Directo / Recursos Humanos). Si solo tiene uno, se muestra su rol como badge secundario.
  - Separador + ítem destructivo "Cerrar sesión" (ícono logout).

Etiquetas de rol: `empleado → "Empleado"`, `jefe → "Jefe Directo"`, `rrhh → "Recursos Humanos"`.

---

## 4. Módulo: Login (`LoginScreen`)

Pantalla previa a la sesión. Layout centrado en una columna estrecha `max-width: 28rem (448px)`.

- **Estructura:** `<main>` a pantalla completa, columna centrada con padding `32px 16px`.
- **Cabecera superior:** logo (cuadro 36px `--primary` con ícono calendario 20px) + "PermisosApp"; a la derecha el `ThemeToggle`.
- **Bloque central (centrado verticalmente):**
  - Badge secundario "Demo interactivo".
  - H1 (24px/600, `text-balance`): "Gestión de Permisos y Vacaciones".
  - Párrafo `--muted-foreground` (14px): invitación a iniciar sesión.
  - **Tarjeta "Iniciar sesión":**
    - Campo "Usuario o correo" (input texto, placeholder `tu.correo@empresa.com`).
    - Campo "Contraseña" (input password con botón-ícono ghost a la derecha para mostrar/ocultar, íconos ojo/ojo-tachado). Si hay error, se muestra `FieldError` bajo el campo y los campos quedan `aria-invalid`.
    - Botón primario ancho completo "Iniciar sesión" (con ícono login a la izquierda).
    - **Caja de credenciales demo:** contenedor `border`, fondo `muted/40`, radio 10px, padding 12px. Texto de ayuda "contraseña: `demo123`" (mono). Lista de usuarios demo: cada fila muestra el usuario (botón mono subrayable que autorrellena el formulario al hacer clic) y el nombre a la derecha (`--muted-foreground`).
- **Footer:** texto centrado 12px `--muted-foreground`: "Demo funcional 100% en el cliente · datos en memoria (no se persisten)".
- Al iniciar sesión con éxito → toast de éxito. El botón mostrar/ocultar contraseña alterna entre `EyeIcon` (contraseña oculta → "Mostrar contraseña") y `EyeOffIcon` (contraseña visible → "Ocultar contraseña").
- Credenciales válidas: `ana.torres@empresa.com`, `carlos.ramirez@empresa.com`, `laura.mendez@empresa.com`, todas con contraseña `demo123`.
- Usuarios sin rol "empleado": Laura Méndez tiene solo rol `rrhh` (no puede cambiar). Carlos Ramírez tiene roles `jefe` + `empleado` (puede cambiar entre ambos).

---

## 5. Módulo: Empleado (`EmployeeView`)

Vista por defecto del rol Empleado. Columna con `gap: 24px`.

### 5.1 Encabezado de vista

Fila (columna en móvil) con `justify-content: space-between`:
- Izquierda: H1 "Hola, {primer nombre}" (20px/600) + subtítulo `--muted-foreground` (14px).
- Derecha: botón primario "**Crear solicitud**" (ícono calendar-plus) que abre el Sheet de nueva solicitud.

### 5.2 Dashboard (`EmployeeDashboard`)

**Fila de 3 StatCards** (grid: 1 col móvil → 3 cols en `sm`):
- "Saldo anual" — ícono calendar-days, sin acento (ícono en caja `--muted`).
- "Días consumidos" — ícono minus-circle, acento **ámbar** (`amber-500/15`, texto `amber-600/400`). Solo cuenta solicitudes aprobadas.
- "Saldo disponible" — ícono wallet, acento **esmeralda**.

**StatCard (anatomía):** tarjeta con contenido en fila (`gap:16px`): a la izquierda un cuadro de 40px `rounded-lg` (fondo `--muted` o color de acento) con ícono 20px; a la derecha, columna con etiqueta (14px `--muted-foreground`), valor grande (24px/600 tabular) y "hint" opcional (12px `--muted-foreground`).

**Colores del cuadro de ícono según acento:**

| Tipo          | Clase CSS                                        | Fondo                | Texto (ícono)             |
| ------------- | ------------------------------------------------ | -------------------- | ------------------------- |
| Sin acento    | `bg-muted text-foreground`                       | `var(--muted)`       | `var(--foreground)`       |
| Ámbar         | `bg-amber-500/15 text-amber-600 dark:text-amber-400` | `rgba(245,158,11,.15)` | `#d97706` / `#fbbf24` |
| Esmeralda     | `bg-emerald-500/15 text-emerald-600 dark:text-emerald-400` | `rgba(16,185,129,.15)` | `#059669` / `#34d399` |

Los mismos valores de acento se usan para el fondo del ícono de Timeline y para los badges de estado (ver §1.2 colores semánticos).

**Fila inferior (grid `lg:grid-cols-3`):**
- Tarjeta "Uso del saldo anual" (ocupa 2 columnas): muestra cifra `30px` "consumidos / total días", porcentaje a la derecha, una **barra de progreso** (`Progress`) y un texto de días disponibles restantes.
- Tarjeta "Resumen de solicitudes" (1 columna): por cada estado (pendiente, aprobada, rechazada, cancelada) una fila con el `StatusBadge` a la izquierda y el conteo (14px/500 tabular) a la derecha.

### 5.3 Mis solicitudes (`MyRequests`)

Tarjeta con:
- **Header:** título "Mis solicitudes" + a la derecha un `Select` (tamaño sm) para ordenar: "Más recientes" / "Más antiguas" / "Por estado".
- **Cuerpo:** si no hay solicitudes → `Empty` (ícono calendar-x, "Aún no tienes solicitudes"). Si hay → **tabla** dentro de contenedor con borde y radio 10px:
  - Columnas: **ID** (mono 12px), **Fechas** (`--muted-foreground`), **Días** (derecha, tabular), **Estado** (`StatusBadge`), **Creada** (fecha, `--muted-foreground`), **Acciones** (derecha).
  - Fila clicable (abre el diálogo de detalle). En **Acciones**: botón-ícono ghost "Ver" (ojo); y si la solicitud está **pendiente**, además "Editar" (lápiz) y "Cancelar" (x-circle). El clic en la celda de acciones no propaga al detalle.
  - Debajo, `TablePagination` (8 por página).
- **Diálogo de detalle** (`RequestDetailDialog`): muestra folio (mono) + `StatusBadge`, filas etiqueta/valor (Fechas, Días solicitados, Motivo, Comentario del jefe si existe), separador e **Historial** (timeline). Si la solicitud está pendiente, footer con botones "Editar" (outline) y "Cancelar solicitud" (destructive).
- **Confirmación de cancelación** (`AlertDialog`): "¿Confirma cancelar esta solicitud?" con botones "Volver" y "Sí, cancelar" (destructive).

### 5.4 Nueva/Editar solicitud (`NewRequestSheet`)

Panel lateral (Sheet) que sirve tanto para crear como para editar.
- **En edición:** se precargan fechas, motivo, y el texto del botón cambia a "Guardar cambios". Se valida contra el saldo disponible (restando solo solicitudes aprobadas, ignorando la solicitud que se está editando).
- Header: título "Nueva solicitud" / "Editar solicitud" + descripción.
- Cuerpo (scrollable):
  - Campo "Rango de fechas" → **Calendario** en modo rango dentro de una caja con borde. Ayuda: "Haz clic en el primer día y luego en el último…".
  - **Resumen de selección** (aparece al elegir fechas): caja con "Has seleccionado **N días** · Saldo disponible: **M días**". Si excede el saldo, la caja pasa a estilo destructivo (`destructive/10`, borde `destructive/40`, texto destructive) y añade "**X días** exceden tu saldo"; además esos días se pintan en rojo en el calendario.
  - Campo "Motivo" → `Textarea` (sin límite de caracteres, placeholder: `"Escribe brevemente el motivo de tu solicitud"`).
  - Si hay error de validación en vivo → `Alert` destructivo "No se puede enviar todavía".
- Footer: botón "Cancelar" (outline, cierra) + botón primario "Enviar solicitud" / "Guardar cambios" (ícono send, cambia según modo), deshabilitado hasta que haya fecha, motivo y no haya error.
- Al enviar con éxito → toast de éxito con descripción "N día(s) · Pendiente de aprobación".

---

## 6. Módulo: Jefe / Aprobador (`ManagerView`)

El aprobador gestiona **todas** las solicitudes de la organización. Columna con `gap: 24px`.

### 6.1 Fila de StatCards (grid 1 → 2 en `sm` → 4 en `lg`)

- "Pendientes" — ícono clock, acento **ámbar**.
- "Aprobadas" — ícono check-circle, acento **esmeralda**.
- "Colaboradores" — ícono users, sin acento.
- "Días aprobados" — ícono calendar-range, sin acento (total acumulado).

### 6.2 Bandeja de aprobaciones (`ManagerInbox`)

- **Encabezado:** H1 "Bandeja de aprobaciones" (24px/600) + subtítulo.
- **Tarjeta:**
  - Header: título "Solicitudes de vacaciones" + descripción "N pendientes de revisión"; a la derecha un **buscador** (input con ícono lupa a la izquierda, placeholder "Buscar por nombre o folio", ancho 256px en `sm`).
  - Debajo, un **ToggleGroup** (variante outline, scroll horizontal en móvil) con filtros: "Pendientes (n)", "Aprobadas (n)", "Rechazadas (n)", "Todas (n)". Por defecto "Pendientes".
  - **Tabla** (con `overflow-x:auto`):
    - Columnas: **Colaborador** (avatar sm + nombre/500 + rol en 12px `--muted-foreground`), **Periodo** (rango de fechas; si hay traslape con una ausencia aprobada aparece un ícono triángulo ámbar; si es pendiente, una segunda línea "Inicia en N días" / "Inicia hoy"), **Días** (derecha, tabular), **Estado** (`StatusBadge`), **Acción** (botón: "Revisar" primario si pendiente, "Ver" outline si no).
  - Vacío → `Empty` (ícono inbox, "Sin solicitudes").
  - Orden: pendientes primero, luego por fecha de creación descendente.

### 6.3 Detalle de revisión (`ManagerRequestDetail`)

Diálogo (modal) `max-width: 448px`:
- Header: folio (mono) + `StatusBadge` + descripción "Solicitud de vacaciones".
- Cuerpo (scroll, `max-height: 55vh`):
  - Caja con avatar + nombre + email del empleado.
  - **Tres mini-tarjetas** (grid 3 col): "Disponible", "Solicita", "Post-aprob." (esta última en rojo si queda negativo). Cada una: borde, radio 10px, centro, etiqueta 12px `--muted-foreground` + cifra 18px/600 tabular.
  - **Alertas de traslape:** si se cruza con una **aprobada** → Alert destructivo "Traslape con una solicitud aprobada" (bloquea aprobar). Si se cruza con otra **pendiente** → Alert normal informativo.
  - Filas etiqueta/valor: Fechas, Motivo, Comentario (si existe).
  - Si se está rechazando: campo "Comentario (obligatorio)" (`Textarea`, máx. 500) con contador o error. La validación muestra **ambos**: error inline (`FieldError`) y toast al hacer submit sin comentario.
  - Separador + **Historial** (timeline los actores son nombres de personas, ej. "Ana Torres", no roles).
- Footer (solo si pendiente), barra a sangre con fondo `muted/50`:
  - Modo normal: "Rechazar" (outline) + "Aprobar" (primario). Si hay traslape aprobado, "Aprobar" se deshabilita y muestra un Tooltip explicando por qué. El Tooltip es un componente JS (shadcn `TooltipTrigger` + `TooltipContent`), no `title` nativo.
  - Modo rechazo: reemplaza el footer inline (no abre sub-diálogo): "Volver" (outline) + "Confirmar rechazo" (destructive, requiere comentario).
- Acciones muestran toasts de éxito/error.

---

## 7. Módulo: Recursos Humanos (`HRView`)

Vista de consulta e informes. Columna con `gap: 24px`.

- **Encabezado:** H1 "Panel de Recursos Humanos" (24px/600) + subtítulo.
- **Fila de StatCards** (1 → 2 en `sm` → 4 en `lg`):
  - "Total solicitudes" (file-stack), "Pendientes" (clock, acento ámbar), "Aprobadas" (check-circle, acento esmeralda), "Empleados" (users).
- **Tarjeta "Historial de solicitudes":**
  - Header: título + descripción "N resultados (con los filtros aplicados)"; a la derecha botón `outline sm` "**Exportar CSV**" (ícono download), deshabilitado si no hay resultados. La exportación genera un CSV con BOM y columnas: Folio, Empleado, Correo, Inicio, Fin, Días, Estado, Motivo, Decidido por.
  - **Fila de filtros** (grid: 1 → 2 en `sm` → 3 en `lg`): buscador con ícono lupa ("Buscar…"), `Select` de empleado ("Todos los empleados" + lista), `Select` de estado ("Todos los estados" + Pendiente/Aprobada/Rechazada/Cancelada).
  - **Tabla** (`overflow-x:auto`, borde, radio 10px). Columnas: **Colaborador** (avatar sm + nombre + email), **Folio** (mono 12px), **Periodo**, **Días** (derecha tabular), **Estado** (`StatusBadge`), **Creada** (fecha). Fila clicable → diálogo de detalle (con `showEmployee`, muestra el nombre del empleado en la descripción).
  - `TablePagination` (10 por página). Vacío → `Empty` (ícono file-search, "Sin resultados").

---

## 8. Cómo recrear el sistema en HTML/CSS (resumen de reglas)

1. **Define primero las variables CSS** en `:root` y `.dark` (tablas §1.2). Usa `color-scheme` y una clase `.dark` en `<html>` conmutada por el toggle de tema. **Detección inicial:** respeta `prefers-color-scheme` del SO al cargar, pero la preferencia manual del usuario (guardada en localStorage) tiene prioridad.
2. **No inventes colores.** El primario es gris oscuro (claro) / gris claro (oscuro). Los únicos matices son los estados y los avatares.
3. **Tarjetas = anillo, no sombra.** `box-shadow: 0 0 0 1px rgba(37,37,37,.10)`, radio 14px, padding 16px.
4. **Botones e inputs son compactos:** 32px de alto, radio 10px, texto 14px. El botón destructivo es **rojo suave** (fondo al 10%), no rojo sólido.
5. **Badges de estado** son píldoras con fondo del color al 15% y texto del color intenso.
6. **Layout:** header sticky con logo a la izquierda y (tema + usuario) a la derecha; contenido centrado a máx. 1152px; secciones separadas con `gap: 24px`; grids de stats responsivos 1→2→4.
7. **Tipografía:** Geist Sans para todo, Geist Mono solo para folios/IDs. Títulos de página 24px, cifras grandes con `tabular-nums`.
8. **Estados de estado (español):** Pendiente=ámbar, Aprobada=esmeralda, Rechazada=rojo, Cancelada=gris.
9. **Interacción:** foco visible con anillo de 3px (`--ring` al 50%); transiciones de 150ms; modales con fade+scale, sheets con slide (derecha en escritorio, abajo en móvil).
10. **Reutiliza patrones:** StatCard, StatusBadge, Timeline, tabla + paginación y el diálogo de detalle se repiten en los tres módulos; constrúyelos una vez y reúsalos.
11. **Formato de fecha:** siempre `10 mar 2025` (locale `es-ES`, `day:2-digit, month:short, year:numeric`). Fechas con hora: `10 mar 2025, 14:30`.
12. **Grids de stats responsivos:** Empleado usa 1 col → 3 cols (`sm`). Jefe/RRHH usan 1 col → 2 cols (`sm`) → 4 cols (`lg`).

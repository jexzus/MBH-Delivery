Jam Burgers 🍔 – App de Delivery (MAUI Blazor)

Aplicación para gestión de pedidos y catálogo, con roles Administrador y Cliente. Frontend en .NET MAUI Blazor; comunicación con backend vía servicios (MauiBlazorDelivery.Services).
<img width="1005" height="543" alt="image" src="https://github.com/user-attachments/assets/9c57522c-5e0e-4e46-b804-69e3cb464508" />
#
*** Tabla de contenidos ***

•	Características.

•	Roles y permisos

•	Pantallas principales

•	Guía de uso – Administrador

•	Guía de uso – Cliente

•	Instalación y ejecución

•	Configuración

•	Modelo de datos (detectado)

•	Impresión y PDF

•	Resolución de problemas

•	Hoja de ruta / mejoras futuras

•	Contribuir

•	Licencia
#
*** Características ***

•	✅ Autenticación con roles (Admin / Cliente)

•	✅ Catálogo: crear, editar, eliminar, buscar, con imagen opcional

•	✅ Pedidos: filtro por estado (Pendiente / En preparación / En reparto / Entregado), búsqueda, detalle e impresión

•	✅ Administradores: alta con confirmación, edición inline, eliminación con confirmación

•	✅ UI consistente: botones gradientes, chips de estado, dropdowns, toasts, modales reutilizables

•	✅ Clientes: registro con validaciones y vista de mis pedidos

•	🧩 Servicios MauiBlazorDelivery.Services.* para separar UI de acceso a API
#
| Rol                     | Puede hacer                                                                                                  |
| ----------------------- | ------------------------------------------------------------------------------------------------------------ |
| 👨‍🍳 **Administrador** | Gestionar **catálogo**, **pedidos** (ver/cambiar estado/imprimir), **administradores** (ABM), cerrar sesión. |
| 🛍️ **Cliente**         | **Registrarse** e iniciar sesión, ver **catálogo** y **mis pedidos** con estados.                            |
#
*** Pantallas principales

/login → Detecta rol y redirige a /paneladmin o /catalogo.

/registro → Alta de cliente con validaciones.

/gestioncatalogo → Listado + búsqueda + “Nuevo producto” + editar/eliminar.

/nuevoproducto → Formulario con imagen opcional (JSON o multipart/form-data).

/gestionadministradores → Alta/edición inline/eliminación con confirmación.

/gestionpedidos → Filtros por estado, búsqueda, detalle e impresión.

/estadopedido (cliente) → Historial personal con chips de estado.
#
*** Guía de uso – Administrador

Ingresá en /login con usuario admin.

Desde el Panel:

Catálogo: crear/editar/eliminar productos. Soporte de imagen y previsualización.

Pedidos: filtrar por estado, buscar por Nro/Cliente/Teléfono/Domicilio, ver detalle (items, cantidades, totales) y cambiar estado (modal de confirmación).

Imprimir: lista y detalle listos para PDF.

Administradores: crear (modal), editar inline, eliminar (modal).

Toasts informan éxito/errores.
#
*** Guía de uso – Cliente

Registro en /registro (usuario, contraseña alfanumérica, nombre/apellido solo letras, teléfono numérico, domicilio).

Login en /login → se redirige a /catalogo.

Mis pedidos en /estadopedido con estados: Pendiente / En preparación / En reparto / Entregado.
#
*** Instalación y ejecución

Requisitos: .NET 8, SDKs MAUI (si compilas móvil), backend accesible.

git clone <repo>
cd <repo>
dotnet restore
dotnet build
# Ejecución típica MAUI (ajustar proyecto si aplica)
dotnet run --project src/YourMauiBlazorProject


También podés lanzar desde Visual Studio seleccionando target (Android/iOS/Windows).
#
*** Configuración *** 

La UI usa MauiBlazorDelivery.Services (AuthService, ProductoService, PedidosService, AdminService).
Configurar en un punto central:

Base URL del backend (dev/prod) para HttpClient

TimeOut y manejo de errores

Persistencia de sesión (según tu implementación)

Si compartís Program.cs/MauiProgram.cs y appsettings*.json, se agrega aquí la config exacta.
#
*** Modelo de datos (detectado) ***

Deducido desde las pages. Ajustar si tus DTOs difieren.

ProductoDto: Id, Nombre/NombreProducto, Descripcion, Precio, ImagenUrl?

AdminDto: Id, NombreUsuario, Contraseña

PedidoDto: NumPedido, FechaPedido, EstadoPedido, MontoTotal, Cliente { Nombre, Apellido, NumTelefono, Domicilio }, DetallePedidos[] { Producto?.NombreProducto, Cantidad, PrecioUnitario, Subtotal }

RegistroClienteDto: NombreUsuario, Contrasena, Nombre, Apellido, NumTelefono, Domicilio
#
*** Impresión y PDF ***

Se usa mauiPrintHelper.printWithClass (JS) para imprimir:

Lista: #print-list

Detalle: #print-detalle

El CSS incluye @media print para ocultar todo salvo esas secciones.
#
*** Resolución de problemas ***

No carga catálogo/pedidos → revisar Base URL, CORS y que la API esté online.

Imágenes rotas → validar ImagenUrl o fallback /images/no-image.png.

Impresión no responde → asegurar que el helper JS exista y las IDs estén en el DOM.

Cambio de estado falla → revisar respuesta de PedidosService.CambiarEstadoAsync y logs del backend.
#
*** Hoja de ruta / mejoras futuras ***

• Migrar modales a Bootstrap 5 (clases modal, modal-dialog, modal-content, btn-close).

• Verificación por email en registro (token, expiración, reenvío).

• Integrar Mercado Pago (Checkout/Payments API, webhooks y sincronización con estado del pedido).

• Notificaciones (email/SMS/push) por cambio de estado.

• Auditoría y logs (quién cambió qué y cuándo).

• Seguridad: hash de contraseñas de administradores, políticas de complejidad y lockout.
#
*** Contribuir ***

Las PRs son bienvenidas. Abrí un issue si encontrás un bug o querés proponer mejoras.
#
*** Licencia ***

Este proyecto se distribuye bajo licencia MIT.

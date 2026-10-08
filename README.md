# >_ Commit Store

Sistema de gestión de productos de una tienda de accesorios para computadoras y escritorios. Proyecto del primer parcial de Laboratorio de Programación 3, desarrollado por **Julieta Landra**.

## Funcionalidades

- Alta, consulta, modificación y baja de productos.
- Consulta con JOIN entre productos y categorías y filtros por nombre/categoría.
- Modificación del producto y su categoría, con detección de cambios.
- Baja confirmada por ID, conservando las categorías.
- Validaciones, mensajes y navegación entre páginas.
- Interfaz oscura con CSS externo.

## Tecnologías y estructura

- C# / .NET 8 / ASP.NET Core Razor Pages.
- SQL Server LocalDB y Microsoft.Data.SqlClient.
- HTML y CSS.

## Base de datos

| Tabla | Campos |
|---|---|
| categorías | idCategoria (PK, identity), descripción |
| productos | idProducto (PK, identity), nombre, precio decimal(18,2), categoría (FK) |

`productos.[categoría]` referencia `[categorías].idCategoria`. Los códigos CS-001 son la presentación del ID numérico, no claves diferentes. No se renumeran después de una baja.

## Ejecución inicial

1. Abrir `TiendaOnline-LDP3.sln` en Visual Studio 2022.
2. Restaurar paquetes NuGet.
3. En una instalación nueva, ejecutar `Scripts/01_commit_store.sql` en `(localdb)\MSSQLLocalDB`.
4. Revisar `ConnectionStrings:CommitStore` en appsettings.json.
5. Ejecutar con F5.

Si la base ya existe y funciona, no hay que recrearla. No se utilizan migraciones de Entity Framework.

## Navegación

El inicio está en `/`. Los formularios están en `/Gestion/Alta`, `/Gestion/Consulta`, `/Gestion/Modificacion` y `/Gestion/Baja`.

En Modificación y Baja: seleccionar el producto, presionar Buscar y después guardar o eliminar. El menú permanece visible y no hay carga automática al cambiar el selector.

Se aceptan precios como `85000`, `85000,00` y `85000.00`, sin separador de miles ni símbolo de moneda. El precio debe ser positivo y tener hasta dos decimales.

## Alcance y revisión

La implementación usa Razor Pages, con ADO.NET como equivalente de SqlDataSource. La tienda pública queda como una ampliación futura.

## Información de la materia

- **Profesor**: Christian Mansilla
- **6to Semetre 2026**
- **Instituto Superior Santo Domingo**

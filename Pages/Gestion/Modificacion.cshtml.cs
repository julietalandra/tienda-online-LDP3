using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Globalization;
using TiendaOnline_LDP3.Modelos;

namespace TiendaOnline_LDP3.Pages.Gestion;

public class ModificacionModel : PageModel
{
    private readonly IConfiguration _config;
    public ModificacionModel(IConfiguration config) { _config = config; }
    [BindProperty(SupportsGet = true)] public int? Id { get; set; }
    [BindProperty] public int? IdCargado { get; set; }
    [BindProperty] public string? Accion { get; set; }
    public List<SelectListItem> Productos { get; set; } = new();
    public Producto? Seleccionado { get; set; }

    [BindProperty] public string? Nombre { get; set; }
    [BindProperty] public string? Precio { get; set; }
    [BindProperty] public int? IdCategoria { get; set; }
    public List<SelectListItem> Categorias { get; set; } = new();
    public Aviso? Aviso { get; set; }

    public void OnGet()
    {
        CargarCategorias();
        CargarProductos();
        if (Id.HasValue) Buscar();
    }

    // Los dos botones envían Accion, como en el ejemplo de la guía 5.
    public void OnPost()
    {
        CargarCategorias();
        CargarProductos();
        if (Accion == "Buscar") Buscar();
        else if (Accion == "Modificar") Modificar();
    }

    private void Buscar()
    {
        CargarSeleccionado();
        if (Seleccionado == null)
        {
            Aviso = new Aviso("Producto no encontrado", "Seleccioná un producto y presioná Buscar.", "error");
            return;
        }
        Nombre = Seleccionado.Nombre;
        Precio = Seleccionado.Precio.ToString("F2", CultureInfo.GetCultureInfo("es-AR"));
        IdCategoria = Seleccionado.IdCategoria;
        IdCargado = Id;
        // Permite que asp-for muestre los valores recién leídos de la base.
        ModelState.Clear();
    }

    private void Modificar()
    {
        if (Id is not > 0 || Id != IdCargado)
        {
            Aviso = new Aviso("Primero buscá el producto", "Presioná Buscar después de cambiar la selección.", "error");
            return;
        }
        CargarSeleccionado();
        if (Seleccionado == null)
        {
            Aviso = new Aviso("Producto no encontrado", "Seleccioná otro producto.", "error");
            return;
        }
        if (!ValidarDatos(out decimal precio)) return;
        if (Nombre!.Trim() == Seleccionado.Nombre && precio == Seleccionado.Precio
            && IdCategoria == Seleccionado.IdCategoria)
        {
            Aviso = new Aviso("No modificaste ningún dato", "Editá al menos un campo para guardar cambios.", "info");
            return;
        }
        string cadena = _config.GetConnectionString("CommitStore")!;
        using (SqlConnection conexion = new SqlConnection(cadena))
        {
            conexion.Open();
            string sql = @"update productos set nombre = @nombre, precio = @precio,
                                                 [categoría] = @categoria
                            where idProducto = @id";
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@nombre", Nombre.Trim());
                comando.Parameters.AddWithValue("@precio", precio);
                comando.Parameters.AddWithValue("@categoria", IdCategoria!.Value);
                comando.Parameters.AddWithValue("@id", Id!.Value);
                int cantidad = comando.ExecuteNonQuery();
                Aviso = cantidad > 0
                    ? new Aviso("Producto actualizado", "Los cambios se guardaron correctamente.")
                    : new Aviso("El producto ya no existe", "Seleccioná otro producto.", "error");
            }
        }
        CargarProductos();
        CargarSeleccionado();
    }

    private bool ValidarDatos(out decimal precio)
    {
        if (string.IsNullOrWhiteSpace(Nombre))
            ModelState.AddModelError(nameof(Nombre), "Ingresá el nombre del producto.");
        else if (Nombre.Length > 100)
            ModelState.AddModelError(nameof(Nombre), "El nombre admite hasta 100 caracteres.");

        string textoPrecio = (Precio ?? "").Trim().Replace(',', '.');
        bool numeroValido = decimal.TryParse(textoPrecio,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out precio);
        if (!numeroValido || precio <= 0 || precio > 9999999999999999.99m
            || decimal.Round(precio, 2) != precio)
            ModelState.AddModelError(nameof(Precio), "Ingresá un precio mayor que cero, con hasta dos decimales y sin separador de miles.");

        // Las opciones vienen de la base, también se comprueba lo enviado.
        if (!IdCategoria.HasValue || !Categorias.Any(c => c.Value == IdCategoria.Value.ToString()))
            ModelState.AddModelError(nameof(IdCategoria), "Seleccioná una categoría válida.");

        if (!ModelState.IsValid)
            Aviso = new Aviso("Revisá los campos indicados", Tipo: "error");
        return ModelState.IsValid;
    }

    private void CargarCategorias()
    {
        Categorias.Clear();
        string cadena = _config.GetConnectionString("CommitStore")!;
        using (SqlConnection conexion = new SqlConnection(cadena))
        {
            conexion.Open();
            string sql = "select idCategoria, [descripción] from [categorías] order by idCategoria";
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            using (SqlDataReader registros = comando.ExecuteReader())
            {
                while (registros.Read())
                {
                    Categorias.Add(new SelectListItem(
                        registros["descripción"].ToString(),
                        registros["idCategoria"].ToString()));
                }
            }
        }
    }

    private void CargarProductos()
    {
        Productos.Clear();
        string cadena = _config.GetConnectionString("CommitStore")!;
        using (SqlConnection conexion = new SqlConnection(cadena))
        {
            conexion.Open();
            using (SqlCommand comando = new SqlCommand(
                "select idProducto, nombre from productos order by idProducto", conexion))
            using (SqlDataReader registros = comando.ExecuteReader())
            {
                while (registros.Read())
                {
                    int id = Convert.ToInt32(registros["idProducto"]);
                    Productos.Add(new SelectListItem(
                        $"CS-{id:D3} · {registros["nombre"]}", id.ToString()));
                }
            }
        }
    }

    private void CargarSeleccionado()
    {
        Seleccionado = null;
        if (Id is not > 0) return;
        string cadena = _config.GetConnectionString("CommitStore")!;
        using (SqlConnection conexion = new SqlConnection(cadena))
        {
            conexion.Open();
            string sql = @"select p.idProducto, p.nombre, p.precio, p.[categoría],
                                  c.[descripción] as descripcionCategoria
                             from productos as p
                             join [categorías] as c on p.[categoría] = c.idCategoria
                            where p.idProducto = @id";
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@id", Id.Value);
                using (SqlDataReader registro = comando.ExecuteReader())
                {
                    if (registro.Read())
                    {
                        Seleccionado = new Producto
                        {
                            IdProducto = Convert.ToInt32(registro["idProducto"]),
                            Nombre = registro["nombre"].ToString()!,
                            Precio = Convert.ToDecimal(registro["precio"]),
                            IdCategoria = Convert.ToInt32(registro["categoría"]),
                            CategoriaDescripcion = registro["descripcionCategoria"].ToString()!
                        };
                    }
                }
            }
        }
    }
}

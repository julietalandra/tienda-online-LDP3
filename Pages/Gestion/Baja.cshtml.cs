using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Globalization;
using TiendaOnline_LDP3.Modelos;

namespace TiendaOnline_LDP3.Pages.Gestion;

public class BajaModel : PageModel
{
    private readonly IConfiguration _config;
    public BajaModel(IConfiguration config) { _config = config; }
    [BindProperty(SupportsGet = true)] public int? Id { get; set; }
    [BindProperty] public int? IdCargado { get; set; }
    [BindProperty] public string? Accion { get; set; }
    public List<SelectListItem> Productos { get; set; } = new();
    public Producto? Seleccionado { get; set; }
    public Aviso? Aviso { get; set; }
    public bool Eliminado { get; set; }

    public void OnGet()
    {
        CargarProductos();
        if (TempData["Eliminado"] is string detalle)
        { Eliminado = true; Aviso = new Aviso("Producto eliminado correctamente", detalle); }
        else if (Id.HasValue) Buscar();
    }

    public IActionResult OnPost()
    {
        CargarProductos();
        if (Accion == "Buscar") Buscar();
        else if (Accion == "Eliminar")
        {
            if (!ModelState.IsValid || Id is not > 0 || Id != IdCargado)
            {
                Aviso = new Aviso("Primero buscá el producto", "Presioná Buscar después de cambiar la selección.", "error");
                return Page();
            }
            CargarSeleccionado();
            if (Seleccionado == null)
            {
                Aviso = new Aviso("Producto no encontrado", "Seleccioná otro producto.", "error");
                return Page();
            }
            string cadena = _config.GetConnectionString("CommitStore")!;
            using (SqlConnection conexion = new SqlConnection(cadena))
            {
                conexion.Open();
                // Se elimina únicamente la fila seleccionada de productos.
                string sql = "delete from productos where idProducto = @id";
                using (SqlCommand comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@id", Id.Value);
                    int cantidad = comando.ExecuteNonQuery();
                    if (cantidad > 0)
                    {
                        TempData["Eliminado"] = $"{Seleccionado.Nombre} ({Seleccionado.Codigo}) ya no está en el catálogo.";
                        return RedirectToPage();
                    }
                    Aviso = new Aviso("El producto ya no existe", "No se eliminó ningún otro registro.", "info");
                    Seleccionado = null;
                }
            }
        }
        return Page();
    }

    private void Buscar()
    {
        CargarSeleccionado();
        if (Seleccionado == null)
        { Aviso = new Aviso("Producto no encontrado", "Seleccioná un producto y presioná Buscar.", "error"); return; }
        IdCargado = Id;
        ModelState.Clear();
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

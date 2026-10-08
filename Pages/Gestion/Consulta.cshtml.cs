using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Globalization;
using TiendaOnline_LDP3.Modelos;

namespace TiendaOnline_LDP3.Pages.Gestion;

public class ConsultaModel : PageModel
{
    private readonly IConfiguration _config;
    public ConsultaModel(IConfiguration config) { _config = config; }
    [BindProperty] public string? Nombre { get; set; }
    [BindProperty] public int? Categoria { get; set; }
    public List<SelectListItem> Categorias { get; set; } = new();
    public List<Producto> Productos { get; set; } = new();

    public void OnGet()
    {
        CargarCategorias();
        Consultar();
    }

    public void OnPost()
    {
        CargarCategorias();
        if (Nombre?.Length > 100)
            ModelState.AddModelError(nameof(Nombre), "La búsqueda admite hasta 100 caracteres.");
        if (Categoria.HasValue && !Categorias.Any(c => c.Value == Categoria.Value.ToString()))
            ModelState.AddModelError(nameof(Categoria), "Elegí una categoría válida.");
        if (ModelState.IsValid) Consultar();
    }

    private void Consultar()
    {
        Productos.Clear();
        string cadena = _config.GetConnectionString("CommitStore")!;
        using (SqlConnection conexion = new SqlConnection(cadena))
        {
            conexion.Open();
            string sql = @"select p.idProducto, p.nombre, p.precio, p.[categoría],
                                  c.[descripción] as descripcionCategoria
                             from productos as p
                             join [categorías] as c on p.[categoría] = c.idCategoria
                            where p.nombre like @nombre
                              and (@categoria = 0 or p.[categoría] = @categoria)
                            order by p.idProducto";
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@nombre", "%" + (Nombre ?? "").Trim() + "%");
                comando.Parameters.AddWithValue("@categoria", Categoria ?? 0);
                using (SqlDataReader registros = comando.ExecuteReader())
                {
                    while (registros.Read())
                    {
                        Productos.Add(new Producto
                        {
                            IdProducto = Convert.ToInt32(registros["idProducto"]),
                            Nombre = registros["nombre"].ToString()!,
                            Precio = Convert.ToDecimal(registros["precio"]),
                            IdCategoria = Convert.ToInt32(registros["categoría"]),
                            CategoriaDescripcion = registros["descripcionCategoria"].ToString()!
                        });
                    }
                }
            }
        }
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
}

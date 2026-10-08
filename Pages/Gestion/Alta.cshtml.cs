using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Globalization;
using TiendaOnline_LDP3.Modelos;

namespace TiendaOnline_LDP3.Pages.Gestion;

public class AltaModel : PageModel
{
    private readonly IConfiguration _config;
    public AltaModel(IConfiguration config) { _config = config; }

    [BindProperty] public string? Nombre { get; set; }
    [BindProperty] public string? Precio { get; set; }
    [BindProperty] public int? IdCategoria { get; set; }
    public List<SelectListItem> Categorias { get; set; } = new();
    public Aviso? Aviso { get; set; }

    public bool Registrado { get; set; }
    public string? NombreRegistrado { get; set; }
    public string? PrecioRegistrado { get; set; }
    public string? CategoriaRegistrada { get; set; }

    public void OnGet()
    {
        CargarCategorias();
        if (TempData["AltaNombre"] is string nombre)
        {
            Registrado = true;
            NombreRegistrado = nombre;
            PrecioRegistrado = TempData["AltaPrecio"]?.ToString();
            CategoriaRegistrada = TempData["AltaCategoria"]?.ToString();
            Aviso = new Aviso("Producto registrado correctamente", "El producto ya está disponible en el catálogo.");
        }
    }

    public IActionResult OnPost()
    {
        // Igual que CargarRubros en la guía: llamar también después del POST.
        CargarCategorias();
        if (!ValidarDatos(out decimal precio)) return Page();
        string cadena = _config.GetConnectionString("CommitStore")!;
        using (SqlConnection conexion = new SqlConnection(cadena))
        {
            conexion.Open();
            string sql = "insert into productos(nombre, precio, [categoría]) values(@nombre, @precio, @categoria)";
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@nombre", Nombre!.Trim());
                comando.Parameters.AddWithValue("@precio", precio);
                comando.Parameters.AddWithValue("@categoria", IdCategoria!.Value);
                comando.ExecuteNonQuery();
            }
        }
        // Se conserva la redirección para no duplicar el alta al refrescar.
        TempData["AltaNombre"] = Nombre!.Trim();
        TempData["AltaPrecio"] = precio.ToString("C2", CultureInfo.GetCultureInfo("es-AR"));
        TempData["AltaCategoria"] = Categorias.First(c => c.Value == IdCategoria!.Value.ToString()).Text;
        return RedirectToPage();
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
}

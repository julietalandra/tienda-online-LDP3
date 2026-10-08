using System.Globalization;

namespace TiendaOnline_LDP3.Modelos;

public class Producto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; }
    public int IdCategoria { get; set; }
    public string CategoriaDescripcion { get; set; } = "";
    public string Codigo => $"CS-{IdProducto:D3}";
    public string PrecioMoneda => Precio.ToString("C2", CultureInfo.GetCultureInfo("es-AR"));
}


using Microsoft.AspNetCore.Mvc;
using basic.Models;
using MySqlConnector;
namespace basic.Controllers;

public class ProductoController: Controller
{
    private readonly string _connectionString;
    public ProductoController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MariaDBConnection");
    }

    public IActionResult RegistroProducto()
    {
        return View();
    }

    public async Task<IActionResult> ListarProducto()
    {
        var ListaProductos = new List<Producto>();
        string query = "SELECT id_producto, nombre, descripcion, precio FROM producto ORDER BY id_producto DESC;";

        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new MySqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var product = new Producto
                        {
                            id_producto = reader.GetInt32("id_producto"),
                            nombre = reader.GetString("nombre"),
                            descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) 
                                ? string.Empty 
                                : reader.GetString("descripcion"),
                            precio = reader.GetDecimal("precio")
                        };
                        ListaProductos.Add(product);
                    }
                }
            }
        }
        return View(ListaProductos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarProducto(Producto producto)
    {
        if (!ModelState.IsValid)
        {
            return View("RegistroProducto", producto);
        }

        string query = "INSERT INTO producto (nombre, descripcion, precio) VALUES (@nombre, @descripcion, @precio);";

        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.Add("@nombre", MySqlDbType.VarChar, 100).Value = producto.nombre;
                command.Parameters.Add("@descripcion", MySqlDbType.VarChar, 500).Value = producto.descripcion;
                command.Parameters.Add("@precio", MySqlDbType.Decimal).Value = producto.precio;

                await command.ExecuteNonQueryAsync();
            }
        }
        return RedirectToAction("ListarProducto", "Producto");
    }
}
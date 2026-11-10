using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RestApi.Entidades;

namespace RestApi.Datos;

public class PlatoRepositorio(IConfiguration config)
{
    private readonly string _cn = config.GetConnectionString("Restaurante")!;

    // El JOIN trae el nombre de la categoría en la misma consulta
    private const string SelectConCategoria = @"
    SELECT p.Id, p.Nombre, p.Descripcion, p.Precio, p.CategoriaId, p.Disponible, c.Nombre
    FROM Platos p
    INNER JOIN Categorias c ON c.Id = p.CategoriaId";

    public Task<List<Plato>> ObtenerTodosAsync() =>
        LeerAsync(SelectConCategoria + " WHERE p.Disponible = 1 ORDER BY c.Nombre, p.Nombre");

    public async Task<Plato?> ObtenerPorIdAsync(int id) =>
        (await LeerAsync(SelectConCategoria + " WHERE p.Id = @id", ("@id", id))).FirstOrDefault();

    public Task<List<Plato>> ObtenerPorCategoriaAsync(int categoriaId) =>
        LeerAsync(SelectConCategoria + " WHERE p.CategoriaId = @cat AND p.Disponible = 1 ORDER BY p.Nombre",
            ("@cat", categoriaId));

    public async Task<int> AgregarAsync(Plato p)
    {
        await using var cn = new SqlConnection(_cn);
        await cn.OpenAsync();

        // Agregamos Disponible al INSERT
        var cmd = new SqlCommand(
            "INSERT INTO Platos (Nombre, Descripcion, Precio, CategoriaId, Disponible) OUTPUT INSERTED.Id VALUES (@nombre, @desc, @precio, @cat, @disp)", cn);
        cmd.Parameters.AddWithValue("@nombre", p.Nombre);
        cmd.Parameters.AddWithValue("@desc", (object)p.Descripcion! ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@precio", p.Precio);
        cmd.Parameters.AddWithValue("@cat", p.CategoriaId);
        cmd.Parameters.AddWithValue("@disp", p.Disponible); // Guardamos el estado enviado

        return (int)(await cmd.ExecuteScalarAsync())!;
    }

        public async Task<int> ActualizarAsync(Plato p)
    {
        await using var cn = new SqlConnection(_cn);
        await cn.OpenAsync();

        // Agregamos Disponible = @disp al UPDATE
        await using var cmd = new SqlCommand(
            "UPDATE Platos SET Nombre = @nombre, Descripcion = @desc, Precio = @precio, CategoriaId = @cat, Disponible = @disp WHERE Id = @id", cn);
        cmd.Parameters.AddWithValue("@nombre", p.Nombre);
        cmd.Parameters.AddWithValue("@desc", (object)p.Descripcion! ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@precio", p.Precio);
        cmd.Parameters.AddWithValue("@cat", p.CategoriaId);
        cmd.Parameters.AddWithValue("@disp", p.Disponible); // Permite cambiar a true o false
        cmd.Parameters.AddWithValue("@id", p.Id);

        return await cmd.ExecuteNonQueryAsync();
    }


    public async Task<int> EliminarAsync(int id)
    {
        await using var cn = new SqlConnection(_cn);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(
            "UPDATE Platos SET Disponible = 0 WHERE Id = @id AND Disponible = 1", cn);
        cmd.Parameters.AddWithValue("@id", id);
        return await cmd.ExecuteNonQueryAsync(); // Retorna las filas afectadas
    }

    // Lee filas del SELECT con JOIN y las convierte en objetos Plato usando SQL Server
    private async Task<List<Plato>> LeerAsync(string sql, params (string nombre, object valor)[] parametros)
    {
        var lista = new List<Plato>();
        await using var cn = new SqlConnection(_cn);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);

        foreach (var (nombre, valor) in parametros)
            cmd.Parameters.AddWithValue(nombre, valor);

        await using var dr = await cmd.ExecuteReaderAsync();
        while (await dr.ReadAsync())
        {
            lista.Add(new Plato
            {
                Id = dr.GetInt32(0),
                Nombre = dr.GetString(1),
                Descripcion = dr.IsDBNull(2) ? "" : dr.GetString(2),
                Precio = dr.GetDecimal(3),
                CategoriaId = dr.GetInt32(4),

                // 1. Agregamos Disponible leyendo la columna booleana (Índice 5)
                Disponible = dr.GetBoolean(5),

                // 2. Desplazamos Categoria para que lea el nuevo Índice 6
                Categoria = dr.IsDBNull(6) ? "" : dr.GetString(6)
            });

        }
        return lista;
    }
}

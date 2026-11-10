using Microsoft.Data.SqlClient;
using RestApi.Entidades;

namespace RestApi.Datos
{
    public class CategoriaRepositorio(IConfiguration config)
    {
        private readonly string _cn = config.GetConnectionString("Restaurante")!;

        public async Task<List<Categoria>> ObtenerTodosAsync()
        {
            var Lista = new List<Categoria>();
            await using var cn = new SqlConnection(_cn);
            await cn.OpenAsync();

            var cmd = new SqlCommand(
                "SELECT Id, Nombre FROM Categorias ORDER BY Nombre", cn);

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                Lista.Add(new Categoria
                {
                    Id = dr.GetInt32(0),
                    Nombre = dr.GetString(1)
                });
            }
            return Lista;
        }

        public async Task<int> AgregarAsync(Categoria c)
        {
            await using var cn = new SqlConnection(_cn);
            await cn.OpenAsync();

            var cmd = new SqlCommand(
                "INSERT INTO Categorias (Nombre) OUTPUT INSERTED.Id VALUES (@nombre)", cn);
            cmd.Parameters.AddWithValue("@nombre", c.Nombre);

            return (int)(await cmd.ExecuteScalarAsync())!;
        }

        public async Task<bool> ActualizarAsync(Categoria c)
        {
            await using var cn = new SqlConnection(_cn);
            await cn.OpenAsync();

            await using var cmd = new SqlCommand(
                "UPDATE Categorias SET Nombre = @nombre WHERE Id = @id AND Disponible = 1", cn);
            cmd.Parameters.AddWithValue("@nombre", c.Nombre);
            cmd.Parameters.AddWithValue("@id", c.Id);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            Categoria? categoria = null;
            await using var cn = new SqlConnection(_cn);
            await cn.OpenAsync();

            var cmd = new SqlCommand(
                "SELECT Id, Nombre FROM Categorias WHERE Id = @id", cn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var dr = await cmd.ExecuteReaderAsync();
            if (await dr.ReadAsync())
            {
                categoria = new Categoria
                {
                    Id = dr.GetInt32(0),
                    Nombre = dr.GetString(1)
                };
            }
            return categoria;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            await using var cn = new SqlConnection(_cn);
            await cn.OpenAsync();

            await using var cmd = new SqlCommand(
                "DELETE FROM Categorias WHERE Id = @id", cn);
            cmd.Parameters.AddWithValue("@id", id);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }


        // Agrega este método dentro de tu CategoriaRepositorio.cs
        public async Task<int> ContarPlatosAsync(int categoriaId)
        {
            await using var cn = new SqlConnection(_cn);
            await cn.OpenAsync();

            // Contamos cuántos platos activos están usando esta categoría
            var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Platos WHERE CategoriaId = @catId AND Disponible = 1", cn);
            cmd.Parameters.AddWithValue("@catId", categoriaId);

            return (int)(await cmd.ExecuteScalarAsync())!;
        }

    }
}

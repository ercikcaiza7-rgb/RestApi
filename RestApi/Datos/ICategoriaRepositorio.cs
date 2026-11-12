using RestApi.Entidades;

namespace RestApi.Datos;

public interface ICategoriaRepositorio
{
    Task<List<Categoria>> ObtenerTodosAsync();
    Task<int> AgregarAsync(Categoria c);
    Task<bool> ActualizarAsync(Categoria c);
    Task<Categoria?> ObtenerPorIdAsync(int id);
    Task<bool> EliminarAsync(int id);
    Task<int> ContarPlatosAsync(int categoriaId);
}

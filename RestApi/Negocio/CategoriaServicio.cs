using RestApi.Datos;
using RestApi.Entidades;

namespace RestApi.Negocio
{
    public class CategoriaServicio(ICategoriaRepositorio repo)
    {
        // Imagen 1: Obtener todas usando la expresión shorthand (=>)
        public Task<List<Categoria>> ObtenerTodasAsync() => repo.ObtenerTodosAsync();

        // Imagen 1: Obtener por ID lanzando KeyNotFoundException (404) si no existe
        public async Task<Categoria> ObtenerPorIdAsync(int id)
        {
            var categoria = await repo.ObtenerPorIdAsync(id);
            if (categoria is null)
                throw new KeyNotFoundException($"No existe la categoría {id}."); // // 404

            return categoria;
        }

        // Imagen 1: Agregar ejecutando Validar primero
        public async Task<int> AgregarAsync(Categoria c)
        {
            Validar(c);
            return await repo.AgregarAsync(c);
        }

        // Imagen 2: Actualizar validando filas afectadas y lanzando 404 si falla
        public async Task ActualizarAsync(Categoria c)
        {
            Validar(c);

            // Suponiendo que tu repositorio retorna bool, lo convertimos a filas para mantener la lógica de la imagen
            var exito = await repo.ActualizarAsync(c);
            var filas = exito ? 1 : 0;

            if (filas == 0)
                throw new KeyNotFoundException($"No existe la categoría {c.Id}."); // // 404
        }

        // Imagen 2: Eliminar aplicando el control de platos (Regla del 409)
        public async Task EliminarAsync(int id)
        {
            // Regla de negocio: no se borra una categoría que tiene platos
            var platos = await repo.ContarPlatosAsync(id);
            if (platos > 0)
            {
                throw new InvalidOperationException(
                    $"La categoría {id} tiene {platos} platos. Muévelos o elimínalos primero."); // // 409
            }

            var exito = await repo.EliminarAsync(id);
            var filas = exito ? 1 : 0;

            if (filas == 0)
                throw new KeyNotFoundException($"No existe la categoría {id}."); // // 404
        }

        // Imagen 3: Estructura exacta del método privado Validar
        private static void Validar(Categoria c)
        {
            if (string.IsNullOrWhiteSpace(c.Nombre))
                throw new ArgumentException("El nombre de la categoría es obligatorio."); // // 400

            if (c.Nombre.Length > 50)
                throw new ArgumentException("El nombre no puede superar los 50 caracteres."); // // 400

            c.Nombre = c.Nombre.Trim();
        }
    }
}

namespace RestApi.Web;

using RestApi.Entidades;
using RestApi.Negocio;

public static class PlatoEndpoints
{
    public static void MapPlatoEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/platos"); // Todas empiezan con /platos

        grupo.MapGet("/", (PlatoServicio s) =>
            Respuesta.Manejar(async () => Results.Ok(await s.ObtenerTodosAsync())));

        grupo.MapGet("/{id:int}", (int id, PlatoServicio s) =>
            Respuesta.Manejar(async () => Results.Ok(await s.ObtenerPorIdAsync(id))));

        grupo.MapPost("/", (Plato p, PlatoServicio s) =>
            Respuesta.Manejar(async () =>
            {
                var nuevoId = await s.AgregarAsync(p);
                p.Id = nuevoId;
                return Results.Created($"/platos/{nuevoId}", p);
            }));

        grupo.MapPut("/{id:int}", (int id, Plato p, PlatoServicio s) =>
            Respuesta.Manejar(async () =>
            {
                p.Id = id;
                await s.ActualizarAsync(p);
                return Results.Ok(p); // Retorna 200 con el objeto modificado tal cual tu imagen
            }));

        grupo.MapDelete("/{id:int}", (int id, PlatoServicio s) =>
            Respuesta.Manejar(async () =>
            {
                await s.EliminarAsync(id);
                return Results.NoContent(); // Retorna 204
            }));
    }
}


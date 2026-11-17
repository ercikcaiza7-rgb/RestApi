using RestApi.Entidades;
using RestApi.Negocio;
using QRCoder;

namespace RestApi.Web
{
            public static class MesaEndpoints{
            public static void MapMesaEndpoints(
                this IEndpointRouteBuilder app)
            {
                var g = app.MapGroup("/mesas").RequireAuthorization("SoloAdmin");

            g.MapGet("", async (MesaServicio s) =>
                    Results.Ok(await s.ListarAsync()));

                g.MapGet("/{id:int}", (int id, MesaServicio s) =>
                    Respuesta.Manejar(async () =>
                        Results.Ok(await s.ObtenerAsync(id))));

                g.MapPost("", (Mesa m, MesaServicio s) =>
                    Respuesta.Manejar(async () =>
                    {
                        m.Id = await s.CrearAsync(m);
                        return Results.Created($"/mesas/{m.Id}", m);
                    }));
                g.MapPut("/{id:int}", (int id, Mesa m, MesaServicio s) =>
            Respuesta.Manejar(async () =>
            {
                m.Id = id;
                await s.ActualizarAsync(m);
                return Results.Ok(m);
            }));

                g.MapDelete("/{id:int}", (int id, MesaServicio s) =>
                    Respuesta.Manejar(async () =>
                    {
                        await s.EliminarAsync(id);
                        return Results.NoContent();
                    }));
            /// genera el QR de la mesa (para imprimirlo y pegarlo en la mesa)
            g.MapGet("/{id:int}/qr", (int id, MesaServicio s, IConfiguration cfg) =>
    Respuesta.Manejar(async () =>
    {
        var mesa = await s.ObtenerAsync(id);            // 404 si no existe
        var url = $"{cfg["App:UrlMenu"]}?mesa={mesa.Numero}";

        using var gen = new QRCodeGenerator();
        using var datos = gen.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(datos).GetGraphic(10);

        return Results.File(png, "image/png", $"mesa-{mesa.Numero}.png");
    }));
        }
        }
    }




// Proyecto RestApi · Program.cs
using RestApi.Datos;
using RestApi.Negocio;
using RestApi.Web;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. REGISTRO DE CAPAS (Inyección de Dependencias)
// ==========================================
builder.Services.AddScoped<ProductoRepositorio>();
builder.Services.AddScoped<ProductoServicio>();

builder.Services.AddScoped<PlatoRepositorio>();
builder.Services.AddScoped<PlatoServicio>();

builder.Services.AddScoped<CategoriaRepositorio>();
builder.Services.AddScoped<CategoriaServicio>();

var app = builder.Build();

// ==========================================
// ENDPOINTS DE PRODUCTOS (Temporales)
// ==========================================
app.MapGet("/productos", (ProductoServicio servicio) => Results.Ok(servicio.ListarDisponibles()));
app.MapGet("/productos/{id:int}", async (int id, ProductoServicio servicio) =>
{
    return await Respuesta.Manejar(async () =>
    {
        var producto = servicio.Buscar(id);
        if (producto is null) throw new KeyNotFoundException("No existe el producto " + id);
        return Results.Ok(producto);
    });
});

// ==========================================
// ENDPOINTS MODULARIZADOS (Categorías y Platos)
// ==========================================
app.MapCategoriaEndpoints(); // Controla /categorias
app.MapPlatoEndpoints();     // Controla /platos

app.Run();


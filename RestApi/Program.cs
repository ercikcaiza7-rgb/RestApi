// Proyecto RestApi · Program.cs
using RestApi.Datos;
using RestApi.Negocio;
using RestApi.Web;
using Microsoft.EntityFrameworkCore;


// Cargar variables de entorno desde archivo .env si existe (según AGENTS.md)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#')) continue;
        var parts = trimmed.Split('=', 2);
        if (parts.Length == 2)
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
    }
}

var builder = WebApplication.CreateBuilder(args);
// ==========================================
// 1. RestauranteContext: Configurar la cadena de conexión y el DbContext
// ==========================================
var cn = builder.Configuration
    .GetConnectionString("Restaurante");

// ==========================================
// 1. REGISTRO DE CAPAS (Inyección de Dependencias)
// ==========================================
builder.Services.AddScoped<ProductoRepositorio>();
builder.Services.AddScoped<ProductoServicio>();

builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<CategoriaServicio>();

builder.Services.AddScoped<IPlatoRepositorio, PlatoRepositorio>();
builder.Services.AddScoped<PlatoServicio>();

builder.Services.AddDbContext<RestauranteContext>(o => o.UseSqlServer(cn));

builder.Services.AddScoped<IMesaRepositorio, MesaRepositorio>();
builder.Services.AddScoped<MesaServicio>();

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<UsuarioServicio>();

var app = builder.Build();

// ==========================================
// ENDPOINTS DE PRODUCTOS (Temporales)
// ==========================================
app.MapGet("/productos", (ProductoServicio servicio) =>
    Respuesta.Manejar(async () => Results.Ok(servicio.ListarDisponibles())));

app.MapGet("/productos/{id:int}", (int id, ProductoServicio servicio) =>
    Respuesta.Manejar(async () =>
    {
        var producto = servicio.Buscar(id);
        if (producto is null) throw new KeyNotFoundException("No existe el producto " + id);
        return Results.Ok(producto);
    }));

// ==========================================
// ENDPOINTS MODULARIZADOS (Categorías y Platos)
// ==========================================
app.MapCategoriaEndpoints(); // Controla /categorias
app.MapPlatoEndpoints();     // Controla /platos
app.MapMesaEndpoints();      // Controla /mesas
app.MapUsuarioEndpoints();   // Controla /usuarios

app.Run();

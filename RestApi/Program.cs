// Proyecto RestApi · Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestApi.Datos;
using RestApi.Entidades;
using RestApi.Negocio;
using RestApi.Web;
using System.Text;


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
// Categoría: repositorio y servicio, con inyección de dependencias
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<CategoriaServicio>();
// Plato: repositorio y servicio, con inyección de dependencias
builder.Services.AddScoped<IPlatoRepositorio, PlatoRepositorio>();
builder.Services.AddScoped<PlatoServicio>();
// QRCodeGenerator se usa para generar códigos QR de las mesas
builder.Services.AddDbContext<RestauranteContext>(o => o.UseSqlServer(cn));
// 
builder.Services.AddScoped<IMesaRepositorio, MesaRepositorio>();
builder.Services.AddScoped<MesaServicio>();
// Usuarios: repositorio y servicio, con inyección de dependencias
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<UsuarioServicio>();
// Jwt - login y registro de usuarios
builder.Services.AddScoped<TokenServicio>();
builder.Services.AddScoped<AuthServicio>();
// validar la autorización según el rol del usuario (Admin o Mesero)
var jwt = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        // Deja "name" y "role" con el mismo nombre del token
        o.MapInboundClaims = false;

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,      // vence a la hora exacta
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("SoloAdmin",
        p => p.RequireRole(Roles.Administrador));

var app = builder.Build();

// NUEVO: en este orden y antes de los Map
app.UseAuthentication();   // ¿quién eres?   → 401
app.UseAuthorization();    // ¿puedes pasar? → 403

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
// ENDPOINTS MODULARIZADOS 
// ==========================================
app.MapCategoriaEndpoints(); // Controla /categorias
app.MapPlatoEndpoints();     // Controla /platos
app.MapMesaEndpoints();      // Controla /mesas
app.MapUsuarioEndpoints();   // Controla /usuarios
app.MapAuthEndpoints();      // Controla /auth
app.Run();

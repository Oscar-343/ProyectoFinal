using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;

var builder = WebApplication.CreateBuilder(args);

// Lee la cadena de conexión a la db
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registra el DbContext para poder inyectarlo en los controladores
builder.Services.AddDbContext<TiendaDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Habilita el uso de Controladores + Vistas
builder.Services.AddControllersWithViews();

// Necesario para que HttpContext.Session funcione
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;                 
    options.Cookie.IsEssential = true;              
});

var app = builder.Build();

// Oculta errores detallados y fuerza HTTPS estricto.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection(); // HTTP -> HTTPS.
app.UseStaticFiles();      // Permite servir archivos de wwwroot 

app.UseRouting(); // Habilita el sistema de rutas de MVC.

app.UseSession(); // Habilita el uso de HttpContext.Session en los controladores.

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
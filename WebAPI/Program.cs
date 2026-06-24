using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = null;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// URL implicit dacă nu s-a setat altul prin --urls / ASPNETCORE_URLS / launchSettings.json.
if (app.Urls.Count == 0 &&
    string.IsNullOrEmpty(builder.Configuration["urls"]) &&
    string.IsNullOrEmpty(builder.Configuration["ASPNETCORE_URLS"]))
{
    app.Urls.Add("http://localhost:5000");
}

app.UseCors();

string webRoot = ResolveWebRoot(app.Environment.ContentRootPath);
if (webRoot != null)
{
    var provider = new PhysicalFileProvider(webRoot);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = provider });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = provider });
    Console.WriteLine("[WebAPI] Pagina WEB servită din: " + webRoot);
    Console.WriteLine("[WebAPI] Deschide: http://localhost:5000/");
}
else
{
    Console.WriteLine("[WebAPI] Folderul Web/ nu a fost găsit lângă WebAPI — frontend-ul nu va fi servit.");
}

app.MapControllers();
app.Run();

static string ResolveWebRoot(string contentRoot)
{
    string[] candidates = new[]
    {
        Path.Combine(contentRoot, "Web"),
        Path.Combine(contentRoot, "..", "Web"),
        Path.Combine(contentRoot, "..", "..", "..", "..", "Web")
    };
    foreach (var c in candidates)
    {
        string full = Path.GetFullPath(c);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "index.html")))
            return full;
    }
    return null;
}

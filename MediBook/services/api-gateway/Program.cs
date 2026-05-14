using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot config
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// CORS — allow Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowAngular");

await app.UseOcelot();

app.Run();

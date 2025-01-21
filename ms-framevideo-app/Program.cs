// src/Program.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using ms_framevideo_app.src.infrastructure.configuration;
using ms_framevideo_app.src.infrastructure.framework;

var builder = WebApplication.CreateBuilder(args);

// Carrega configurações do appsettings.json
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Adiciona Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adiciona injeção de dependência customizada
builder.Services.AddProjectDependencies(builder.Configuration);

// Exemplo de adicionar um BackgroundService que consome o RabbitMQ
builder.Services.AddHostedService<KubernetesWorkerService>();

var app = builder.Build();

// Se em modo de desenvolvimento, mostra Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

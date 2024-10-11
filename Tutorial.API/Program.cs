﻿using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Configuration;
using Polly;
using Serilog;
using System.Net;
using System.Xml.Serialization;
using Tutorial.Domain.Model.Options;
using Tutorial.Infrastructure.Facades.Common.HttpClients;
using Tutorial.Infrastructure.Facades.Common.HttpClients.Interfaces;
using Tutorial.Infrastructure.Services;
using Tutorial.Infrastructure.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient
builder.Services.AddHttpClient();

// Add HttpClientSender
builder.Services.AddHttpClientSender();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(HttpResultProfile));

// Services
builder.Services.AddTransient<ITransientService, SomeService>();
builder.Services.AddScoped<IScopedService, SomeService>();
builder.Services.AddSingleton<ISingletonService, SomeService>();
builder.Services.AddSingleton<ISingletonService, CaptiveDependencyService>();
builder.Services.AddTransient<IMQTTService, MQTTService>();
builder.Services.AddTransient<IHttpHeadersManager, HttpHeadersManager>();
builder.Services.AddSingleton<IPollyOptionsService, PollyOptionsService>();

//Add support to logging with SERILOG
//builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

// Options
builder.Services.Configure<PollyOptions>(builder.Configuration.GetSection("PollyOptions"));

var app = builder.Build();

app.UseStaticFiles();

//Add support to logging request with SERILOG
//app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

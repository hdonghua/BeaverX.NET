using BeaverX.Core;
using BeaverX.Sample.HttpApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBeaverX<HttpApiSampleModule>();

var app = builder.Build();

app.InitializeBeaverX();

app.Run();
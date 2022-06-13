using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.MatrixParameter.Samples;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);

app.Run();

public partial class Program { }
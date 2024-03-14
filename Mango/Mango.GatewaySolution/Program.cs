using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppAuthentication();

if (!builder.Environment.EnvironmentName.ToLower().Equals("development"))
{
    builder.Configuration.AddJsonFile("ocelot.Production.json", optional: false, reloadOnChange: true);
}
else
{
    builder.Configuration.AddJsonFile("ocelot.Development.json", optional: false, reloadOnChange: true);
}

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseOcelot().GetAwaiter().GetResult();

app.Run();

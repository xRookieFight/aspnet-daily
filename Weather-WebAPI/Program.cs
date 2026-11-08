using Microsoft.Extensions.Options;
using Weather_WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.Configure<OpenMeteoOptions>(builder.Configuration.GetSection(OpenMeteoOptions.SectionName));
builder.Services.AddHttpClient<WeatherService>((services, client) =>
{
    var options = services.GetRequiredService<IOptions<OpenMeteoOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();

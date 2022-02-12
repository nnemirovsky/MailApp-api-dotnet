using System.Reflection;
using MailApp.Filters;
using MailApp.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IUriService>(serviceProvider =>
{
    var accessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var request = accessor.HttpContext?.Request;
    var uri = string.Concat(request?.Scheme, "://", request?.Host.ToUriComponent());
    return new UriService(uri);
});

builder.Services.AddSingleton<IMailDataService>(serviceProvider =>
{
    var connStr = builder.Configuration.GetConnectionString("Postgresql");
    var logger = serviceProvider.GetRequiredService<ILogger<MailDataService>>();
    return new MailDataService(connStr, logger);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "MailApp API",
            Version = "v1"
        }
    );

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

try
{
    PaginationFilter.MaxPageSize = int.Parse(builder.Configuration.GetSection("Pagination")["MaxPageSize"]);
}
catch (Exception ex) when (ex is FormatException or ArgumentNullException or OverflowException)
{
    PaginationFilter.MaxPageSize = 10;
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => { options.DefaultModelsExpandDepth(-1); });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

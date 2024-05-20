using System.ComponentModel.DataAnnotations;
using CollegeAPI.Models;
using CollegeAPI.Swagger;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(builder.Configuration["AllowedOrigins"]);
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });
    opts.AddPolicy(
        name: "AnyOrigin",
        cfg =>
        {
            cfg.AllowAnyOrigin();
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
        }
    );
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.ParameterFilter<SortColumnFilter>();
    opts.ParameterFilter<SortOrderFilter>();
});
builder.Services.AddControllers(opts =>
{
    opts.ModelBindingMessageProvider.SetValueIsInvalidAccessor(x => $"The value '{x}' is invalid.");
    opts.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(x =>
        $"The value '{x}' must be a number."
    );
    opts.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (x, y) => $"The value '{x}' is not valid for {y}."
    );
    opts.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => "A value is required.");
    opts.CacheProfiles.Add("NoCache", new CacheProfile() { NoStore = true, });
    opts.CacheProfiles.Add(
        "Any-60",
        new CacheProfile() { Duration = 60, Location = ResponseCacheLocation.Any }
    );
});

string? connString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseMySql(connString, ServerVersion.AutoDetect(connString))
);

/* builder.Services.Configure<ApiBehaviorOptions>(opts => */
/* { */
/*     opts.SuppressModelStateInvalidFilter = true; */
/* }); */

builder.Services.AddResponseCaching(opts =>
{
    opts.MaximumBodySize = 32 * 1024 * 1024;
    opts.SizeLimit = 50 * 1024 * 1024;
});

builder.Services.AddStackExchangeRedisCache(opts =>
{
    opts.Configuration = builder.Configuration["Redis:ConnectionString"];
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Configuration.GetValue<bool>("UseDeveloperExceptionPage"))
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/error");

app.UseExceptionHandler(action =>
{
    action.Run(async context =>
    {
        var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();

        var details = new ProblemDetails();
        details.Detail = exceptionHandler?.Error.Message;
        details.Extensions["traceId"] =
            System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;
        details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        details.Status = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(details));
    });
});

app.UseHttpsRedirection();
app.UseCors();
app.UseResponseCaching();
app.UseAuthorization();
app.Use(
    (context, next) =>
    {
        context.Response.GetTypedHeaders().CacheControl =
            new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
            {
                NoCache = true,
                NoStore = true
            };
        return next.Invoke();
    }
);

/* app.MapGet("/error", */
/*     [EnableCors("AnyOrigin")] */
/*     [ResponseCache(Profile)] */
/*     (HttpContext context)=> { */
/*     IExceptionHandlerPathFeature? exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>(); */
/*     ProblemDetails details = new ProblemDetails(); */
/**/
/*     details.Detail = exceptionHandler?.Error.Message; */
/*     details.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier; */
/**/
/*     details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"; */
/*     details.Status = StatusCodes.Status501NotImplemented; */
/**/
/*     app.Logger.LogError( */
/*         Custom */
/*         ) */
/*     ); */

app.MapControllers().RequireCors("AnyOrigin");

app.Run();

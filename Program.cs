using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using CollegeAPI.Models;
using CollegeAPI.Swagger;
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
    opts.AddPolicy(name: "AnyOrigin",
        cfg =>
        {
            cfg.AllowAnyOrigin();
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.ParameterFilter<SortColumnFilter>();
    opts.ParameterFilter<SortOrderFilter>();
});
builder.Services.AddControllers(opts =>
{
    opts.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        x => $"The value '{x}' is invalid.");
    opts.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        x => $"The value '{x}' must be a number.");
    opts.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (x, y) => $"The value '{x}' is not valid for {y}.");
    opts.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => "A value is required.");
});

string? connString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseMySql(connString, ServerVersion.AutoDetect(connString)));

builder.Services.Configure<ApiBehaviorOptions>(opts =>
{
    opts.SuppressModelStateInvalidFilter = true;
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

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapGet("/error", [EnableCors("AnyOrigin")][ResponseCache(NoStore = true)]
() => Results.Problem());

app.MapControllers().RequireCors("AnyOrigin");

app.Run();


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Security.Cryptography;
using WebApplicationWecomEpygi.Models;
using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Criação da instância de JwtConfig
var jwtConfig = new JwtConfig {
Issuer = GenerateRandomString(32),
SecretKey = GenerateRandomString(32),
};

// Add services to the container.
builder.Services.AddSingleton<IJwtConfig>(jwtConfig);
builder.Services.AddControllersWithViews();

// Configure authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfig.SecretKey))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(),"Views", "StaticFiles")),
    RequestPath = "/StaticFiles"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");





app.Run();
string GenerateRandomString(int length)
{
    // Gerar chave secreta de 256 bits
    byte[] secretKeyBytes = new byte[length];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(secretKeyBytes);
    }
    string secretKey = Convert.ToBase64String(secretKeyBytes);
    return $"{secretKey}";
}

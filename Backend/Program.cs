using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"]
    };
});

builder.Services.AddScoped<TokenService>();

// Configure DbContext
builder.Services.AddDbContext<DotnetContext>(options =>
    options.UseSqlite("Datasource=./data.db"));

// Register IPasswordHasher<AppUser> with a custom implementation
builder.Services.AddScoped<IPasswordHasher<AppUser>, CustomPasswordHasher>();

// Add services for controllers and endpoints
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Apply migrations automatically (You might want to remove this in production)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DotnetContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Define endpoints here
// Login Endpoint
// app.MapPost("/login", async (DotnetContext dbContext, AppUser loginDetails, IPasswordHasher<AppUser> hasher, TokenService tokenService) =>
// {
//     var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Username == loginDetails.Username);
//     if (user == null || hasher.VerifyHashedPassword(user, user.Password, loginDetails.Password) != PasswordVerificationResult.Success)
//     {
//         return Results.Problem("Invalid login attempt.", statusCode: 401);
//     }

//     var token = tokenService.GenerateJwtToken(user);
//     var refreshToken = GenerateRefreshToken();
//     user.RefreshToken = refreshToken;
//     user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

//     await dbContext.SaveChangesAsync();
//     return Results.Ok(new { Token = token, RefreshToken = refreshToken });
// });
app.MapPost("/login", async (DotnetContext dbContext, AppUser loginDetails, IPasswordHasher<AppUser> hasher) =>
{
    var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Username == loginDetails.Username);
    if (user == null || hasher.VerifyHashedPassword(user, user.Password, loginDetails.Password) != PasswordVerificationResult.Success)
    {
        return Results.Problem("Invalid login attempt.", statusCode: 401);
    }

    // Since you're not using tokens, you might want to perform some other action here,
    // like updating last login timestamp or just returning a success message.
    // For now, just return a simple success message.
    return Results.Ok(new { Message = "Login successful" });
});


static string GenerateRefreshToken()
{
    var randomNumber = new byte[32];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}

// Register Endpoint
app.MapPost("/register", async (DotnetContext dbContext, AppUser registrationDetails, IPasswordHasher<AppUser> hasher) =>
{
    if (await dbContext.Users.AnyAsync(u => u.Username == registrationDetails.Username))
    {
        return Results.BadRequest("User already exists.");
    }

    // Generate a refresh token
    registrationDetails.RefreshToken = GenerateRefreshToken();
    registrationDetails.RefreshTokenExpiryTime = DateTime.Now.AddDays(7); // Set an expiry time

    registrationDetails.Password = hasher.HashPassword(registrationDetails, registrationDetails.Password);
    dbContext.Users.Add(registrationDetails);
    await dbContext.SaveChangesAsync();

    return Results.Ok("User registered successfully.");
});


// Refresh Token Endpoint
app.MapPost("/refresh-token", async (DotnetContext dbContext, string refreshToken, TokenService tokenService) => // Removed IConfiguration config from parameters
{
    var user = await dbContext.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime > DateTime.Now);
    if (user == null)
    {
        return Results.Json(new { Message = "Invalid refresh token." }, statusCode: 401);
    }

    // Use tokenService to generate a new token
    var newToken = tokenService.GenerateJwtToken(user); // Updated to use tokenService
    var newRefreshToken = GenerateRefreshToken(); // Assuming this method is defined elsewhere in your Program.cs
    user.RefreshToken = newRefreshToken;
    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

    await dbContext.SaveChangesAsync();
    return Results.Ok(new { Token = newToken, RefreshToken = newRefreshToken });
});

// Get All Users Endpoint
app.MapGet("/users", async (DotnetContext dbContext) =>
{
    var users = await dbContext.Users.Select(u => new UserDto { Id = u.Id, Username = u.Username }).ToListAsync();
    return Results.Ok(users);
});

// Get User By ID Endpoint
app.MapGet("/users/{id}", async (DotnetContext dbContext, int id) =>
{
    var user = await dbContext.Users.FindAsync(id);
    if (user == null) return Results.NotFound("User not found.");
    return Results.Ok(new UserDto { Id = user.Id, Username = user.Username });
}).RequireAuthorization("AdminOnly");

// Update User Endpoint
app.MapPut("/users/{id}", async (DotnetContext dbContext, int id, AppUser updatedUser, IPasswordHasher<AppUser> hasher) =>
{
    var user = await dbContext.Users.FindAsync(id);
    if (user == null) return Results.NotFound("User not found.");

    user.Username = updatedUser.Username;
    user.Password = hasher.HashPassword(user, updatedUser.Password);
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
}).RequireAuthorization("AdminOnly");

// Delete User Endpoint
app.MapDelete("/users/{id}", async (DotnetContext dbContext, int id) =>
{
    var user = await dbContext.Users.FindAsync(id);
    if (user == null) return Results.NotFound("User not found.");

    dbContext.Users.Remove(user);
    await dbContext.SaveChangesAsync();

    return Results.Ok($"User with ID {id} has been deleted.");
}).RequireAuthorization("AdminOnly");


app.Run();

public class AppUser
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
}

// Models and DbContext
public class DotnetContext : DbContext
{
    public DbSet<AppUser> Users { get; set; }

    public DotnetContext(DbContextOptions<DotnetContext> options) : base(options) { }
}

public class CustomPasswordHasher : IPasswordHasher<AppUser>
{
    public string HashPassword(AppUser user, string password)
    {
        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8)) + ":" + Convert.ToBase64String(salt);
    }

    public PasswordVerificationResult VerifyHashedPassword(AppUser user, string hashedPassword, string providedPassword)
    {
        var parts = hashedPassword.Split(':');
        if (parts.Length != 2)
        {
            return PasswordVerificationResult.Failed;
        }
        var salt = Convert.FromBase64String(parts[1]);
        var providedHash = KeyDerivation.Pbkdf2(
            password: providedPassword,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8);

        var expectedHash = Convert.FromBase64String(parts[0]);
        if (providedHash.SequenceEqual(expectedHash))
        {
            return PasswordVerificationResult.Success;
        }
        return PasswordVerificationResult.Failed;
    }
}


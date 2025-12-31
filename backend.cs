using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Add DB Context (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=.;Database=CubTechDB;Trusted_Connection=True;"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors();

// Root Endpoint
app.MapGet("/", () => "CubTech Backend Running");

// ================= LOGIN =================
app.MapPost("/login", async (LoginRequest login, ApplicationDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u =>
        u.Email == login.Email && u.Password == login.Password);

    if (user != null)
        return Results.Ok(new { message = "Login successful" });

    return Results.BadRequest(new { message = "Invalid email or password" });
});

// ================= GET ALL STUDENTS =================
app.MapGet("/students", async (ApplicationDbContext db) =>
{
    var students = await db.Students.ToListAsync();
    return Results.Ok(students);
});

// ================= ADD NEW STUDENT =================
app.MapPost("/students", async (Student student, ApplicationDbContext db) =>
{
    if (string.IsNullOrEmpty(student.Name) ||
        string.IsNullOrEmpty(student.Email) ||
        string.IsNullOrEmpty(student.Phone) ||
        string.IsNullOrEmpty(student.Course))
    {
        return Results.BadRequest(new { message = "All fields are required" });
    }

    db.Students.Add(student);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Student registered successfully" });
});

// ================= DELETE STUDENT =================
app.MapDelete("/students/{id}", async (int id, ApplicationDbContext db) =>
{
    var student = await db.Students.FindAsync(id);
    if (student == null) return Results.NotFound(new { message = "Student not found" });

    db.Students.Remove(student);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Student deleted successfully" });
});

app.Run();

// ================= DATABASE & MODELS =================
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options) { }
    public DbSet<Student> Students { get; set; }
    public DbSet<User> Users { get; set; }
}

public class Student
{
    public int Id { get; set; }
    [Required] public string Name { get; set; }
    [Required] public string Email { get; set; }
    [Required] public string Phone { get; set; }
    [Required] public string Course { get; set; }
}

public class User
{
    public int Id { get; set; }
    [Required] public string Email { get; set; }
    [Required] public string Password { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

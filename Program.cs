using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDb>(options =>
    options.UseSqlite("Data Source=employees.db"));

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("/employees"));

app.MapGet("/employees", async (AppDb db) =>
    await db.Employees.ToListAsync());

app.MapGet("/employees/{id:int}", async (int id, AppDb db) =>
    await db.Employees.FindAsync(id) is Employee e ? Results.Ok(e) : Results.NotFound());

app.MapPost("/employees", async (Employee emp, AppDb db) => {
    db.Employees.Add(emp);
    await db.SaveChangesAsync();
    return Results.Created($"/employees/{emp.Id}", emp);
});

app.MapPut("/employees/{id:int}", async (int id, Employee input, AppDb db) => {
    var emp = await db.Employees.FindAsync(id);
    if (emp is null) return Results.NotFound();
    emp.Name = input.Name;
    emp.Position = input.Position;
    emp.Salary = input.Salary;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/employees/{id:int}", async (int id, AppDb db) => {
    var emp = await db.Employees.FindAsync(id);
    if (emp is null) return Results.NotFound();
    db.Employees.Remove(emp);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.EnsureCreated();
}

app.Run();

public class Employee {
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Position { get; set; } = "";
    public decimal Salary { get; set; }
}

public class AppDb : DbContext {
    public AppDb(DbContextOptions<AppDb> opts) : base(opts) {}
    public DbSet<Employee> Employees { get; set; }
}

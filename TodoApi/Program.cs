// using Microsoft.EntityFrameworkCore;
// using TodoApi;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.IdentityModel.Tokens;
// using System.Text;
// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddCors();
//  // הוסף את השורה הזו

// builder.Services.AddDbContext<ToDoDbContext>(options =>
//     options.UseMySql("Server=localhost;Database=ToDoDB;User=root;Password=yourpassword", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.41-mysql")));

// builder.Services.AddAuthorization();
// builder.Services.AddControllers();


// builder.Services.AddEndpointsApiExplorer(); // Move this line up
// builder.Services.AddSwaggerGen();
// var key = "your_super_secret_key!123"; // שמרי את זה במקום בטוח, אפשר בקובץ הגדרות

// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = false,
//             ValidateAudience = false,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
//         };
//     });
// var app = builder.Build();

// app.MapGet("/", () => "Hello World!");

// app.MapPost("/login", (User loginUser, ToDoDbContext db) =>
// {
//     var user = db.Users.FirstOrDefault(u => u.Username == loginUser.Username && u.Password == loginUser.Password);

//     if (user == null)
//     {
//         return Results.Unauthorized();
//     }

//     // צור תביעות Claims לטוקן
//     var claims = new[]
//     {
//         new Claim(ClaimTypes.Name, user.Username),
//         new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
//     };

//     var keyBytes = Encoding.UTF8.GetBytes(key);
//     var securityKey = new SymmetricSecurityKey(keyBytes);
//     var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

//     var tokenDescriptor = new JwtSecurityToken(
//         claims: claims,
//         expires: DateTime.UtcNow.AddHours(1),
//         signingCredentials: credentials
//     );

//     var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

//     return Results.Ok(new { token });
// });
// app.MapPost("/register", (User newUser, ToDoDbContext db) =>
// {
//     // אין צורך בהצפנה של הסיסמה
//     db.Users.Add(newUser);
//     db.SaveChanges();
//     return Results.Ok();
// });

// app.MapGet("/item", async (ToDoDbContext db) => {
//     var items = await db.Items.ToListAsync();
//     foreach (var item in items) {
//         Console.WriteLine($"📌 Item: {item.IdItems}, {item.Name}, {item.IsComplete}");
//     }
//     return Results.Ok(items);
// }).RequireAuthorization();

// app.MapPost("/item", async (ToDoDbContext db, Item newTask) =>
// {
//     db.Items.Add(newTask); // ה-IdItems יווצר אוטומטית
//     await db.SaveChangesAsync();
//     return Results.Created($"/item/{newTask.IdItems}", newTask); // החזר את האובייקט עם ה-ID החדש
// }).RequireAuthorization();


// app.MapPut("/item/{IdItems}", async (int IdItems, ToDoDbContext db, Item newitem) =>
// {
//     Console.WriteLine($"Updating: {IdItems}, {newitem.Name}, {newitem.IsComplete}"); // בדיקת קלט
//     var itemd = await db.Items.FindAsync(IdItems);

//     if (itemd == null) return Results.NotFound();

//     itemd.Name = newitem.Name;
//     itemd.IsComplete = newitem.IsComplete;

//     await db.SaveChangesAsync();
//     return Results.Ok(itemd);
// }).RequireAuthorization();



// app.MapDelete("/item/{IdItems}", async (int IdItems, ToDoDbContext db) =>

// {
//     var task = await db.Items.FindAsync(IdItems);
//     if (task is null) return Results.NotFound();

//     db.Items.Remove(task);
//     await db.SaveChangesAsync();
//     return Results.NoContent();
// }).RequireAuthorization();

// app.UseCors(policy =>
//     policy.AllowAnyOrigin()
//           .AllowAnyMethod()
//           .AllowAnyHeader());

// //  builder.Services.AddControllers();

// // builder.Services.AddEndpointsApiExplorer();
// // builder.Services.AddSwaggerGen();

// app.UseSwagger();
// app.UseSwaggerUI();
// app.UseAuthentication();
// app.UseAuthorization();
// app.MapControllers();
// app.Run();
using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
var builder = WebApplication.CreateBuilder(args);

// הוספת שירותים
builder.Services.AddCors();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.41-mysql")));

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// הגדרת מפתח סודי לטוקן (אל תשכח לשמור אותו במקום בטוח!)
var key = "ThisIsASecretKeyForJWT256Bits!!!"; // דוגמה למפתח באורך נכון (32 תווים, 256 סיביות)

// הגדרת Authentication עם JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

var app = builder.Build();
app.UseCors(builder =>
    builder.AllowAnyOrigin() // מאפשר לכל מקור
           .AllowAnyMethod() // מאפשר כל שיטה (GET, POST, PUT וכו')
           .AllowAnyHeader()); // מאפשר כל כותרת

app.UseAuthentication();
app.UseAuthorization();
// הגדרת מסלולים

// דף ברוך הבא (רק לבדוק אם השרת עובד)
app.MapGet("/", () => "Hello World!");

// התחברות - יצירת טוקן JWT
app.MapPost("/login", (User loginUser, ToDoDbContext db) =>
{
    Console.WriteLine($"Received username: {loginUser.Username}, password: {loginUser.Password}");

    var user = db.Users.FirstOrDefault(u => u.Username == loginUser.Username && u.Password == loginUser.Password);

    if (user == null)
    {
        Console.WriteLine("User not found or invalid credentials.");
        return Results.Unauthorized();
    }

    Console.WriteLine($"User {loginUser.Username} found, creating JWT...");

    // שימוש באותו מפתח שהגדרת ב-AddAuthentication
    var keyBytes = Encoding.UTF8.GetBytes("ThisIsASecretKeyForJWT256Bits!!!");
    var securityKey = new SymmetricSecurityKey(keyBytes);
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[] {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
    };

    var tokenDescriptor = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials
    );

    var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

    Console.WriteLine("JWT created successfully.");

    return Results.Ok(new { token });
});


// הרשמה - הוספת משתמש חדש
app.MapPost("/register", (User newUser, ToDoDbContext db) =>

{
    Console.WriteLine($"Attempting to register user: {newUser.Username}");

    try
    {
        db.Users.Add(newUser);
        db.SaveChanges();
        return Results.Ok();
    }
    catch (Exception ex)
{
    Console.WriteLine($"❌ Error during registration: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"💥 Inner exception: {ex.InnerException.Message}");
    
    return Results.Problem(detail: ex.Message, statusCode: 500);
}

});



// גישה למשימות - רק לאחר התחברות והחזקת טוקן JWT
app.MapGet("/item", async (ToDoDbContext db) => {
    var items = await db.Items.ToListAsync();
    foreach (var item in items) {
        Console.WriteLine($"📌 Item: {item.IdItems}, {item.Name}, {item.IsComplete}");
    }
    return Results.Ok(items);
}).RequireAuthorization();  // דורש אימות JWT

// הוספת משימה חדשה - רק לאחר התחברות והחזקת טוקן JWT
app.MapPost("/item", async (ToDoDbContext db, Item newTask) =>
{
    db.Items.Add(newTask); // ה-IdItems יווצר אוטומטית
    await db.SaveChangesAsync();
    return Results.Created($"/item/{newTask.IdItems}", newTask); // החזר את האובייקט עם ה-ID החדש
}).RequireAuthorization();  // דורש אימות JWT

// עדכון משימה קיימת
app.MapPut("/item/{IdItems}", async (int IdItems, ToDoDbContext db, Item newitem) =>
{
    Console.WriteLine($"Updating: {IdItems}, IsComplete: {newitem.IsComplete}"); // בדיקת קלט
    var itemd = await db.Items.FindAsync(IdItems);

    if (itemd == null) return Results.NotFound();

    itemd.IsComplete = newitem.IsComplete; // עדכון הסטטוס בלבד

    await db.SaveChangesAsync();
    return Results.Ok(itemd);
}).RequireAuthorization(); // דורש אימות JWT

// מחיקת משימה
app.MapDelete("/item/{IdItems}", async (int IdItems, ToDoDbContext db) =>
{
    var task = await db.Items.FindAsync(IdItems);
    if (task is null) return Results.NotFound();

    db.Items.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();  // דורש אימות JWT

// הגדרת CORS

// הפעלת Swagger
app.UseSwagger();
app.UseSwaggerUI();


// מיפוי Controllers (אם יש לך Controllers נוספים)
app.MapControllers();

app.Run();


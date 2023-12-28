using PolyArchitect.Worker;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();

app.MapGet("/helloWorld", () => "Hello World!");
// app.MapGet("/", () => "Hello World!");
app.MapHub<PersonalHub>("/debugTest");

app.Run();

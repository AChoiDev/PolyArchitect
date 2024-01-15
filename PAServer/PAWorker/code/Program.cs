using PolyArchitect.Worker;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddSingleton<AdoptionLifetime>();
builder.Services.AddHostedService(p => p.GetRequiredService<AdoptionLifetime>());

builder.Services.AddSingleton<PACoreService>();
builder.Services.AddHostedService(p => p.GetRequiredService<PACoreService>());

app = builder.Build();

app.MapGet("/helloWorld", () => "Hello World!");
// app.MapGet("/", () => "Hello World!");
app.MapHub<PersonalHub>("/debugTest");

app.Run();
Console.WriteLine("Web Application has been shut down. The window is safe to close. The application will close in 30 seconds if left to idle.");
Task.Delay(TimeSpan.FromMinutes(0.5f)).Wait();

public static partial class Program {
    private static WebApplication? app;
    public static async Task StopAsync()  {
        if (app != null) {
            await app.StopAsync();
        } else {
            throw new Exception("tried to stop not initialized app");
        }
    }
}
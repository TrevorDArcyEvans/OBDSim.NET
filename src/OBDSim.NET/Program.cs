namespace OBDSim.NET;

using OBDSim.NET.Components;

public class Program
{
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);
    var cfg = builder.Configuration;

    // Add services to the container.
    builder.Services
      .AddSingleton(new OBDSimulator(cfg["OBD:Port"]))
      .AddRazorComponents()
      .AddInteractiveServerComponents();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
      app.UseExceptionHandler("/Error");

      // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
      app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseAntiforgery();

    app
      .MapRazorComponents<App>()
      .AddInteractiveServerRenderMode();

    app.Run();
  }
}
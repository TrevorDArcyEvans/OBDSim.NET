using OBDSim.NET;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services
  .AddOpenApi()
  .AddControllers();

builder.Services
  .AddEndpointsApiExplorer()
  .AddSwaggerGen()
  .AddSingleton<OBDSimulatorFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseSwagger();
  app.UseSwaggerUI();
}
else
{
  // staging + production
  app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.MapControllers();

app.Run();

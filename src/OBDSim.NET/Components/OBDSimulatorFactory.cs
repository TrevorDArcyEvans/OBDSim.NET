namespace OBDSim.NET.Components;

public sealed class OBDSimulatorFactory
{
  private static readonly object padlock = new();
  private static OBDSimulator _instance;

  public OBDSimulatorFactory(IConfiguration cfg, ILoggerFactory logFact)
  {
    lock (padlock)
    {
      if (_instance == null)
      {
        _instance = new OBDSimulator(cfg["OBD:Port"], logFact.CreateLogger<OBDSimulator>());
      }
    }
  }

  public OBDSimulator Instance => _instance;
}

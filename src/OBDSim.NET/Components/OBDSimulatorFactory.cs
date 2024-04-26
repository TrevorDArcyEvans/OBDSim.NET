namespace OBDSim.NET.Components;

public sealed class OBDSimulatorFactory
{
  private readonly IConfiguration _cfg;
  private readonly ILoggerFactory _logFact;

  public OBDSimulatorFactory(IConfiguration cfg, ILoggerFactory logFact)
  {
    _cfg = cfg;
    _logFact = logFact;
  }

  public OBDSimulator Create()
  {
    var logger = _logFact.CreateLogger<OBDSimulator>();
    return new OBDSimulator(_cfg["OBD:Port"], logger);
  }
}

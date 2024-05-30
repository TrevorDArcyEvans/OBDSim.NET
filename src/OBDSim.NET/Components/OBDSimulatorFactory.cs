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
        var comPort = cfg["OBD:Port"];
        var baudRate = int.Parse(cfg["OBD:BaudRate"]);
        var serialPort = new ObdSerialPort(comPort, baudRate, logFact.CreateLogger<ObdSerialPort>());

        _instance = new OBDSimulator(serialPort, logFact.CreateLogger<OBDSimulator>());
      }
    }
  }

  public OBDSimulator Instance => _instance;
}

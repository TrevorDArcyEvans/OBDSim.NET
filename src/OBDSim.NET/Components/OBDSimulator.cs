namespace OBDSim.NET.Components;

using System.IO.Ports;

public sealed class OBDSimulator : IDisposable
{
  private readonly SerialPort _serialPort;
  private readonly ILogger<OBDSimulator> _logger;

  public OBDSimulator(string port, ILogger<OBDSimulator> logger)
  {
    _logger = logger;

    _logger.LogInformation("Available Ports:");
    foreach (var s in SerialPort.GetPortNames())
    {
      _logger.LogInformation($"   {s}");
    }

    _serialPort = new SerialPort(port)
    {
      BaudRate = 9600,
      Parity = Parity.None,
      StopBits = StopBits.One,
      DataBits = 8,
      Handshake = Handshake.None
    };

    _serialPort.DataReceived += DataReceivedHandler;

    _serialPort.Open();

    _logger.LogInformation($"Opened port: {port}");
  }

  private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
  {
    var sp = (SerialPort)sender;
    var inData = sp.ReadExisting();
    _logger.LogInformation($"--> {inData}");
  }

  public void Dispose()
  {
    _serialPort.Dispose();
  }
}
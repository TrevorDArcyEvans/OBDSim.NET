namespace OBDSim.NET;

using System.IO.Ports;
using Microsoft.Extensions.Logging;

public sealed class ObdSerialPort : IObdSerialPort
{
  private readonly SerialPort _serialPort;
  private readonly ILogger<ObdSerialPort> _logger;

  private string _readExisting = string.Empty;

  public event SerialDataReceivedEventHandler DataReceived;

  public ObdSerialPort(string comPort, int baudRate, ILogger<ObdSerialPort> logger)
  {
    _serialPort = new SerialPort(comPort)
    {
      BaudRate = baudRate,
      Parity = Parity.None,
      StopBits = StopBits.One,
      DataBits = 8,
      Handshake = Handshake.None
    };
    _serialPort.DataReceived += OnDataReceived;
    _logger = logger;
  }

  private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
  {
    var sp = (SerialPort)sender;
    _readExisting = sp.ReadExisting();
    _logger.LogInformation($">-- {_readExisting}");
    DataReceived(this, e);
  }

  public void Open()
  {
    try
    {
      _serialPort.Open();
    }
    catch (Exception e)
    {
      _logger.LogError(e, $"Failed to open port: {_serialPort.PortName}");
      throw;
    }

    _logger.LogInformation($"Opened port: {_serialPort.PortName}");
  }

  public void Write(string data)
  {
    // strip off CR-LF for clarity
    _logger.LogInformation($"--> {data.Replace("\r", "").Replace("\n", "")}");
    _serialPort.Write(data);
  }

  public string ReadExisting()
  {
    var retVal = _readExisting;
    _readExisting = string.Empty;
    return retVal;
  }

  public void Dispose()
  {
    _serialPort.Dispose();
  }
}

namespace OBDSim.NET.Components;

using System.IO.Ports;

public sealed class ObdSerialPort : IObdSerialPort
{
  private readonly SerialPort _serialPort;
  private readonly ILogger<ObdSerialPort> _logger;

  public event SerialDataReceivedEventHandler DataReceived
  {
    add => _serialPort.DataReceived += value;

    remove => _serialPort.DataReceived -= value;
  }

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
    var inData = sp.ReadExisting();
    _logger.LogInformation($">-- {inData}");
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
    _logger.LogInformation($"--> {data}");
    _serialPort.Write(data);
  }

  public void Dispose()
  {
    _serialPort.Dispose();
  }
}

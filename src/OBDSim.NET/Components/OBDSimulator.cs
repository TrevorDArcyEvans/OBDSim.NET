namespace OBDSim.NET.Components;

using System.IO.Ports;

public sealed class OBDSimulator : IDisposable
{
  private readonly SerialPort _serialPort;

  public OBDSimulator(string port)
  {
    Console.WriteLine("Available Ports:");
    foreach (var s in SerialPort.GetPortNames())
    {
      Console.WriteLine($"   {s}");
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

    Console.WriteLine($"Opened port: {port}");
  }

  private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
  {
    var sp = (SerialPort)sender;
    var inData = sp.ReadExisting();
    Console.WriteLine($"--> {inData}");
  }

  public void Dispose()
  {
    _serialPort.Dispose();
  }
}

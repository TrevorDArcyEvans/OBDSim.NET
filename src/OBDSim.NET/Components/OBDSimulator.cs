namespace OBDSim.NET.Components;

using System.IO.Ports;

public sealed class OBDSimulator : IDisposable
{
  private readonly SerialPort _serialPort;
  private readonly ILogger<OBDSimulator> _logger;

  /// <summary>
  /// How much randomisation to apply as a percentage [0-100]
  /// of the nominal value
  /// </summary>
  public int JitterPercent { get; set; } = 6;

  /// <summary>
  /// Speed in km/hr [0 - 255]
  /// </summary>
  public uint Speed { get; set; } = 50;

  /// <summary>
  /// Engine coolant temperature in deg Celsius [-40 - 215]
  /// </summary>
  public int EngineTemperature { get; set; } = 75;

  /// <summary>
  /// Engine speed in RPM [0 -  16,383.75]
  /// </summary>
  public uint RPM { get; set; } = 900;

  /// <summary>
  /// Throttle position in percent [0 - 100]
  /// </summary>
  public uint ThrottlePosition { get; set; } = 15;

  /// <summary>
  /// Calculated engine load in percent [0 - 100]
  /// </summary>
  public uint CalculatedEngineLoadValue { get; set; } = 20;

  /// <summary>
  /// Fuel pressure in kPa [0 - 765]
  /// </summary>
  public uint FuelPressure { get; set; } = 540;

  /// <summary>
  /// Monitor status since DTCs cleared.
  /// True if MIL is ON; False if MIL is off.
  /// </summary>
  public bool MalfunctionIndicatorLamp { get; set; } = false;

  /// <summary>
  /// List of supported modes
  /// <see cref="http://en.wikipedia.org/wiki/OBD-II_PIDs#Mode_1_PID_03"/>
  /// </summary>
  public enum Mode
  {
    Unknown = 0x00,
    CurrentData = 0x01,
    FreezeFrameData = 0x02,
    DiagnosticTroubleCodes = 0x03
  }

  /// <summary>
  /// List of supported PIDs
  /// <see cref="http://en.wikipedia.org/wiki/OBD-II_PIDs#Mode_1_PID_03"/>
  /// </summary>
  public enum PID
  {
    Unknown = 0x0,
    MIL = 0x01,
    DTCCount = 0x01,
    Speed = 0x0D,
    EngineTemperature = 0x05,
    RPM = 0x0C,
    ThrottlePosition = 0x11,
    CalculatedEngineLoadValue = 0x04,
    FuelPressure = 0x0A
  };

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
    var sp = (SerialPort) sender;
    var inData = sp.ReadExisting();
    _logger.LogInformation($"--> {inData}");

    if (inData.Length != 4)
    {
      return;
    }

    SendResponse(inData);
  }

  private void SendResponse(string inData)
  {
    var pidStr = inData.Substring(2, 2);
    var pid = (PID) Convert.FromHexString(pidStr).Single();
    switch (pid)
    {
      case PID.Unknown:
        break;

      case PID.MIL:
        SendMIL();
        break;

      case PID.Speed:
        SendSpeed();
        break;
      
      case PID.EngineTemperature:
        SendEngineTemperature();
        break;
      
      case PID.RPM:
         SendRPM();
       break;
      
      case PID.ThrottlePosition:
         SendThrottlePosition();
       break;
      
      case PID.CalculatedEngineLoadValue:
        SendCalculatedEngineLoadValue();
        break;
      
      case PID.FuelPressure:
         SendFuelPressure();
       break;
    }
  }

  private void SendMIL()
  {
    throw new NotImplementedException();
  }

  private void SendSpeed()
  {
    throw new NotImplementedException();
  }

  private void SendEngineTemperature()
  {
    throw new NotImplementedException();
  }

  private void SendRPM()
  {
    throw new NotImplementedException();
  }
  
  private void SendThrottlePosition()
  {
    throw new NotImplementedException();
  }

  private void SendCalculatedEngineLoadValue()
  {
    throw new NotImplementedException();
  }

  private void SendFuelPressure()
  {
    throw new NotImplementedException();
  }

  public void Dispose()
  {
    _serialPort.Dispose();
  }
}

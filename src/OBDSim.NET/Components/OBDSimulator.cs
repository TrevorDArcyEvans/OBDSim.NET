namespace OBDSim.NET.Components;

using System.IO.Ports;
using System.Numerics;

public sealed class OBDSimulator : IDisposable
{
  private readonly SerialPort _serialPort;
  private readonly ILogger<OBDSimulator> _logger;

  /// <summary>
  /// How much randomisation (+/-) to apply as a percentage [0-100]
  /// of the nominal value.  Output value will be over a spread of
  /// 2x JitterPercent.
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
    var sp = (SerialPort)sender;
    var inData = sp.ReadExisting();
    _logger.LogInformation($"--> {inData}");

    SendResponse(inData);
  }

  private void SendResponse(string inData)
  {
    switch (inData.ToUpperInvariant())
    {
      // reset
      case "ATZ":
        SendReset();
        break;

      // PID.MIL
      case "0101":
        SendMIL();
        break;

      // PID.Speed
      case "010D":
        SendSpeed();
        break;

      // PID.EngineTemperature
      case "0105":
        SendEngineTemperature();
        break;

      // PID.RPM
      case "010C":
        SendRPM();
        break;

      // PID.ThrottlePosition
      case "0111":
        SendThrottlePosition();
        break;

      // PID.CalculatedEngineLoadValue
      case "0104":
        SendCalculatedEngineLoadValue();
        break;

      // PID.FuelPressure
      case "010A":
        SendFuelPressure();
        break;

      default:
        SendNoData();
        break;
    }
  }

  private void SendNoData()
  {
    var dataStr = $"\nNO DATA \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendReset()
  {
    var dataStr = $"\nOBDSim.NET \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendMIL()
  {
    var dataA = (MalfunctionIndicatorLamp ? 128 : 127).ToString("x2");
    var dataB = 112.ToString("x2");
    var dataC = 157.ToString("x2");
    var dataD = 32.ToString("x2");
    var dataStr = $"\n01 01 {dataA} {dataB} {dataC} {dataD} \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendSpeed()
  {
    var simVal = GetSimulatedValue<uint>(Speed, 0, 255);
    var dataA = simVal.ToString("x2");
    var dataStr = $"\n01 0d {dataA} \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendEngineTemperature()
  {
    var simVal = GetSimulatedValue<int>(EngineTemperature, -40, 215);
    var dataA = (simVal + 40).ToString("x2");
    var dataStr = $"\n01 05 {dataA} \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendRPM()
  {
    var simVal = GetSimulatedValue<uint>(RPM, 0, 16383);
    var rpmA = simVal / 64;
    var rpmB = simVal * 4 - 256 * rpmA;
    var rpmAstr = rpmA.ToString("x2");
    var rpmBstr = rpmB.ToString("x2");
    var dataStr = $"\n01 0c {rpmAstr} {rpmBstr} \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendThrottlePosition()
  {
    var simVal = GetSimulatedValue<uint>(ThrottlePosition, 0, 100);
    var expStr = ((int)Math.Round(simVal / 100d * 255d)).ToString("x2");
    var dataStr = $"\n01 11 {expStr} \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendCalculatedEngineLoadValue()
  {
    var simVal = GetSimulatedValue<uint>(CalculatedEngineLoadValue, 0, 100);
    var expStr = ((int)Math.Round(simVal / 100d * 255d)).ToString("x2");
    var dataStr = $"\n01 04 {expStr} \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendFuelPressure()
  {
    var simVal = GetSimulatedValue<uint>(FuelPressure, 0, 765);
    var dataA = ((int)Math.Round(simVal / 3d)).ToString("x2");
    var dataStr = $"\n01 0a {dataA} \r\n>";

    _serialPort.Write(dataStr);
  }

  private int RandomSign()
  {
    var num = Random.Shared.Next(-100, 100);
    return num / Math.Abs(num);
  }

  private T GetSimulatedValue<T>(T nominal, T min, T max) where T : INumber<T>
  {
    var sign = RandomSign();
    var raw = nominal + T.CreateChecked(sign * JitterPercent / 100d);
    var clamped = T.Clamp(raw, min, max);
    return clamped;
  }

  public void Dispose()
  {
    _serialPort.Dispose();
  }
}

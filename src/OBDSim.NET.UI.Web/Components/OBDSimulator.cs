namespace OBDSim.NET.UI.Web.Components;

using System.IO.Ports;
using System.Numerics;

public sealed class OBDSimulator : IDisposable
{
  private readonly IObdSerialPort _serialPort;
  private readonly ILogger<OBDSimulator> _logger;

  /// <summary>
  /// How much randomisation (+/-) to apply as a percentage [0-100]
  /// of the nominal value.  Output value will be over a spread of
  /// 2x JitterPercent.
  /// </summary>
  public int JitterPercent { get; set; } = 6;

  /// <summary>
  /// Vehicle speed in km/hr [0 - 255]
  /// </summary>
  public uint VehicleSpeed { get; set; } = 50;

  /// <summary>
  /// Engine coolant temperature in deg Celsius [-40 - 215]
  /// </summary>
  public int EngineCoolantTemperature { get; set; } = 75;

  /// <summary>
  /// Engine speed in RPM [0 -  16,383.75]
  /// </summary>
  public uint EngineSpeed { get; set; } = 900;

  /// <summary>
  /// Throttle position in percent [0 - 100]
  /// </summary>
  public uint ThrottlePosition { get; set; } = 15;

  /// <summary>
  /// Calculated engine load in percent [0 - 100]
  /// </summary>
  public uint CalculatedEngineLoad { get; set; } = 20;

  /// <summary>
  /// Fuel pressure in kPa [0 - 765]
  /// </summary>
  public uint FuelPressure { get; set; } = 540;

  /// <summary>
  /// Monitor status since DTCs cleared.
  /// True if MIL is ON; False if MIL is off.
  /// </summary>
  public bool DTCCount { get; set; } = false;

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
    DTCCount = 0x01,
    FuelSystemStatus = 0x03,
    CalculatedEngineLoad = 0x04,
    EngineCoolantTemperature = 0x05,
    ShortTermFuelTrim = 0x06,
    LongTermFuelTrim = 0x07,
    FuelPressure = 0x0A,
    EngineSpeed = 0x0C,
    VehicleSpeed = 0x0D,
    TimingAdvance = 0x0E,
    IntakeAirTemperature = 0x0F,
    MassAirFlow = 0x10,
    ThrottlePosition = 0x11,
    OxygenSensorsPresent = 0x13,
    OxygenSensor2 = 0x15,
    OBDstandards = 0x1C,
    RunTimeSinceEngineStart = 0x1F,
    PIDsupported = 0x20,
  };

  public OBDSimulator(IObdSerialPort serialPort, ILogger<OBDSimulator> logger)
  {
    _logger = logger;

    _logger.LogInformation("Available Ports:");
    foreach (var s in SerialPort.GetPortNames())
    {
      _logger.LogInformation($"   {s}");
    }

    _serialPort = serialPort;

    _serialPort.DataReceived += DataReceivedHandler;

    _serialPort.Open();
  }

  private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
  {
    var sp = (IObdSerialPort)sender;
    var inData = sp.ReadExisting();

    SendResponse(inData);
  }

  private void SendResponse(string inData)
  {
    var trimData = inData.ToUpperInvariant().TrimEnd('\r', '\n');
    switch (trimData)
    {
      // reset
      case "ATZ":
        SendReset();
        break;

      case "ATE0": // echo OFF
      case "ATH1": // headers ON
      case "ATL0": // linefeeds OFF
      case "ATSP0": // set auto protocol 
        SendOK();
        break;

      case "ATI":
        SendVersion();
        break;

      case "ATRV":
        SendVoltage();
        break;

      case "ATDPN":
        SendDescribeProtocolByNumber();
        break;

      case "0100":
        SendSupportedPIDs();
        break;

      case "0600":
        SendNoSupportedPIDs_600();
        break;

      case "0900":
        SendNoSupportedPIDs_900();
        break;

      // PID.DTCCount
      case "0101":
        SendDTCCount();
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

  private void SendDescribeProtocolByNumber()
  {
    // 6 -->  ISO 15765-4 (CAN 11/500)
    var dataStr = $"\nA6 \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendVoltage()
  {
    var dataStr = $"\n12.5V \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendVersion()
  {
    var dataStr = $"\nv1.0.0 \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendNoData()
  {
    var dataStr = $"\nNO DATA \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendReset()
  {
    // PyOBD looks for 'ELM' in response
    var dataStr = $"\nOBDSim.NET (ELM Simulator)\r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendOK()
  {
    var dataStr = $"\nOK \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendDTCCount()
  {
    var dataA = (DTCCount ? 128 : 127).ToString("x2");
    var dataB = 112.ToString("x2");
    var dataC = 157.ToString("x2");
    var dataD = 32.ToString("x2");
    var dataStr = $"\n01 01 {dataA} {dataB} {dataC} {dataD} \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendSpeed()
  {
    var simVal = GetSimulatedValue<uint>(VehicleSpeed, 0, 255);
    var dataA = simVal.ToString("x2");
    var dataStr = $"\n01 0d {dataA} \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendEngineTemperature()
  {
    var simVal = GetSimulatedValue<int>(EngineCoolantTemperature, -40, 215);
    var dataA = (simVal + 40).ToString("x2");
    var dataStr = $"\n01 05 {dataA} \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendRPM()
  {
    var simVal = GetSimulatedValue<uint>(EngineSpeed, 0, 16383);
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
    var simVal = GetSimulatedValue<uint>(CalculatedEngineLoad, 0, 100);
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

  private void SendSupportedPIDs()
  {
    var simVal = GetSupportedPIDs();
    var dataStr = $"\n01 00 {simVal} \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendNoSupportedPIDs_600()
  {
    var simVal = "00000000";
    var dataStr = $"\n06 00 {simVal} \r\n>";

    _serialPort.Write(dataStr);
  }

  private void SendNoSupportedPIDs_900()
  {
    var simVal = "00000000";
    var dataStr = $"\n09 00 {simVal} \r\n>";

    _serialPort.Write(dataStr);
  }

  private static string GetSupportedPIDs()
  {
    // response is 4 bytes ie 32 chars, but arrays are zero based,
    // so we allocate 33 chars and ignore the first char
    var data = new char[33];

    for (var i = 0; i < data.Length; i++)
    {
      data[i] = '0';
    }

    data[(int)PID.DTCCount] = '1';
    data[(int)PID.FuelSystemStatus] = '1';
    data[(int)PID.CalculatedEngineLoad] = '1';
    data[(int)PID.EngineCoolantTemperature] = '1';
    data[(int)PID.EngineSpeed] = '1';
    data[(int)PID.VehicleSpeed] = '1';
    data[(int)PID.ThrottlePosition] = '1';

    // remove leading char, so are now dealing with
    // a one based array
    data = data.Skip(1).ToArray();

    var hexStr = string.Empty;

    // 4 bytes in response
    for (var i = 0; i < 4; i++)
    {
      // skip to each byte
      var binStr = new string(data.Skip(i * 8).Take(8).ToArray());
      var num = Convert.ToInt32(binStr, 2);
      hexStr += num.ToString("X2");
    }

    return hexStr;
  }

  private int RandomSign()
  {
    var num = Random.Shared.Next(-100, 100);
    num = num == 0 ? 1 : num;
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

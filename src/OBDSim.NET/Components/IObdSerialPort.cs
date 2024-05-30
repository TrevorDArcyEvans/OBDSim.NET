namespace OBDSim.NET.Components;

using System.IO.Ports;

public interface IObdSerialPort
{
  void Open();
  void Write(string data);
  event SerialDataReceivedEventHandler DataReceived;
  void Dispose();
}

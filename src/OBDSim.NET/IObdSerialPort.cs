namespace OBDSim.NET;

using System.IO.Ports;

public interface IObdSerialPort
{
  void Open();
  void Write(string data);
  event SerialDataReceivedEventHandler DataReceived;
  string ReadExisting();
  void Dispose();
}

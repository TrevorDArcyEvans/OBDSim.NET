using Microsoft.AspNetCore.Components;

namespace OBDSim.NET.Components.Pages;

public partial class Home
{
  [Inject]
  private OBDSimulator _obd { get; set; }
}

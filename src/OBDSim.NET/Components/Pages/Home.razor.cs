namespace OBDSim.NET.Components.Pages;

using Microsoft.AspNetCore.Components;

public partial class Home
{
  [Inject]
  private OBDSimulatorFactory _obdFact { get; set; }

  private OBDSimulator _obd;

  protected override void OnInitialized()
  {
    _obd = _obdFact.Create();

    base.OnInitialized();
  }
}

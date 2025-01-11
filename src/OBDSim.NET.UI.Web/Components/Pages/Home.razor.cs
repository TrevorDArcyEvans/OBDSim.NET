using Microsoft.AspNetCore.Components;

namespace OBDSim.NET.UI.Web.Components.Pages;

public partial class Home
{
  [Inject]
  private OBDSimulatorFactory _obdFact { get; set; }

  private OBDSimulator _obd;

  protected override void OnInitialized()
  {
    _obd = _obdFact.Instance;

    base.OnInitialized();
  }
}

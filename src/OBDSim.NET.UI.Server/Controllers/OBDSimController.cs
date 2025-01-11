namespace OBDSim.NET.UI.Server.Controllers;

using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class OBDSimController : ControllerBase
{
  private readonly ILogger<OBDSimController> _logger;
  private readonly OBDSimulator _obd;

  public OBDSimController(OBDSimulatorFactory obdFact, ILogger<OBDSimController> logger)
  {
    _logger = logger;
    _obd = obdFact.Instance;
  }

  // GET: api/<OBDSimController>
  [HttpGet]
  public IEnumerable<string> Get()
  {
    return new string[] { "value1", "value2" };
  }

  // GET api/<OBDSimController>/5
  [HttpGet("{id}")]
  public string Get(int id)
  {
    return "value";
  }

  // POST api/<OBDSimController>
  [HttpPost]
  public void Post([FromBody] string value)
  {
  }

  // PUT api/<OBDSimController>/5
  [HttpPut("{id}")]
  public void Put(int id, [FromBody] string value)
  {
  }

  // DELETE api/<OBDSimController>/5
  [HttpDelete("{id}")]
  public void Delete(int id)
  {
  }
}

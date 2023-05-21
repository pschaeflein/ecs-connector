namespace ChuckNorrisConnector.Models
{
  public class FlowTriggerEndpoint
  {
    public long Id { get; set; }
    public required string FlowInstanceId { get; set; }
    public required string FlowName { get; set; }
    public required string Endpoint { get; set; }
  }
}

using Microsoft.EntityFrameworkCore;

namespace ChuckNorrisConnector.Models
{
  public class FlowManagementContext:DbContext
  {
    public FlowManagementContext(DbContextOptions<FlowManagementContext> options) : base(options) { }

    public DbSet<FlowTriggerEndpoint> FlowTriggerEndpoints { get; set; }  
  }
}

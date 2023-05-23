using ChuckNorrisConnector.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text;

namespace ChuckNorrisConnector.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Consumes("application/json")]
  [Produces("application/json")]
  public class ChuckNorrisJokesController : ControllerBase
  {
    //private const string UNSUBSCRIBE_URL = "https://[dev-tunnel-address]/api/ChuckNorrisJokes/Unsubscribe/{0}";
    private const string UNSUBSCRIBE_URL = "https://gw6z58x5-7007.uks1.devtunnels.ms/api/ChuckNorrisJokes/Unsubscribe/{0}";

    private readonly IConfiguration _configuration;
    private readonly FlowManagementContext _flowManagementContext;
    private readonly ChuckNorrisIOService _service;

    public ChuckNorrisJokesController(
      IConfiguration configuration,
      FlowManagementContext flowManagementContext,
      ChuckNorrisIOService service)
    {
      _configuration = configuration;
      _flowManagementContext = flowManagementContext;
      _service = service;
    }

    // Saves the flow trigger endpoint in Memory to be able to send request to the flow
    [HttpPost("Subscribe", Name ="Subscribe")]
    public async Task<IActionResult> SubscribeTriggerUrl(SubscribeRequest subscribeRequest)
    {
      var instanceId = Guid.NewGuid().ToString();
      _flowManagementContext.FlowTriggerEndpoints.Add(
          new FlowTriggerEndpoint
          {
            FlowInstanceId = instanceId,
            FlowName = subscribeRequest.FlowName,
            Endpoint = subscribeRequest.FlowTriggerEndpoint
          });

      await _flowManagementContext.SaveChangesAsync();

      Response.Headers.Location = string.Format(UNSUBSCRIBE_URL, instanceId);
      return Ok(new { instanceId });
    }

    // Remove the flow trigger endpoint if the flow has been deleted
    [HttpDelete("Unsubscribe/{flowInstanceId}", Name ="Unsubscribe")]
    public async Task<IActionResult> UnsubscribeTriggerUrl(string flowInstanceId)
    {
      // remove the url from the dictionary 
      var flowEndpoint = await _flowManagementContext.FlowTriggerEndpoints
                                .FirstOrDefaultAsync(endpoint => endpoint.FlowInstanceId == flowInstanceId);
      if (flowEndpoint != null)
      {
        _flowManagementContext.FlowTriggerEndpoints.Remove(flowEndpoint);
        await _flowManagementContext.SaveChangesAsync();
        await _flowManagementContext.SaveChangesAsync();

        return Ok();
      }
      else
      {
        return BadRequest("Unsubscribe Error: Flow trigger endpoint was not found");
      }

    }

    /// <summary>
    /// Get a moderated list of joke categories
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet("categories",Name ="Categories")]
    [SwaggerOperation(
      Summary = "Creates a new product",
      Description = "Requires admin privileges",
      OperationId = "CreateProduct")]
    public async Task<IActionResult> GetCategories()
    {
      var categories = await _service.GetCategories();
      categories.Remove("explicit");

      return Ok(categories);
    }

    /// <summary>
    /// Get a moderated joke in the specified category
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("joke", Name ="Joke")]
    [ProducesResponseType(typeof(Joke), 200)]
    public async Task<IActionResult> GetJoke([FromQuery] string? category = null)
    {
      var joke = await _service.GetJoke(category);

      if (joke == null || string.IsNullOrEmpty(joke.Value))
      {
        return Problem(statusCode: 500, title: "Error calling Chuck...");
      }

      // Run thru text moderation...
      string SubscriptionKey = _configuration["AzureContentModerator:SubscriptionKey"];
      string Endpoint = _configuration["AzureContentModerator:Endpoint"];

      if (string.IsNullOrEmpty(SubscriptionKey) || string.IsNullOrEmpty(Endpoint))
      {
        return Problem(statusCode: 500, title: "Configuration error");
      }

      ContentModeratorClient client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(SubscriptionKey));
      client.Endpoint = Endpoint;

      var moderatedJoke = await client.TextModeration.ScreenTextAsync("text/plain", new MemoryStream(Encoding.UTF8.GetBytes(joke.Value)), "eng", true, true, null, true);

      if (moderatedJoke.Terms?.Count> 0)
      {
        foreach (var term in moderatedJoke.Terms)
        {
          var replacement = new string('*', term.Term.Length);
          joke.Value = joke.Value.Replace(term.Term, replacement);
          joke.Moderated = true;
        }
      }

      return Ok(joke);
    }
  }
}

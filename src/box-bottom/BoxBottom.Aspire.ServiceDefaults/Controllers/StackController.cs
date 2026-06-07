using BoxBottom.Aspire.ServiceDefaults;
using Microsoft.AspNetCore.Mvc;

namespace BoxBottom.Aspire.ServiceDefaults.Controllers;

public sealed record StackNameResponse(string StackName);

[ApiController]
[Route("api")]
public class StackController(StackProperties stackProperties) : ControllerBase
{
    [HttpGet("StackName", Order = -1000)]
    public ActionResult<StackNameResponse> GetStackName() =>
        Ok(new StackNameResponse(stackProperties.StackName));
}

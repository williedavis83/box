using Microsoft.AspNetCore.Mvc;

namespace Foo.Primary.Api.Controllers;

[ApiController]
[Route("api")]
public class HelloController : ControllerBase
{
    [HttpGet("Hello")]
    public IActionResult Hello() => Ok("Hello");
}

#pragma warning disable ASP013 // Name the controller to match the route.
namespace ValidCode;

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

[Route("/api/values/{id}")]
[Route("/api/values")]
[ApiController]
public class WithTwoRoutesOnClassController : ControllerBase
{
    // GET api/values
    [HttpGet]
    public ActionResult<IEnumerable<string>> Get()
    {
        return new[] { "value1", "value2" };
    }

    [HttpGet]
    public ActionResult<string> Get(int id)
    {
        return $"'{id}'";
    }

    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/values/5
    [HttpPut]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/values/5
    [HttpDelete]
    public void Delete(int id)
    {
    }
}

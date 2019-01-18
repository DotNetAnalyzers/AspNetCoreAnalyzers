#pragma warning disable ASP012 // Don't use [controller].
namespace ValidCode
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;

    [Route("/api/[controller]/{id}")]
    [Route("/api/[controller]")]
    [ApiController]
    public class WithBracketControllerController : ControllerBase
    {
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

        [HttpPut]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete]
        public void Delete(int id)
        {
        }
    }
}

namespace ValidCode
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;

    [Route("/api/[controller]({id})")]
    [Route("/api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
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
}

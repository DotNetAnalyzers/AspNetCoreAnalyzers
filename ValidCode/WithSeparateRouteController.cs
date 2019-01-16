namespace ValidCode
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class WithSeparateRouteController : Controller
    {
        [Route("api/values/{id}")]
        [HttpGet]
        public IActionResult GetValue(string id)
        {
            return this.Ok(id);
        }

        [Route("api/values")]
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        [Route("api/values/{id}")]
        [HttpPut]
        public void Put(int id, [FromBody] string value)
        {
        }

        [Route("api/values/{id}")]
        [HttpDelete]
        public void Delete(int id)
        {
        }
    }
}

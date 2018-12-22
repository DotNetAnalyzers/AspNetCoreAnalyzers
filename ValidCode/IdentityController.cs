namespace ValidCode
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class IdentityController : Controller
    {
        [HttpGet("string/{text:minlength(1):maxlength(2)}")]
        public IActionResult GetString(string text)
        {
            return this.Ok(text);
        }
    }
}

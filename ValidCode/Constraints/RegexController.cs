namespace ValidCode.Constraints
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class RegexController : Controller
    {
        [HttpGet("string1/{value:regex(a{{0,1}})}")]
        public IActionResult Get1(string value)
        {
            return this.Ok(value);
        }

        [HttpGet("string2/{value:regex(\\\\d+)}")]
        public IActionResult Get2(string value)
        {
            return this.Ok(value);
        }

        [HttpGet("string3/{value:regex(^\\\\d{{3}}-\\\\d{{2}}-\\\\d{{4}}$)}")]
        public IActionResult Get3(string value)
        {
            return this.Ok(value);
        }
    }
}

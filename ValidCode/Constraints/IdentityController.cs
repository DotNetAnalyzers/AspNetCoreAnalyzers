namespace ValidCode.Constraints;

using System;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class IdentityController : Controller
{
    [HttpGet("ints/{value:int:min(1):max(2):range(3)}")]
    public IActionResult Get(int value)
    {
        return this.Ok(value);
    }

    [HttpGet("bools/{value}")]
    public IActionResult Get(bool value)
    {
        return this.Ok(value);
    }

    [HttpGet("maybe-bools/{value?}")]
    public IActionResult Get(bool? value)
    {
        return this.Ok(value);
    }

    [HttpGet("times/{value:datetime}")]
    public IActionResult Get(DateTime value)
    {
        return this.Ok(value);
    }

    [HttpGet("decimals/{value:decimal}")]
    public IActionResult Get(decimal value)
    {
        return this.Ok(value);
    }

    [HttpGet("doubles/{value:double}")]
    public IActionResult Get(double value)
    {
        return this.Ok(value);
    }

    [HttpGet("floats/{value:float}")]
    public IActionResult Get(float value)
    {
        return this.Ok(value);
    }

    [HttpGet("times/{value:guid}")]
    public IActionResult Get(Guid value)
    {
        return this.Ok(value);
    }

    [HttpGet("longs/{value:long}")]
    public IActionResult Get(long value)
    {
        return this.Ok(value);
    }

    [HttpGet("strings/{value:minlength(1):maxlength(2):required:alpha:regex(a{{0,1}})}")]
    public IActionResult Get(string value)
    {
        return this.Ok(value);
    }
}

using App.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;
public abstract class ApiController : ControllerBase
{
    protected IActionResult ApiReturn<T>(T? data) => data switch
    {
        IApiResult result => HandleApiResult(result),
        ValidationResult validation => HandleValidation(validation),
        _ => HandleResult(data)
    };

    IActionResult HandleValidation(ValidationResult result) =>
        result.IsValid
            ? Ok(result)
            : BadRequest(result.Message);

    IActionResult HandleApiResult(IApiResult result) =>
        result.Error
            ? BadRequest(result.Message)
            : result.HasData
                ? Ok(result)
                : NotFound(result);

    IActionResult HandleResult<T>(T? result) =>
        result is null
            ? NotFound(result)
            : Ok(result);    
}
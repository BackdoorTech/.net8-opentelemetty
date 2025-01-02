using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

public class ValidateModelAttribute : ActionFilterAttribute
{

  public override void OnActionExecuting(ActionExecutingContext context)
  {
    Console.WriteLine("nuice");
    if (!context.ModelState.IsValid)
    {
      // Extract validation errors
      var validationErrors = context.ModelState.Values
          .SelectMany(v => v.Errors)
          .Select(e => e.ErrorMessage)
          .ToArray();

      var errorResponse = new
      {
        StatusCode = 500,
        Message = "Validation failed",
        Errors = validationErrors
      };

      context.Result = new ObjectResult(errorResponse)
      {
        StatusCode = 500
      };
    }
  }
}

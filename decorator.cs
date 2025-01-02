using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

public class ValidateModelAttribute : ActionFilterAttribute
{

   private readonly IErrorLoggingService _errorLoggingService; // Inject the error logging service

  public ValidateModelAttribute(IErrorLoggingService errorLoggingService)
  {
    _errorLoggingService = errorLoggingService;
  }

  public override void OnActionExecuting(ActionExecutingContext context)
  {
      if (!context.ModelState.IsValid)
      {
          // Extract validation errors
          var validationErrors = context.ModelState.Values
              .SelectMany(v => v.Errors)
              .Select(e => e.ErrorMessage)
              .ToArray();

          var errorResponse = new
          {
              StatusCode = 400, // Use 400 for Bad Request
              Message = "Validation failed",
              Errors = validationErrors
          };

          // Log the error details to the database
          var method = context.HttpContext.Request.Method;
          var endpoint = context.HttpContext.Request.Path;
          var payload = context.ActionArguments;  // This will contain the payload sent in the request

          _errorLoggingService.LogValidationError(method, endpoint, payload, string.Join(", ", validationErrors));


          context.Result = new ObjectResult(errorResponse)
          {
              StatusCode = 500
          };
      }
  }
}

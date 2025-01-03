
using System.Diagnostics;

class ActivityManagerR : IActivityManagerR {
  // Retrieve the current Activity at the time of access
  private  Activity CurrentActivity => Activity.Current;

  // Example Method to Add Tags or Metadata to the Current Activity
  public void AddTag(string key, string value)
  {
    CurrentActivity?.AddTag(key, value);
  }


  // Retrieve Trace ID of the Current Activity
  public string GetTraceId()
  {
    return CurrentActivity?.TraceId.ToString() ?? "no-trace-id";
  }

    // Retrieve Span ID of the Current Activity
  public string GetSpanId()
  {
    return CurrentActivity?.SpanId.ToString() ?? "no-span-id";
  }

  // Add or update a tag in the current Activity
  public void SetTag(string key, string value)
  {
    if (CurrentActivity != null)
    {
      // If the tag already exists, it will be updated
      CurrentActivity.SetTag(key, value);
    }
  }
}

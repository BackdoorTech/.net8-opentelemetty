public interface IActivityManagerR
{
  // Retrieve the current Trace ID
  string GetTraceId();

  // Retrieve the current Span ID
  string GetSpanId();

  // Add a new tag to the current Activity
  void AddTag(string key, string value);

  // Add or update a tag in the current Activity
  void SetTag(string key, string value);
}

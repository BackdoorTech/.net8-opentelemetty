public class ErrorLog
{
  public int Id { get; set; }
  public required string Method { get; set; }
  public required string Endpoint { get; set; }
  public required string Payload { get; set; }
  public required string ErrorMessage { get; set; }
  public DateTime Timestamp { get; set; }
}

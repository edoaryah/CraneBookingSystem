namespace AspnetCoreMvcFull.Events
{
  public class CraneMaintenanceEvent
  {
    public int CraneId { get; set; }
    public DateTime MaintenanceStartTime { get; set; }
    public DateTime MaintenanceEndTime { get; set; }
    public required string Reason { get; set; }
  }
}

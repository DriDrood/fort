using System;

namespace Fort.Models.Store
{
  public class Order
  {
    public string Id { get; set; }
    public Guid PlayerId { get; set; }
    public int StartSize { get; set; }
    public int EndSize { get; set; }
    public int? StartAmount { get; set; }
    public int? EndAmount { get; set; }
  }
}
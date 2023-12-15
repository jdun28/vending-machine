namespace vending_machine
{
  public class Transaction
  {
    public string[]? Items { get; set; }
    public decimal AmountPaid { get; set; }

    public DateTime Timestamp { get; set; }
  }
}
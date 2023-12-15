namespace vending_machine
{
  public interface IVendingMachine
  {
    IEnumerable<Product> GetInventory();
    void UpdateInventory(string itemName);
    void RecordTransaction(Transaction transaction);
    bool RefundTransaction(Transaction transaction);
    IEnumerable<Transaction> GetAllTransactions();
    Transaction GetTransaction(int transactionId);
  }

}
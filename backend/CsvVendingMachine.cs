using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace vending_machine
{
  public class CsvVendingMachine : IVendingMachine
  {
    private const string InventoryCsvFilePath = "inventory.csv";
    private const string LedgerCsvFilePath = "ledger.csv";

    public CsvVendingMachine()
    {
      InitializeCsvFiles();
    }

    public IEnumerable<Product> GetInventory()
    {
      return ReadInventoryCsvFile();
    }

    public Product FindProduct(string itemName)
    {
      var allInventory = ReadInventoryCsvFile();
      return allInventory.Find(product => product.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase)) ?? null;
    }

    public void UpdateInventory(string itemName)
    {
      var product = FindProduct(itemName);
      if (product != null && product.Quantity > 0)
      {
        product.Quantity--;
        WriteInventoryCsvFile(new List<Product> { product }); // Pass a list with a single product
      }
    }

    public void RecordTransaction(Transaction transaction)
    {
      var ledger = ReadLedgerCsvFile();
      ledger.Add(transaction);
      WriteLedgerCsvFile(ledger);
    }

    // For ease of project and time constraints this refund transaction is removing the
    // transaction from the ledger. Ideally, this transaction would also be recorded
    // but with a negative dollar amount and negative quantities associated.
    public bool RefundTransaction(Transaction transaction)
    {
      var ledger = ReadLedgerCsvFile();

      if (ledger.Contains(transaction))
      {
        foreach (var item in transaction.Items)
        {
          var product = FindProduct(item);
          if (product != null)
          {
            product.Quantity++;
          }
        }
        ledger.Remove(transaction);
        WriteLedgerCsvFile(ledger);
        return true;
      }
      return false;
    }

    public IEnumerable<Transaction> GetAllTransactions()
    {
      return ReadLedgerCsvFile();
    }

    public Transaction GetTransaction(int transactionId)
    {
      var ledger = ReadLedgerCsvFile();
      return (transactionId >= 0 && transactionId < ledger.Count) ? ledger[transactionId] : null;
    }

    private void InitializeCsvFiles()
    {
      if (!File.Exists(InventoryCsvFilePath))
      {
        WriteInventoryCsvFile(new List<Product>
          {
              new Product { Name = "Soda", Price = 0.95m, Quantity = 10},
              new Product { Name = "Candy Bar", Price = 0.60m, Quantity = 15 },
              new Product { Name = "Chips", Price = 0.99m, Quantity = 8 }
          });
      }

      if (!File.Exists(LedgerCsvFilePath))
      {
        WriteLedgerCsvFile(new List<Transaction>());
      }
    }

    private List<Product> ReadInventoryCsvFile()
    {
      var inventory = new List<Product>();

      using (var reader = new StreamReader(InventoryCsvFilePath))
      {
        while (!reader.EndOfStream)
        {
          var line = reader.ReadLine();
          var values = line.Split(',');

          inventory.Add(new Product
          {
            Name = values[0],
            Price = decimal.Parse(values[1], CultureInfo.InvariantCulture),
            Quantity = int.Parse(values[2])
          });
        }
      }

      return inventory;
    }

    private void WriteInventoryCsvFile(List<Product> inventory)
    {
      using (var writer = new StreamWriter(InventoryCsvFilePath))
      {
        foreach (var product in inventory)
        {
          writer.WriteLine($"{product.Name},{product.Price},{product.Quantity}");
        }
      }
    }

    private List<Transaction> ReadLedgerCsvFile()
    {
      var ledger = new List<Transaction>();

      using (var reader = new StreamReader(LedgerCsvFilePath))
      {
        while (!reader.EndOfStream)
        {
          var line = reader.ReadLine();
          var values = line.Split(',');

          ledger.Add(new Transaction
          {
            Items = values[0].Split('|').ToArray(),
            AmountPaid = decimal.Parse(values[1], CultureInfo.InvariantCulture),
            Timestamp = DateTime.Parse(values[2])
          });
        }
      }

      return ledger;
    }

    private void WriteLedgerCsvFile(List<Transaction> ledger)
    {
      using (var writer = new StreamWriter(LedgerCsvFilePath))
      {
        foreach (var transaction in ledger)
        {
          var items = string.Join('|', transaction.Items);
          writer.WriteLine($"{items},{transaction.AmountPaid},{transaction.Timestamp}");
        }
      }
    }
  }
}
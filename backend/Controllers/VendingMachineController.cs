using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace vending_machine
{
  [EnableCors("AllowAllOrigins")]
  [ApiController]
  [Route("api/[controller]")]
  public class VendingMachineController : ControllerBase
  {
    private readonly IVendingMachine _repository;

    public VendingMachineController(IVendingMachine repository)
    {
      _repository = repository;
    }

    [HttpPost("purchase")]
    public IActionResult RecordTransaction([FromBody] Transaction transaction)
    {
      try
      {
        // Make sure transaction is valid
        if (transaction == null || transaction.Items == null || transaction.Items.Length == 0)
        {
          return BadRequest(new { Message = "Invalid transaction data" });
        }

        // Figure out if there is enough stock of an item to fulfill order
        var inventory = _repository.GetInventory();
        foreach (var item in transaction.Items)
        {
          var amtAvailable = inventory.FirstOrDefault(p => p.Name.Equals(item, StringComparison.OrdinalIgnoreCase))?.Quantity;
          if (amtAvailable == null || amtAvailable.Value.Equals(0))
          {
            return BadRequest(new { Message = $"Item '{item}' is out of stock" });
          }
        }

        // Calculate total for the transaction
        var totalOwed = transaction.Items.Sum(item => inventory.First(p => p.Name.Equals(item, StringComparison.OrdinalIgnoreCase)).Price);
        if (transaction.AmountPaid < totalOwed)
        {
          return BadRequest(new { Message = "Insufficient funds" });
        }

        // Update inventory quantities
        foreach (var item in transaction.Items)
        {
          _repository.UpdateInventory(item);
        }

        // Finally, record transaction in "ledger"
        _repository.RecordTransaction(transaction);

        return Ok(new { Message = "Purchase successful", Transaction = transaction });
      }
      catch (Exception e)
      {
        return StatusCode(500, new { Message = "Internal Server Error", Error = e.Message });
      }
    }

    [HttpPost("refund")]
    public IActionResult RecordRefund([FromBody] Transaction transaction)
    {
      try
      {
        if (transaction == null || transaction.Items == null || transaction.Items.Length == 0)
        {
          return BadRequest(new { Message = "Invalid transaction data" });
        }

        // Find transaction in ledger
        var transactionFromLedger = _repository.GetAllTransactions().FirstOrDefault(t =>
          t.Items.SequenceEqual(transaction.Items) &&
          t.AmountPaid == transaction.AmountPaid
        );
        if (transactionFromLedger == null)
        {
          return NotFound(new { Message = "Unable to locate transaction" });
        }

        // RefundTransaction handles ledger actions, so we should not need to worry about that here
        var refundSuccess = _repository.RefundTransaction(transaction);
        if (refundSuccess)
        {
          return Ok(new { Message = "Refund successful", RefundedTransaction = transaction });
        }
        return BadRequest(new { Message = "Refund failed. Transaction not found in ledger." });
      }
      catch (Exception e)
      {
        return StatusCode(500, new { Message = "Internal Server Error", Error = e.Message });
      }
    }

    [HttpGet("inventory")]
    public IActionResult GetInventory()
    {
      try
      {
        var inventory = _repository.GetInventory();

        if (inventory.Any())
        {
          return Ok(new { Inventory = inventory });
        }
        return NotFound(new { Message = "Inventory not found " });
      }
      catch (Exception e)
      {
        return StatusCode(500, new { Message = "Internal Server Error", Error = e.Message });
      }
    }

    [HttpGet("ledger")]
    public IActionResult GetAllTransactions()
    {
      try
      {
        var ledger = _repository.GetAllTransactions();
        if (ledger.Any())
        {
          return Ok(new { Ledger = ledger });
        }
        return NotFound(new { Message = "No transactions found." });
      }
      catch (Exception e)
      {
        return StatusCode(500, new { Message = "Internal Server Error", Error = e.Message });
      }
    }

    [HttpGet("transaction")]
    public IActionResult GetTransaction(int transactionId)
    {
      try
      {
        var transaction = _repository.GetTransaction(transactionId);
        if (transaction != null)
        {
          return Ok(new { Transaction = transaction });
        }
        return NotFound(new { Message = "Transaction not found." });
      }
      catch (Exception e)
      {
        return StatusCode(500, new { Message = "Internal Server Error", Error = e.Message });
      }
    }
  }
}

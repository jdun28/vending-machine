import React, { useState, useEffect } from 'react';

const VendingMachine = () => {
  const [inventory, setInventory] = useState([]);
  const [transaction, setTransaction] = useState({ items: [], amountPaid: 0 });
  const [message, setMessage] = useState('');
  const [amountDue, setAmountDue] = useState(0);

  useEffect(() => {
    fetchInventory();
  },[]);
  
  const fetchInventory = async () => {
    try {
      const resp = await fetch('http://localhost:5000/api/VendingMachine/inventory');
      const data = await resp.json();
      setInventory(data);
    } catch (error) {
      console.error('Error fetching inventory:', error);
    }
  };

  const handleItemClick = (item) => {
    setTransaction((prevTransaction) => ({
      ...prevTransaction,
      items: [...prevTransaction.items, item.name],
    }));
    setAmountDue(amountDue + item.price);
  };

  const handleAmountPaidChange = (e) => {
    setTransaction((prevTransaction) => ({
      ...prevTransaction,
      amountPaid: parseFloat(e.target.value),
    }));
  };

  const handlePurchase = async () => {
    try {
      if (transaction.amountPaid !== amountDue) {
        setMessage('Amount paid must match amount due');
        return;
      }
      const resp = await fetch('http://localhost:5000/api/VendingMachine/purchase', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(transaction),
      });
      if (resp.ok) {
        const data = await resp.json();
        setMessage(data.message);
        fetchInventory();
        setAmountDue(0);
        setTransaction({items: [], amountPaid: 0 });
      } else {
        const data = await resp.json();
        setMessage(data.message);
      }
    } catch (error) {
      console.error('Error making purchase:', error);
    }
  };

  return (
    <div>
      <h1>Vending Machine</h1>
      <div>
        <h2>Inventory</h2>
        {Array.isArray(inventory.inventory)? (
          <ul>
          {inventory.inventory.map((item) => (
            <li key={item.name}>
              <span style={{paddingRight: 4}}>{item.name} - ${item.price} - Quantity: {item.quantity}</span>
              <button onClick={() => handleItemClick(item)}>Add Item</button>
            </li>
          ))}
          </ul>
        ) : ( 
        <p>Loading...</p>
        )}
        
      </div>
      <div>
        <h2>Transaction</h2>
        <div style={{paddingBottom: 8}}>
          <strong>Selected Items: </strong> {transaction.items.join(', ')}
        </div>
        <div style={{paddingBottom: 8}}>
          <strong>Amount Due: </strong> ${amountDue}
        </div>
        <div>
          <label style={{paddingRight: 8}}>
            <span style={{paddingRight: 4}}><strong>Amount Paid:</strong></span>
            $<input type="number" value={transaction.amountPaid} onChange={handleAmountPaidChange} />
          </label>
          <button onClick={handlePurchase}>Make Purchase</button>
        </div>
      </div>
      <strong>{message}</strong>
    </div>
  );
};

export default VendingMachine;
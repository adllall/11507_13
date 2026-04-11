namespace CW;
public class TransactionEventArgs : EventArgs
{
    public decimal Amount { get; set; }
    public decimal Limit { get; set; }
    public DateTime Time { get; set; }
}

public class FinancialMonitor
{
    public decimal WarningLimit { get; set; } = 50000;
    public event EventHandler<TransactionEventArgs> LargeTransactionDetected;

    public void ProcessTransfer(decimal amount)
    {
        if (amount >= WarningLimit)
        {
            LargeTransactionDetected.Invoke(this, new TransactionEventArgs
            {
                Amount = amount,
                Limit = WarningLimit,
                Time = DateTime.Now
            });
        }
        Console.WriteLine($"Перевод {amount} обработан.");
    }

}
/*
class Program
{
    static void Main()
    {
        var monitor = new FinancialMonitor { WarningLimit = 10000 };
        monitor.LargeTransactionDetected += SecurityHandler;
        monitor.ProcessTransfer(5000);
        monitor.ProcessTransfer(15000);
        monitor.ProcessTransfer(9999);
        monitor.ProcessTransfer(10000);
    }

    static void SecurityHandler(object sender, TransactionEventArgs e)
    {
        Console.WriteLine($"ТРЕВОГА! Крупный перевод: {e.Amount} (Лимит: {e.Limit}) в {e.Time}");
    }
}
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

// Модуль проверка данных
[AttributeUsage(AttributeTargets.Property)]
public class RequiredAttribute : Attribute { }

public class Order
{
    [Required]
    public string UserEmail { get; set; }
    [Required]
    public decimal TotalAmount { get; set; }
    public string ProductName { get; set; }
    public OrderStatus Status { get; set; }
    public decimal DiscountedAmount { get; set; }
    public int OrderId { get; set; }
    
    public Order(){
        Status = OrderStatus.New;
        DiscountedAmount = TotalAmount;
    }
}

public enum OrderStatus {
    New,
    Paid,
    Rejected,
    SentToDelivery,
    DeliveryConfirmed,
    DeliveryFailed
}

//  Модуль проверка данных
public class DataValidationModule
{
    public bool ValidateOrder(Order order)
    {
        var properties = order.GetType().GetProperties();
        
        foreach (var prop in properties)
        {
            var requiredAttr = prop.GetCustomAttribute<RequiredAttribute>();
            if (requiredAttr != null)
            {
                var value = prop.GetValue(order);
                if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                {
                    Console.WriteLine($"Validation failed: {prop.Name} is required but missing");
                    return false;
                }
                
                if (value is decimal dec && dec == 0)
                {
                    Console.WriteLine($"Validation failed: {prop.Name} cannot be zero");
                    return false;
                }
            }
        }
        
        return true;
    }
}

// Модуль динамические скидки
public interface IDiscountRule {
    string RuleName { get; }
    decimal ApplyDiscount(Order order, decimal currentAmount);
}

public class RegularCustomerDiscount : IDiscountRule {
    public string RuleName => "Regular Customer Discount";
    public decimal ApplyDiscount(Order order, decimal currentAmount) {
        return currentAmount * 0.9m; // 10% скидка для обычных клиентов
    }
}

public class PremiumCustomerDiscount : IDiscountRule {
    public string RuleName => "Premium Customer Discount";
    public decimal ApplyDiscount(Order order, decimal currentAmount) {
        if (order.UserEmail?.Contains("premium") == true) // 20% скидка для премиум клиентов
        {
            return currentAmount * 0.8m;
        }
        return currentAmount;
    }

public class BulkOrderDiscount : IDiscountRule {
    public string RuleName => "Bulk Order Discount";
    public decimal ApplyDiscount(Order order, decimal currentAmount) {
        if (order.TotalAmount > 1000){  
            return currentAmount * 0.95m; // 5% скидка для заказов больше 1000
        }
        return currentAmount;
    }
}

public class HolidayDiscount : IDiscountRule {
    public string RuleName => "Holiday Discount";
    public decimal ApplyDiscount(Order order, decimal currentAmount) {
        return currentAmount * 0.85m; // 15% праздничная скидка
    }
}
public class DiscountModule {
    private List<IDiscountRule> _discountRules;
    public DiscountModule() {
        _discountRules = new List<IDiscountRule>();
        LoadDiscountRulesFromAssembly();
    }
    private void LoadDiscountRulesFromAssembly() {
        var discountRuleType = typeof(IDiscountRule);
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => discountRuleType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        foreach(var type in types){
            try{
                var rule = (IDiscountRule)Activator.CreateInstance(type);
                _discountRules.Add(rule);
                Console.WriteLine($"Loaded discount rule: {rule.RuleName}");
            }
            catch(Exception ex){
                Console.WriteLine($"Failed to load discount rule {type.Name}: {ex.Message}");
            }
        }
    }
    public decimal ApplyAllDiscounts(Order order) {
        decimal discountedAmount = order.TotalAmount;
        foreach(var rule in _discountRules) {
            decimal previousAmount = discountedAmount;
            discountedAmount = rule.ApplyDiscount(order, discountedAmount);
            if(previousAmount != discountedAmount) {
                Console.WriteLine($"Applied {rule.RuleName}: {previousAmount:C} -> {discountedAmount:C}");
            }
        }
        order.DiscountedAmount = discountedAmount;
        return discountedAmount;
    }
}

// Модуль отправка в доставку
public class DeliveryModule {
    private static readonly Random _random = new Random();
    public async Task<bool> SendToDeliveryAsync(Order order) {
        if(order.Status != OrderStatus.Paid){
            throw new InvalidOperationException($"Order {order.OrderId} is not paid. Current status: {order.Status}");
        }
        await Task.Delay(500);
        if(_random.Next(100) == 0) {
            Console.WriteLine($"Delivery service failed for order {order.OrderId}");
            return false;
        }
        Console.WriteLine($"Delivery service confirmed order {order.OrderId}");
        return true;
    }
}

// Модуль логирования
public class OrderEventArgs : EventArgs {
    public int OrderId { get; set; }
    public string StageName { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AuditLogger {
    private readonly string _logFilePath;
    public AuditLogger(string logFilePath = null){
        _logFilePath = logFilePath ?? "audit_log.txt";
    }
    public void OnOrderStateChanged(object sender, OrderEventArgs e){
        string logMessage = $"[{e.Timestamp:yyyy-MM-dd HH:mm:ss}] Order {e.OrderId} - Stage: {e.StageName}";
        Console.WriteLine($"LOG: {logMessage}");
        try{
            File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
        }
        catch (Exception ex){
            Console.WriteLine($"Failed to write to log file: {ex.Message}");
        }
    }
}

public class LoggingModule {
    public event EventHandler<OrderEventArgs> OnOrderStateChanged;
    public void LogOrderStage(int orderId, string stageName) {
        var args = new OrderEventArgs {
            OrderId = orderId,
            StageName = stageName,
            Timestamp = DateTime.Now
        };
        
        OnOrderStateChanged?.Invoke(this, args);
    }
    public void SubscribeLogger(AuditLogger logger) {
        OnOrderStateChanged += logger.OnOrderStateChanged;
    }
}

//Основная система обработки
public class SmartBridgeSystem
{
    private readonly DataValidationModule _validationModule;
    private readonly DiscountModule _discountModule;
    private readonly DeliveryModule _deliveryModule;
    private readonly LoggingModule _loggingModule;
    private readonly SemaphoreSlim _deliverySemaphore;
    public SmartBridgeSystem() {
        _validationModule = new DataValidationModule();
        _discountModule = new DiscountModule();
        _deliveryModule = new DeliveryModule();
        _loggingModule = new LoggingModule();
        _deliverySemaphore = new SemaphoreSlim(5);
        var auditLogger = new AuditLogger();
        _loggingModule.SubscribeLogger(auditLogger);
    }
    
    public async Task<bool> ProcessSingleOrderAsync(Order order){
        try{
            // Проверка данных
            _loggingModule.LogOrderStage(order.OrderId, "Data Validation Started");
            if (!_validationModule.ValidateOrder(order)){
                order.Status = OrderStatus.Rejected;
                _loggingModule.LogOrderStage(order.OrderId, "Data Validation Failed - Order Rejected");
                return false;
            }
            _loggingModule.LogOrderStage(order.OrderId, "Data Validation Passed");
            
            // Применение скидок
            _loggingModule.LogOrderStage(order.OrderId, "Discount Application Started");
            var discountedAmount = _discountModule.ApplyAllDiscounts(order);
            _loggingModule.LogOrderStage(order.OrderId, $"Discount Applied. New amount: {discountedAmount:C}");
            
            // Оплата
            order.Status = OrderStatus.Paid;
            _loggingModule.LogOrderStage(order.OrderId, "Payment Processed");
            
            // Доставка
            _loggingModule.LogOrderStage(order.OrderId, "Delivery Processing Started");
            var deliverySuccess = await _deliveryModule.SendToDeliveryAsync(order);
            if(deliverySuccess){
                order.Status = OrderStatus.DeliveryConfirmed;
                _loggingModule.LogOrderStage(order.OrderId, "Delivery Confirmed - Order Completed");
                return true;
            }
            else
            {
                order.Status = OrderStatus.DeliveryFailed;
                _loggingModule.LogOrderStage(order.OrderId, "Delivery Failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            _loggingModule.LogOrderStage(order.OrderId, $"Error: {ex.Message}");
            order.Status = OrderStatus.Rejected;
            return false;
        }
    }
    
    // Модуль параллельной обработки
    public async Task<ProcessBatchResult> ProcessBatchAsync(List<Order> orders) {
        var results = new ProcessBatchResult {
            TotalOrders = orders.Count,
            ProcessedOrders = new Dictionary<int, bool>()
        };
        var tasks = new List<Task>();
        foreach(var order in orders){
            await _deliverySemaphore.WaitAsync();
            var task = Task.Run(async () =>
            {
                try{
                    var success = await ProcessSingleOrderAsync(order);
                    lock (results)
                    {
                        results.ProcessedOrders[order.OrderId] = success;
                        if (success)
                            results.SuccessfulOrders++;
                        else
                            results.FailedOrders++;
                    }
                }
                finally
                {
                    _deliverySemaphore.Release();
                }
            });
            
            tasks.Add(task);
        }
        await Task.WhenAll(tasks);
        return results;
    }
}

public class ProcessBatchResult{
    public int TotalOrders { get; set; }
    public int SuccessfulOrders { get; set; }
    public int FailedOrders { get; set; }
    public Dictionary<int, bool> ProcessedOrders { get; set; }
    public void PrintResults(){
        Console.WriteLine("\n========== BATCH PROCESSING RESULTS ==========");
        Console.WriteLine($"Total Orders: {TotalOrders}");
        Console.WriteLine($"Successful: {SuccessfulOrders}");
        Console.WriteLine($"Failed: {FailedOrders}");
        Console.WriteLine("=============================================\n");
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("SMART BRIDGE SYSTEM TEST \n");
        var bridge = new SmartBridgeSystem();
        var testOrders = new[]
        {
            new { Id = 1, Email = "premium@test.com", Amount = 2500m, Name = "Premium Bundle" },
            new { Id = 2, Email = "customer@test.com", Amount = 800m, Name = "Standard Package" },
            new { Id = 3, Email = "", Amount = 500m, Name = "No Email" },
            new { Id = 4, Email = "bulk@test.com", Amount = 3000m, Name = "Bulk Order" },
            new { Id = 5, Email = "holiday@test.com", Amount = 400m, Name = "Holiday Special" },
            new { Id = 6, Email = "test@test.com", Amount = 0m, Name = "Zero Amount" },
            new { Id = 7, Email = "regular@test.com", Amount = 150m, Name = "Small Order" }
        };

        var orders = testOrders.Select(to => new Order
        {
            OrderId = to.Id,
            UserEmail = to.Email,
            TotalAmount = to.Amount,
            ProductName = to.Name
        }).ToList();
        Console.WriteLine("Starting test with 7 orders...");
        var result = await bridge.ProcessBatchAsync(orders);
        Console.WriteLine("TEST RESULTS:");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"{"ID",-5} {"Product",-20} {"Status",-18} {"Original",10} {"Discounted",12}");
        Console.WriteLine(new string('-', 60));
        foreach (var order in orders)
        {
            string status = order.Status.ToString();
            string discountInfo = order.DiscountedAmount < order.TotalAmount ? "✓" : " ";
            Console.WriteLine(
                $"{order.OrderId,-5} {order.ProductName,-20} {status,-18} {order.TotalAmount,10:C} {order.DiscountedAmount,12:C} {discountInfo}");
        }

        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"\n SUMMARY:");
        Console.WriteLine($"   Total Orders: {result.TotalOrders}");
        Console.WriteLine($"   Successful: {result.SuccessfulOrders}");
        Console.WriteLine($"   Failed: {result.FailedOrders}");
        Console.WriteLine($"   Success Rate: {100.0 * result.SuccessfulOrders / result.TotalOrders:F1}%");
        if (File.Exists("audit_log.txt"))
        {
            var logLines = File.ReadAllLines("audit_log.txt");
            Console.WriteLine($"\n Audit log created: {logLines.Length} entries");
            Console.WriteLine($"   Last 3 log entries:");
            foreach (var line in logLines.Skip(Math.Max(0, logLines.Length - 3)))
            {
                Console.WriteLine($"   {line}");
            }
        }
    }
}
}
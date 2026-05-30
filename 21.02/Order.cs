using System;
using System.Numerics;

public class Order<T> where T : INumber<T>
{
    public int Id { get; set; }
    public T BasePrice { get; set; }
    public T FinalPrice { get; set; }
    public override string ToString(){
        return $"Order #{Id}: BasePrice={BasePrice}, FinalPrice={FinalPrice}";
    }
}

public interface IIdStep<T> where T : INumber<T>{
    IPriceStep<T> SetId(int id);
}
public interface IPriceStep<T> where T : INumber<T>{
    IFinalStep<T> SetBasePrice(T price);
}
public interface IFinalStep<T> where T : INumber<T>{
    Order<T> Build();
}


public class OrderBuilder<T> : IIdStep<T>, IPriceStep<T>, IFinalStep<T> where T : INumber<T>{
    private int _id;
    private T _basePrice;
    private OrderBuilder() { }
    public static IIdStep<T> Create() => new OrderBuilder<T>();
    public IPriceStep<T> SetId(int id){
        _id = id;
        return this;
    }
    public IFinalStep<T> SetBasePrice(T price){
        _basePrice = price;
        return this;
    }
    public Order<T> Build(){
        return new Order<T>{
            Id = _id,
            BasePrice = _basePrice,
            FinalPrice = _basePrice
        };
    }
}


public abstract class OrderHandler<T> where T : INumber<T>
{
    protected OrderHandler<T>? _next;
    public OrderHandler<T> SetNext(OrderHandler<T> next) {
        _next = next;
        return next;
    }
    public virtual void Handle(Order<T> order) {
        _next?.Handle(order);
    }
}

public class DiscountHandler<T> : OrderHandler<T> where T : INumber<T>
{
    private readonly T _discountAmount;
    public DiscountHandler(T discountAmount) {
        _discountAmount = discountAmount;
    }
    public override void Handle(Order<T> order) {
        order.FinalPrice = order.FinalPrice - _discountAmount;
        Console.WriteLine($"  Применена скидка {_discountAmount}: новая цена {order.FinalPrice}");
        base.Handle(order);
    }
}
public class TaxHandler<T> : OrderHandler<T> where T : INumber<T>
{
    private readonly double _taxRate; 
    public TaxHandler(double taxRate) {
        _taxRate = taxRate;
    }
    public override void Handle(Order<T> order) {
        T multiplier = T.CreateChecked(1 + _taxRate);
        order.FinalPrice = order.FinalPrice * multiplier;
        Console.WriteLine($"  Добавлен налог {_taxRate * 100}%: новая цена {order.FinalPrice}");
        base.Handle(order);
    }
}

public class ValidationHandler<T> : OrderHandler<T> where T : INumber<T> {
    public override void Handle(Order<T> order) {
        if(order.FinalPrice < T.Zero){
            throw new InvalidOperationException($"Цена заказа #{order.Id} стала отрицательной: {order.FinalPrice}");
        }
        Console.WriteLine($"  Проверка: цена {order.FinalPrice} >= 0");
        base.Handle(order);
    }
}


public class OrderProcessor<T> where T : INumber<T> {
    private readonly OrderHandler<T> _chain;
    public OrderProcessor(OrderHandler<T> chain){
        _chain = chain;
    }
    public void Process(Order<T> order) {
        Console.WriteLine($"\nОбработка заказа #{order.Id}");
        Console.WriteLine($"  Начальная цена: {order.BasePrice}");
        _chain.Handle(order);
        Console.WriteLine($"  Итоговая цена: {order.FinalPrice}");
    }
}

class Order {
    static void Main() {
        var order = OrderBuilder<decimal>.Create()
            .SetId(1)
            .SetBasePrice(500m)
            .Build();
        Console.WriteLine($"Создан: {order}");
        var discount = new DiscountHandler<decimal>(100m);
        var tax = new TaxHandler<decimal>(0.2);
        var validation = new ValidationHandler<decimal>();
        discount.SetNext(tax).SetNext(validation);
        var processor = new OrderProcessor<decimal>(discount);
        processor.Process(order);
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Reflection;


// Задача 1
namespace ObjectClonerAndTaskQueue
{
    public static class ObjectCloner
    {
        public static object Clone(object obj)
        {
            if (obj == null)
                return null;

            Type type = obj.GetType();
            object cloned = Activator.CreateInstance(type);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            { 
                if (prop.CanRead && prop.CanWrite)
                {
                    object value = prop.GetValue(obj);
                    prop.SetValue(cloned, value);
                }
            }
            
            return cloned;
        }
        public static T Clone<T>(T obj) where T : class
        {
            return (T)Clone((object)obj);
        }
    }
    
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public bool IsStudent { get; set; }
        
        public override string ToString()
        {
            return $"{Name}, {Age} лет, Студент: {(IsStudent ? "Да" : "Нет")}";
        }
    }
    // Задача 2
    public class TaskQueueManager
    {
        private readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
        private readonly object _lock = new object();
        private readonly int _producerCount;
        private readonly int _consumerCount;

        private int _producerCompleted = 0;
        private int _consumerCompleted = 0;
    
        private bool _producerFinished = false;
        public TaskQueueManager(int producerCount, int consumerCount){
            _producerCount = producerCount;
            _consumerCount = consumerCount;
        }
    public void Start()
        {
            Thread producerThread = new Thread(ProducerWork);
            producerThread.Name = "Producer";
            producerThread.Start();
            
            Thread consumerThread = new Thread(ConsumerWork);
            consumerThread.Name = "Consumer";
            consumerThread.Start();
            
            producerThread.Join();
            consumerThread.Join();
            
            Console.WriteLine($"Producer обработал: {_producerCompleted}");
            Console.WriteLine($"Consumer выполнил: {_consumerCompleted}");
        }
        private void ProducerWork()
        {
            Console.WriteLine($"[{Thread.CurrentThread.Name}] Запущен. Будет создано {_producerCount} действий...");
            for (int i = 0; i < _producerCount; i++)
            {
                var original = new Person
                {
                    Name = $"Студент_{i + 1}",
                    Age = 18 + (i % 10),
                    IsStudent = true
                };
                var clone = (Person)ObjectCloner.Clone(original); // клон 1 задачи 
                clone.Name = $"{clone.Name}_копия";
                Action action = () =>
                {
                    Console.WriteLine($"[Consumer] Выполняю действие с клоном: {clone}");
                    Thread.Sleep(100);
                };
                _queue.Enqueue(action);
                Console.WriteLine($"[{Thread.CurrentThread.Name}] Добавлено действие #{i + 1} с клоном объекта");
                _producerCompleted++;
                Thread.Sleep(50); 
            }
            _producerFinished = true;
            Console.WriteLine($"[{Thread.CurrentThread.Name}] Закончил создание действий.");
        }
        
        private void ConsumerWork()
        {
            Console.WriteLine($"[{Thread.CurrentThread.Name}] Запущен");
            while(!_producerFinished || !_queue.IsEmpty)
            {
                if(_queue.TryDequeue(out Action action))
                {
                    action();
                    _consumerCompleted++;
                    Console.WriteLine($"[{Thread.CurrentThread.Name}] Выполнено действие #{_consumerCompleted}. Осталось в очереди: {_queue.Count}");
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
            
            Console.WriteLine($"[{Thread.CurrentThread.Name}] Закончил выполнение действий. Всего выполнено: {_consumerCompleted}");
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            // 1
            Console.WriteLine("Задача 1");
            
            var originalPerson = new Person
            {
                Name = "Иван Иванов",
                Age = 25,
                IsStudent = false
            };
            var clonedPerson = ObjectCloner.Clone(originalPerson);
            Console.WriteLine("Оригинал: " + originalPerson);
            Console.WriteLine("Клон: " + clonedPerson);
            originalPerson.Name = "Новый Иван";
            originalPerson.Age = 30;
            Console.WriteLine("\nПосле изменения оригинала:");
            Console.WriteLine("Оригинал: " + originalPerson);
            Console.WriteLine("Клон: " + clonedPerson);
            
            // 2 3
            Console.WriteLine("Задача 2");
            var manager = new TaskQueueManager(producerCount: 20, consumerCount: 20);
            manager.Start();
        }
    }
}

    
    
    

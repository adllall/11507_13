using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

class IO1
{
    static void Main()
    {
        var machine = new CoffeeMachine();
        bool running =true;
        while (running)
        {
            Console.WriteLine("\n1-Меню 2-Остатки 3-Заказать 4-Конец смены 0-Выход");
            switch (Console.ReadLine())
            {
                case "1": machine.ShowMenu(); break;
                case "2": machine.ShowIngredients(); break;
                case "3":
                    Console.Write("Название: ");
                    machine.MakeDrink(Console.ReadLine() ?? "");
                    break;
                case "4": machine.EndOfShift(); break;
                case "0": running = false; break;
            }
        }
    }
}

class CoffeeMachine
{
    private Config config;
    private readonly string configFile = "config.json";
    private readonly string logFile = "sales_history.txt";
    private readonly Dictionary<string, Dictionary<string, int>> recipes = new()
    {
        { "Эспрессо", new() { { "Вода", 50 }, { "Зерна", 10 } } },
        { "Американо", new() { { "Вода", 150 }, { "Зерна", 10 } } },
        { "Капучино", new() { { "Вода", 50 }, { "Молоко", 100 }, { "Зерна", 10 } } },
        { "Латте", new() { { "Вода", 50 }, { "Молоко", 150 }, { "Зерна", 10 } } }
    };
    
    public CoffeeMachine() => config = LoadConfig();
    private Config LoadConfig()
    {
        if(!File.Exists(configFile))
        {
            var defaultCfg = new Config
            {
                DrinkPrices = new() { { "Эспрессо", 100 }, { "Американо", 120 }, { "Капучино", 150 }, { "Латте", 170 } },
                Ingredients = new() { { "Вода", 5000 }, { "Молоко", 3000 }, { "Зерна", 2000 } }
            };
            string json = JsonSerializer.Serialize(defaultCfg, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configFile, json);
            return defaultCfg;
        }
        return JsonSerializer.Deserialize<Config>(File.ReadAllText(configFile)) ?? new Config();
    }
    
    private void SaveConfig() => 
        File.WriteAllText(configFile, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
    public void ShowMenu()
    {
        foreach (var d in config.DrinkPrices)
            Console.WriteLine($"{d.Key} - {d.Value} руб.");
    }

    public void ShowIngredients()
    {
        foreach (var i in config.Ingredients)
            Console.WriteLine($"{i.Key}: {i.Value}");
    }
    public bool MakeDrink(string name)
    {
        if(!config.DrinkPrices.ContainsKey(name) || !recipes.ContainsKey(name)) {
            Console.WriteLine("Нет такого напитка");
            return false;
        }
        var recipe = recipes[name];
        foreach (var ing in recipe)
            if (config.Ingredients.GetValueOrDefault(ing.Key, 0) < ing.Value)
            {
                Console.WriteLine($"Не хватает {ing.Key}");
                return false;
            }
        foreach(var ing in recipe)
            config.Ingredients[ing.Key] -= ing.Value;
        SaveConfig();
        File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Продано: {name}, Цена: {config.DrinkPrices[name]}\n");
        Console.WriteLine($"✅ {name} готов!");
        return true;
    }
    public void EndOfShift()
    {
        if(!File.Exists(logFile)) {
            Console.WriteLine("Продаж нет");
            return;
        }
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        var sales = File.ReadAllLines(logFile)
            .Where(l => l.StartsWith($"[{today}"))
            .ToList();
        var report = new
        {
            Date = today,
            TotalSales = sales.Count,
            TotalRevenue = sales.Sum(l => 
            {
                var parts = l.Split(", Цена: ");
                return parts.Length > 1 && int.TryParse(parts[1], out int p) ? p : 0;
            }),
            DrinksSold = sales.Select(l => l.Split("Продано: ")[1].Split(", Цена:")[0])
                              .GroupBy(d => d)
                              .ToDictionary(g => g.Key, g => g.Count())
        };
        
        string reportFile = $"report_{today.Replace("-", "_")}.json";
        File.WriteAllText(reportFile, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Отчёт сохранён: {reportFile}\nПродаж: {report.TotalSales}, Выручка: {report.TotalRevenue} руб.");
    }
}
class Config
{
    public Dictionary<string, int> DrinkPrices { get; set; } = new();
    public Dictionary<string, int> Ingredients { get; set; } = new();
}

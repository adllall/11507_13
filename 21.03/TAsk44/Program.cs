using System;
using System.Linq;

public class Task4
{
    static void Main()
    {
        string[] words = { "cat", "bat", "car", "bar", "dog", "fog", "log", "cot" };
        Console.WriteLine("Исходный набор слов:");
        foreach (string word in words)
        {
            Console.Write(word + " ");
        }
        Console.WriteLine("\n");
        var dictionary = PreprocessWords(words);
        TestQuery(dictionary, "car", words);
        TestQuery(dictionary, "cat", words);
        TestQuery(dictionary, "dog", words);
        TestQuery(dictionary, "cog", words);
    }

    static void TestQuery(Dictionary<string, List<string>> dict, string query, string[] allWords)
    {
        Console.WriteLine($"Запрос: '{query}'");
        var result = FindWordsByOneLetterChange(dict, query, allWords);
        if (result.Count == 0)
        {
            Console.WriteLine("  Слов, отличающихся одной буквой, не найдено");
        }
        else
        {
            Console.WriteLine($"  Найдено: {string.Join(", ", result)}");
        }
        Console.WriteLine();
    }
    
    static Dictionary<string, List<string>> PreprocessWords(string[] words)
    {
        var dict = new Dictionary<string, List<string>>();
        foreach (string word in words)
        {
            int len = word.Length;
            for (int i = 0; i < len; i++)
            {
                string mask = word.Substring(0, i) + '*' + word.Substring(i + 1);
                
                if (!dict.ContainsKey(mask))
                {
                    dict[mask] = new List<string>();
                }
                dict[mask].Add(word);
            }
        }
        return dict;
    }
    
    static List<string> FindWordsByOneLetterChange(
        Dictionary<string, List<string>> dict, 
        string query, 
        string[] allWords)
    {
        var result = new HashSet<string>();
        int len = query.Length;
        for (int i = 0; i < len; i++)
        {
            string mask = query.Substring(0, i) + '*' + query.Substring(i + 1);
            
            if (dict.ContainsKey(mask))
            {
                foreach (string word in dict[mask])
                {
                    result.Add(word);
                }
            }
        }
        result.Remove(query);
        return result.ToList();
    }
}

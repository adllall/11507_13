using System;
using System.Linq;

public class Task3
{
    public static void Main()
    {
        string[] strings = {  "pukkk", "privet", "blablabla", "kak", " ", "algosikikiki", "dela", "hahaha"};
        var result = FindStringsWithMaxTwoRepeats(strings);
        
        foreach (var str in result)
            Console.WriteLine(str);
    }
    
    public static IEnumerable<string> FindStringsWithMaxTwoRepeats(IEnumerable<string> strings)
    {
        return strings
            .Where(str => str != null)  
            .Where(str => str.Length > 0) 
            .Where(str => 
                str.ToLower() 
                    .GroupBy(c => c)
                    .All(group => group.Count() <= 2)
            );
    }
    
    private static bool IsValidString(string str)
    {
        return str.GroupBy(c => c)
            .All(group => group.Count() <= 2);
    }
}
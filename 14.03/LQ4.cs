using System;
using System.Collections.Generic;
using System.Linq;

class LQ4
{
    private Dictionary<string, List<string>> patternMap = new();
    public void Preprocess(IEnumerable<string> words)
    {
        foreach(var word in words) {
            int k = word.Length;
            for (int i = 0; i < k; i++)
            {
                string pattern = word.Substring(0, i) + "*" + word.Substring(i + 1);
                if(!patternMap.ContainsKey(pattern))
                    patternMap[pattern] = new List<string>();
                patternMap[pattern].Add(word);
            }
        }
    }

    public IEnumerable<string> GetWordsWithOneReplace(string query)
    {
        int k = query.Length;
        var result = new HashSet<string>(); 
        for(int i = 0; i < k; i++)
        {
            string pattern = query.Substring(0, i) + "*" + query.Substring(i + 1);
            if(patternMap.TryGetValue(pattern, out var matches)){
                foreach(var match in matches) {
                    if(match != query)
                        result.Add(match);
                }
            }
        }
        return result;
    }
    // public static void Main()
    // {
    //     var solution = new LQ4();
    //     var words = new[] { "cat", "bat", "rat", "hat", "cut", "car", "cab" };
    //     solution.Preprocess(words);
    //     var result = solution.GetWordsWithOneReplace("cat");
    //     Console.WriteLine(string.Join(", ", result)); // bat, rat, hat, cut
    // }
}
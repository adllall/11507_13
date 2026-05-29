using System;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleApp8;

class LQ3
{
    // public static void Main()
    // {
    //     var strings = new[] { "pollki", "aammnn", "ooopp", "abcabc", "mmnnmm" };
    //     var result = FilterStringsWithMaxTwoRepeat(strings);
    //     foreach (var s in result)
    //         Console.WriteLine(s); 
    // }
    static IEnumerable<string> FilterStringsWithMaxTwoRepeat(IEnumerable<string> strings)
    {
        return strings.Where(s =>
        {
            return s.GroupBy(c => c).All(g => g.Count() <= 2);
        });
    }
}
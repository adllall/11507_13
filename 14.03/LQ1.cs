using System;
using System.Linq;

class LQ1
{
    // static void Main()
    // {
    //     int[] array = {1,2,3,4,5};
    //     int k = 3;
    //     int[] result = ShiftLeft(array, k);
    //     Console.WriteLine(string.Join(" ", result));
    // }
    static int[] ShiftLeft(int[] arr, int k)
    {
        if(arr.Length== 0) 
            return arr;
        k = k % arr.Length;
        int[] result = arr.Skip(k).Concat(arr.Take(k)).ToArray();
        return result;
    }
}
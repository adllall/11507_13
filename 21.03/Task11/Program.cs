using System;
using System.Linq;

class Program
{
    public static int[] ShiftLeft(int[] array, int k)
    {
        if (array == null || array.Length == 0 || k % array.Length == 0) return array.ToArray();
            int n = array.Length;
        int shift = k % n; 
        return array.Skip(shift)
            .Concat(array.Take(shift))
            .ToArray();
    }
    public static void Main()
    {
     	int[] array = { 1, 2, 3, 4, 5 };
    	int k = 4;
    	var result = ShiftLeft(array, k);
    	Console.WriteLine(string.Join(", ", result));
    }
		
}

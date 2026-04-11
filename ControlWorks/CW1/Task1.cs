using System;
namespace CW;

// Task1
public static class MathUtils
{
    public static T FindMedian<T>(T a, T b, T c) where T : IComparable<T> 
    { 
        T[] arr = new T[] { a, b, c };
        Array.Sort(arr);
        return arr[1]; 
    }

    public static T FindArrayMedian<T>(T[] array) where T :IComparable<T> 
    {
        if (array == null || array.Length == 0)
        {
            throw new ArgumentException("Массив не может быть пустым!");
        }
        T[] sorted = (T[])array.Clone();
        Array.Sort(sorted);
        int mid = (sorted.Length - 1) / 2;
        return sorted[mid];
    }
}

/*
class Task1
{
    static void Main(string[] args)
    {
        Console.WriteLine(MathUtils.FindMedian(10, 5, 20)); // 10
        Console.WriteLine(MathUtils.FindMedian("Apple", "Banana", "Cherry")); // banana
        int [] intArray1 = {1,3,4,2}; 
        Console.WriteLine(MathUtils.FindArrayMedian(intArray1)); // 2
        int [] intArray2 = {5,3,4,2,1};
        Console.WriteLine(MathUtils.FindArrayMedian(intArray2)); //3
    }
}
*/
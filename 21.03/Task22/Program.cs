using System;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        var test1 = new List<(int x, int y)> { (0, 0) };
        RunTest(test1);
        
        var test2 = new List<(int x, int y)> { (0, 0), (0, 1) };
        RunTest(test2);
    }

    static void RunTest(List<(int x, int y)> points)
    {
        Console.WriteLine($"Исходные точки: ({string.Join("), (", points)})");
        
        var neighborhood = GetNeighborhood(points);
        
        Console.WriteLine($"Количество точек в 1-окрестности: {neighborhood.Count}");
        Console.WriteLine("Точки 1-окрестности:");

        var sorted = neighborhood.OrderBy(p => p.x).ThenBy(p => p.y);
        foreach (var point in sorted)
        {
            Console.Write($"({point.x},{point.y}) ");
        }
        Console.WriteLine();
    }

    static HashSet<(int x, int y)> GetNeighborhood(List<(int x, int y)> points)
    {
        var neighbors = new HashSet<(int x, int y)>();
        
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        foreach (var point in points)
        {
            for (int i = 0; i < 8; i++)
            {
                int nx = point.x + dx[i];
                int ny = point.y + dy[i];
                neighbors.Add((nx, ny));
            }
        }

        return neighbors;
    }
}
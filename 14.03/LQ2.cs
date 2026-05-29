using System;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleApp8;
public class Point
{
    public Point(int x, int y) {
        X = x;
        Y = y;
    }
    public int X, Y;
    public override bool Equals(object? obj)
    {
        if(obj is Point other) {
            return X == other.X && Y == other.Y;
        }
        return base.Equals(obj);
    }
    
    public override int GetHashCode() {
        return HashCode.Combine(X, Y);
    }
    
    public override string ToString() {
        return $"({X}, {Y})";
    }
}

class LQ2
{
    static void Main()
    {
        var points1 = new List<Point> {new Point(0, 0)};
        var neighborhood1 = GetNeighborhood(points1);
        Console.WriteLine($"Для набора [(0, 0)]: {neighborhood1.Count} точек");
        foreach(var p in neighborhood1.OrderBy(p => p.X).ThenBy(p => p.Y)){
            Console.Write($"{p} ");
        }
        Console.WriteLine("\n");
        
        var points2 = new List<Point> { new Point(0, 0), new Point(0, 2) };
        var neighborhood2 = GetNeighborhood(points2);
        Console.WriteLine($"Для набора [(0, 0), (0, 2)]: {neighborhood2.Count} точек");
        foreach(var p in neighborhood2.OrderBy(p => p.X).ThenBy(p => p.Y)){
            Console.Write($"{p} ");
        }
        Console.WriteLine();
    }
    
    static HashSet<Point> GetNeighborhood(IEnumerable<Point> points)
    {
        int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };
        var neighbors = points
            .SelectMany(p => Enumerable.Range(0, 8), (p, i) => new Point(p.X + dx[i], p.Y + dy[i]))
            .Distinct()
            .ToHashSet();
        return neighbors;
    }
}
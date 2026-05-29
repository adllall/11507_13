using System;
using System.Diagnostics;
using System.IO;


class DataA
{
    static void Main()
    {
        string filePath = "bigdata.txt";
        if(!File.Exists(filePath))
        {
            Stopwatch genStopwatch= Stopwatch.StartNew();
            using(var sw = new StreamWriter(filePath))
            {
                for(int i = 0; i < 50_000_000; i++)
                {
                    sw.WriteLine("Data line with some A symbols and other chars.");
                }
            }
            genStopwatch.Stop();
        }
        
        Stopwatch stopwatch = Stopwatch.StartNew();
        byte[] buffer = new byte[65536];
        long totalACount = 0;
        byte targetByte = (byte)'A';

        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            int bytesRead;
            while((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
            {
                for(int i = 0; i < bytesRead; i++)
                {
                    if(buffer[i] == targetByte)
                        totalACount++;
                }
            }
        }
        stopwatch.Stop();
        
        
        Console.WriteLine($"количество символов 'A': {totalACount:N0}");
        Console.WriteLine($"время выполнения: {stopwatch.ElapsedMilliseconds}");

    }
}
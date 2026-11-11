using System;

internal class Program
{
    private static void Main()
    {
        decimal original = 12.50m;
        decimal proposed = -5m;

        if (proposed > 0m)
        {
            original = proposed;
        }

        Console.WriteLine(original == 50m);
    }
}
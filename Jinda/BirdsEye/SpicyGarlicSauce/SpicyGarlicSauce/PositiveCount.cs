using System;
using System.Collections.Generic;

public class PositiveCount()
{
    static void Main()
    {
        List<decimal> expenses = new List<decimal> { 12.50m, -5.00m, 0m, 20.00m, 7.25m };
        int totalPositiveCount = CalculateTotalPositiveCount(expenses);

        Console.WriteLine($"The positive total count is {totalPositiveCount}.");
    }

    public static int CalculateTotalPositiveCount(List<decimal> expenses)
    {
        int totalCount = 0;

        foreach (decimal expense in expenses)
        {
            if (expense > 0)
            {
                totalCount++;
            }
        }

        return totalCount;
    }
}

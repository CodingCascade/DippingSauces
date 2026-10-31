using System;
using System.Collections.Generic;

namespace SpicyGarlicSauce
{
    public class CountExpenseAboveLimit()
    {
        static void Main()
        {
            List<decimal> expenses = new List<decimal>
            {
                12.50m,
                -5.00m,
                0m,
                20.00m,
                7.25m
            };

            int aboveLimitExpenses = CountExpensesAboveLimit(expenses, 15.00m);
            Console.WriteLine($"The total expense count that is over the limit or budget is {aboveLimitExpenses}.");
        }

        public static int CountExpensesAboveLimit(List<decimal> expenses, decimal expenseLimit)
        {
            int totalCount = 0;

            foreach (decimal expense in expenses)
            {
                if (expense > expenseLimit)
                {
                    totalCount++;
                }
            }

            return totalCount;
        }

    }
}

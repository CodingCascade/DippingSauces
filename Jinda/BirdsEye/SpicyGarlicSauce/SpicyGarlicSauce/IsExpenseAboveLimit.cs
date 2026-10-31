using System;
using System.Collections.Generic;

namespace SpicyGarlicSauce
{
    public static class IsExpenseAboveLimit
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

            bool hasLimitExpenses = HasExpenseAboveLimit(expenses, 10.00m);
            Console.WriteLine($"Has there been an expense that is over the limit: {hasLimitExpenses}.");
        }

        public static bool HasExpenseAboveLimit(List<decimal> expenses, decimal expenseLimit)
        {
            foreach (decimal expense in expenses)
            {
                if (expense > expenseLimit)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

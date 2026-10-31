namespace SpicyGarlicSauce
{
    public class SumExpenseAboveLimit()
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

            decimal aboveLimitExpenses = SumExpensesAboveLimit(expenses, 15.00m);
            Console.WriteLine($"The total expenses that are over the limit are {aboveLimitExpenses}.");
        }

        public static decimal SumExpensesAboveLimit(List<decimal> expenses, decimal expenseLimit)
        {
            decimal totalAmount = 0m;

            foreach (decimal expense in expenses)
            {
                if (expense > expenseLimit)
                {
                    totalAmount += expense;
                }
            }

            return totalAmount;
        }

    }
}

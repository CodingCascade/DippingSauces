using System;
using System.Collections.Generic;

class Expense
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

class Program
{
    static void Main()
    {
        List<Expense> expenses = new List<Expense>();
        bool running = true;

        while (running)
        {
            Console.WriteLine("\n1 Add | 2 View | 3 Total | 0 Exit");
            Console.Write("Choose an option: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    {
                        AddExpense(expenses);
                        break;
                    }
                case "2":
                    {
                        ShowExpenses(expenses);
                        break;
                    }
                case "3":
                    {
                        decimal total = 0m;

                        foreach (Expense expense in expenses)
                        {
                            total += expense.Amount;
                        }

                        Console.WriteLine($"Total spent: {total:F2}");
                        break;
                    }
                case "0":
                case null:
                    {
                        running = false;
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Choose 1 (Add an Expense), 2 (View Expenses), 3 (View Total Spent), or 0 (Exit).");
                        break;
                    }
            }
        }
    }

    static void AddExpense(List<Expense> expenses)
    {
        Console.Write("Description: ");
        string? description = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(description))
        {
            Console.WriteLine("Please enter a description.");
            return;
        }

        Console.Write("Amount: ");
        string? input = Console.ReadLine();

        if (!decimal.TryParse(input, out decimal amount) || amount <= 0)
        {
            Console.WriteLine("Please enter a number that is greater than zero.");
            return;
        }

        Expense expense = new Expense();
        expense.Description = description.Trim();
        expense.Amount = amount;
        expenses.Add(expense);
        Console.WriteLine("Expense added successfully!");
    }

    static void ShowExpenses(List<Expense> expenses)
    {
        if (expenses.Count == 0)
        {
            Console.WriteLine("No expenses yet.");
            return;
        }

        foreach (Expense expense in expenses)
        {
            Console.WriteLine($"{expense.Description}: {expense.Amount:F2}");
        }
    }
}
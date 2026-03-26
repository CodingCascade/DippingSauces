using System;

namespace LearningCodingInCSharp
{
    class MyProgram
    {
        static void Main()
        {
            /*int score = 100; // Declare a variable
            Console.WriteLine("Player's score is: " + score);

            Console.WriteLine("What's your name?");
            string firstName = Console.ReadLine() ?? "there";
            Console.WriteLine("Hi, " + firstName + "! Welcome to C# programming!!");*/

            // Do while
            /*int counter = 100;

            do
            {
                Console.WriteLine("The counter is {0}", counter);
                counter++;
            } while (counter < 0); */

            //while

            /*int counter = 5;

            while (counter > 0)
            {
                Console.WriteLine("The counter is {0}.", counter);
                counter = counter - 1;
            }*/

            /*//Break
            int i = 0;

            for (i =0; i < 5; i++)
            {
                Console.WriteLine("i = {0}", i);
                if (i == 2)
                    break;
            }*/


        }
    }

    class LearningCreatingClass
    {
        public static string greetings = "Bonsoir!";
        public static int Age { get; set; }

        public static void DisplayAge()
        {
            Age = 45;
               Console.WriteLine("J'ai {0} ans. ", Age);
        }
    }

}

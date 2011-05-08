using System;
using SampleLibrary.Interfaces;

namespace SampleLibrary
{
    public class FizzBuzz : IPrinter
    {
        public void Print(int number)
        {
            if (number % 3 == 0 && number % 5 == 0)
            {
                Console.WriteLine("FizzBuzz");
                return;
            }

            if (number % 3 == 0)
                Console.WriteLine("Fizz");

            if (number % 5 == 0)
                Console.WriteLine("Buzz");
        }
    }
}

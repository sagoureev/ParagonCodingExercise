using System;

namespace ParagonCodingExercise
{
    class Program
    {

        /// <summary>
        /// This is the function that runs everything. I prefer Program.cs to always be super minimal
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine("Flight Processor Version 1");
            FlightProcessor processor = new FlightProcessor();
            processor.Execute();
            Console.WriteLine("Thank you for using the Flight Processor");
        }
    }
}

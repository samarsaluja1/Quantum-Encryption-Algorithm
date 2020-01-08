using System;
using Microsoft.Quantum.Simulation.Simulators;
using Microsoft.Quantum.Simulation.Core;
using System.Collections.Generic;
using System.IO;



namespace Microsoft.Quantum.Samples.IntegerFactorization
{
 
    class Program
    {
        //Checks if Input Integer is Prime
        static public bool isPrime(int x)
        {
            if (x == 2) { return true; }
            for (int i = 2; i <= Math.Ceiling(Math.Sqrt(x) + 1); i++) { if (x % i == 0 && i != x) { return false; } }
            return true;
        }

        static void Main(string[] args)
        {
            //Variables
            long numberToFactor;
            long nTrials = 3;
            bool useRobustPhaseEstimation = true;
            List<int> factors = new List<int>();
            string textfilepath = "factors.txt";
            int l;
            string temp_input;
            int counter = 0;

            //Delete Previous Keys in Database
            File.WriteAllText(textfilepath, "");

            Console.WriteLine("Welcome to the Quantum Prime Factoriser. Press Any Key to Continue... ");
            Console.ReadKey();

            //Restarts program for second factorisation
            start:
            factors.Clear();
            Console.WriteLine();
            counter += 1;

            //Takes Input from User to be Factorised
            input:
            Console.WriteLine();
            Console.Write(string.Format("Enter the Key {0} to be Factorised: ", counter));
            temp_input = Console.ReadLine();

            //Error Checking
            if (!int.TryParse(Convert.ToString(temp_input), out l)) { Console.WriteLine("Invalid Input. Only numbers can be factorised."); goto input; }
            numberToFactor = Convert.ToInt32(temp_input);
            if (numberToFactor > 30) { Console.WriteLine("This number is too large to factorise on a simulated quantum computer."); goto input; }
            if (numberToFactor%2 == 0 && (numberToFactor/2)%2 == 0) { Console.WriteLine("This number is invalid for the encryption algorithm."); goto input; }


                // Repeat Shor's algorithm, due to the probabilistic nature of quantum computers.
                for (int i = 0; i < nTrials; ++i)
            {
                try
                {
                    // Make sure to use simulator within using block. 
                    // This ensures that all resources used by QuantumSimulator
                    // are properly released if the algorithm fails and throws an exception.
                    using (QuantumSimulator sim = new QuantumSimulator())
                    {
                        Console.WriteLine($"==========================================");
                        Console.WriteLine($"Factoring {numberToFactor}");

                        retry:
                        // Compute the factors
                        (long factor1, long factor2) = Shor.Run(sim, numberToFactor, useRobustPhaseEstimation).Result;

                        if (isPrime(Convert.ToInt32(factor1)) && isPrime(Convert.ToInt32(factor2)))
                        {
                            Console.WriteLine($"Factors are {factor1} and {factor2}");
                            factors.Add(Convert.ToInt32(factor1));
                            factors.Add(Convert.ToInt32(factor2));
                        }
                        else { goto retry; }
                    }
                }
                // Shor's algorithm is a probabilistic algorithm and can fail with certain 
                // probability in several ways. For more details see Shor.qs.
                // If the run of Shor's algorithm fails it throws ExecutionFailException.
                // However, due to the use of System.Task in .Run method,
                // the exception of interest is getting wrapped into AggregateException.
                catch (AggregateException e)
                {
                    // Report the failure of the algorithm to standard output 
                    Console.WriteLine($"This run of Shor's algorithm failed:");

                    // Unwrap AggregateException to get the message from Q# fail statement.
                    // Go through all inner exceptions.
                    foreach (Exception eInner in e.InnerExceptions)
                    {
                        // If the exception of type ExecutionFailException
                        if (eInner is ExecutionFailException failException)
                        {
                            // Print the message it contains
                            Console.WriteLine($"   {failException.Message}");
                        }
                    }
                }
            }

            //Writing Factors to External Database
            factors.Sort();
            string[] factors2 = new string[3];
            factors2[0] = Convert.ToString(numberToFactor);
            factors2[1] = Convert.ToString(factors[0]);
            factors2[2] = Convert.ToString(factors[3]);
            File.AppendAllLines(textfilepath,factors2);

            if (counter != 2) { goto start; }

            Console.ReadKey();
        }
    }
}
using System;
using System.Collections.Generic;

namespace EvolutionaryAlgorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            Experiment2();
        }

        static void Experiment()
        {
            bool wrongInput = true;
            int intSelectionCycles = 0;
            int intGenerations = 0;

            // Simple dialogue to prompt user to enter desired amounts of selection cycles 
            // and generations to be executed and produced by the algorithm
            while (wrongInput)
            {
                Console.WriteLine("Set the number of selection cycles to be executed:");
                string selectionCycles = Console.ReadLine();
                Console.WriteLine("Set the number of generations to be evolved in every selection cycle:");
                string generations = Console.ReadLine();

                if (int.TryParse(selectionCycles, out intSelectionCycles) &&
                    int.TryParse(generations, out intGenerations))
                {
                    if (intSelectionCycles > 0 && intGenerations > 0)
                        wrongInput = false;
                    else
                        Console.WriteLine("Input was incorrect. Try again.");
                }
                else
                {
                    Console.WriteLine("Input was incorrect. Try again.");
                }
            }

            // Initializes the population based on experiment parameters
            // This is always initialized the same way, population is being 
            // shuffled inside the Selection class
            List<Member> initialPopulation = new List<Member>();
            for (int i = 0; i < 50; i++)
            {
                Member AMember = new Member('A', 1.5);
                Member BMember = new Member('B', 1.5);
                initialPopulation.Add(AMember);
                initialPopulation.Add(BMember);
            }

            // Initializes and executes selection experiment with given parameters
            Selection selection = new Selection(initialPopulation);
            selection.Experiment(intSelectionCycles, intGenerations);
        }

        static void Experiment2()
        {
            bool wrongInput = true;
            int intSelectionCycles = 0;
            int intGenerations = 0;
            int q = 0;

            // Simple dialogue to prompt user to enter desired amounts of selection cycles 
            // and generations to be executed and produced by the algorithm
            while (wrongInput)
            {
                Console.WriteLine("Set the number of selection cycles to be executed:");
                string selectionCycles = Console.ReadLine();
                Console.WriteLine("Set the number of generations to be evolved in every selection cycle:");
                string generations = Console.ReadLine();
                Console.WriteLine("Set the amount of competitors in tournaments:");
                string qString = Console.ReadLine();

                if (int.TryParse(selectionCycles, out intSelectionCycles) &&
                    int.TryParse(generations, out intGenerations) &&
                    int.TryParse(qString, out q))
                {
                    if (intSelectionCycles > 0 && intGenerations > 0 && q > 1 && q < 6)
                        wrongInput = false;
                    else
                        Console.WriteLine("Input was incorrect. Try again.");
                }
                else
                {
                    Console.WriteLine("Input was incorrect. Try again.");
                }
            }

            // Simple dialogue to prompt user to enter population type and specifications
            // Population is initialized based on choices made by the user
            wrongInput = true;
            List<Member> initialPopulation = null;
            string choice = "";
            double lower = 0, upper = 0, maximum = 0;
            double constant = 0;
            double[] parameters = null;
            while (wrongInput)
            {
                Console.WriteLine("Choose type of experiment:");
                Console.WriteLine("Options - linear, constant");
                choice = Console.ReadLine();

                if (choice.ToLower().Contains("lin"))
                {
                    Console.WriteLine("Specify lower and upper boundaries and the maximum fitness: ");
                    Console.Write("Lower boundary: ");
                    string lowerString = Console.ReadLine();
                    Console.Write("Upper bondary: ");
                    string upperString = Console.ReadLine();
                    Console.Write("Maximum fitness: ");
                    string maximumString = Console.ReadLine();
                    if (double.TryParse(lowerString, out lower) == false)
                    {
                        Console.WriteLine("Input was incorrect. Try again.");
                        continue;
                    }
                    if (double.TryParse(upperString, out upper) == false)
                    {
                        Console.WriteLine("Input was incorrect. Try again.");
                        continue;
                    }
                    if (double.TryParse(maximumString, out maximum) == false)
                    {
                        Console.WriteLine("Input was incorrect. Try again.");
                        continue;
                    }
                    initialPopulation = Initializer.InitializeLinearPopulation(lower, upper, maximum, 100);
                    parameters = new double[] { lower, upper, maximum };
                    wrongInput = false;
                }
                if (choice.ToLower().Contains("cons"))
                {
                    Console.WriteLine("Specify constant and maximum value: ");
                    Console.Write("Constant value: ");
                    string constantString = Console.ReadLine();
                    Console.Write("Maximum value: ");
                    string maximumString = Console.ReadLine();
                    if (double.TryParse(constantString, out constant) == false)
                    {
                        Console.WriteLine("Input was incorrect. Try again.");
                        continue;
                    }
                    if (double.TryParse(maximumString, out maximum) == false)
                    {
                        Console.WriteLine("Input was incorrect. Try again.");
                        continue;
                    }
                    initialPopulation = Initializer.InitializeConstantPopulation(constant, maximum, 100);
                    parameters = new double[] { constant, maximum };
                    wrongInput = false;
                }
            }
            QTournament tournamnet = new QTournament(q, initialPopulation, choice, parameters);
            tournamnet.Experiment(intSelectionCycles, intGenerations);
        }
    }
}

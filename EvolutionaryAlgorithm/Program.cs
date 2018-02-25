using System;
using System.Collections.Generic;

namespace EvolutionaryAlgorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            Experiment();
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
    }
}

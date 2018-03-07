using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace EvolutionaryAlgorithm
{
    public class Selection
    {
        // Lists containing initial and currently evolved population
        private List<Member> initialPopulation;
        private List<Member> currentPopulation;
        // Lists containing selection progress
        private List<double> majorityCount;
        private List<double> minorityCount;
        // Lists first generations fully affected by genetic drift
        private List<int> driftedGenerations;


        public Selection(List<Member> firstGeneration)
        {
            initialPopulation = firstGeneration;
            currentPopulation = new List<Member>();
            majorityCount = new List<double>();
            minorityCount = new List<double>();
            driftedGenerations = new List<int>();
        }

        // Used in every selection cycle to clone the initial population
        public static void CloneList(List<Member> newList, List<Member> oldList)
        {
            for(int i = 0; i < oldList.Count; i++)
            {
                newList.Add(new Member(oldList[i].Attribute, oldList[i].Fitness));
            }
        }

        public void Experiment(int numberOfSelections, int numberOfGenerations)
        {
            // Initializing fields
            for (int i = 0; i < numberOfGenerations; i++)
            {
                majorityCount.Add(0);
                minorityCount.Add(0);
            }

            // Execution of selection cycles
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < numberOfSelections; i++)
            {
                Console.WriteLine(i);
                ExecuteSelectionCycles(numberOfGenerations, out int firstGenerationAffectedByDrift);
                if (firstGenerationAffectedByDrift > 0)
                    driftedGenerations.Add(firstGenerationAffectedByDrift);

                currentPopulation.Clear();
            }
            watch.Stop();

            // Task 0.) total time needed for experiment
            double[] timeElapsed = new double[3];
            timeElapsed[0] = watch.Elapsed.Minutes;
            timeElapsed[1] = watch.Elapsed.Seconds;
            timeElapsed[2] = watch.Elapsed.Milliseconds;

            // Task 1.) chance of genetic drift happening
            double geneticDriftRatio = (double)driftedGenerations.Count / (double)numberOfSelections * 100;

            // Task 2.) first generation affected by drift on average
            double firstDriftedGeneration = (double)driftedGenerations.Sum() / (double)driftedGenerations.Count();

            // Task 2.5) median first generation affected by drift
            double firstDriftedGenerationMedian = CalculateMedian(driftedGenerations);

            // Task 2.75) earliest generation affected by drift
            int earliestDriftedGeneration = driftedGenerations.OrderBy(number => number).ToList()[0];

            // Task 3.) average progress of selection
            for (int i = 0; i < numberOfGenerations; i++)
            {
                majorityCount[i] /= numberOfSelections;
                minorityCount[i] /= numberOfSelections;
            }

            // Save results into a .txt file
            SaveResults(geneticDriftRatio, firstDriftedGeneration, firstDriftedGenerationMedian, numberOfSelections, 
                numberOfGenerations, timeElapsed, earliestDriftedGeneration);
        }

        // Calculates median value from a given data list
        private double CalculateMedian(List<int> data)
        {
            data = data.OrderBy(number => number).ToList();
            if (data.Count % 2 == 1)
                return data[(data.Count + 1) / 2];
            else
                return (data[data.Count / 2] + data[data.Count / 2 + 1]) / 2.0;
        }

        private void ExecuteSelectionCycles(int numberOfCycles, out int firstGenerationAffectedByDrift)
        {
            // Before every selection cycle, the initial population is cloned and shuffled to eliminate 
            // any effects of preliminary population settings
            CloneList(currentPopulation, initialPopulation);
            ShufflePopulation(currentPopulation);

            firstGenerationAffectedByDrift = -1;
            bool driftDetected = false;
            // Executes given amount of selections
            for (int i = 1; i <= numberOfCycles; i++)
            {
                currentPopulation = SelectParents(currentPopulation);
                if (GeneticDriftDetected() && driftDetected == false)
                {
                    driftDetected = true;
                    firstGenerationAffectedByDrift = i;
                }
                // New population data is stored for final output
                EvaluatePopulation(i - 1);
            }
        }

        // Randomly shuffles given population
        // This is used before every selection cycle
        private void ShufflePopulation(List<Member> currentPopulation)
        {
            Random random = new Random();

            for (int i = 0; i < currentPopulation.Count; i++)
            {
                int switcheroo = random.Next(0, 99);
                if (switcheroo != i)
                {
                    Member first = currentPopulation[i];
                    Member second = currentPopulation[switcheroo];
                    currentPopulation[i] = second;
                    currentPopulation[switcheroo] = first;
                }
            }
        }

        // Generates new parents population from current population
        private List<Member> SelectParents(List<Member> currentPopulation)
        {
            // Probability of every member to be selected as parent in the new generation is equal to
            // its fitness divived by sum of all fitness values members have
            List<Member> newGeneration = new List<Member>();
            List<double> numberOfParents = new List<double>();
            double fitnessSum = currentPopulation.Sum(member => member.Fitness);
            // This variable is equal to rho
            int parentsPopulationScale = currentPopulation.Count;

            // Calculates how many times is each member supposed to be chosen as parent
            // = eta values for current population members
            for (int i = 0; i < currentPopulation.Count; i++)
            {
                numberOfParents.Add((double)parentsPopulationScale * (double)currentPopulation[i].Fitness / (double)fitnessSum);
            }

            // Roulette is simulated by pseudo-random generator of a sequence of 100 bytes
            // All chosen numbers are generated at once and are afterwards normalised and scaled to
            // (0.0;rho) interval
            // This method was tested and produces no prefference for any value in the interval
            Random random = new Random();
            byte[] chosenBytes = new byte[parentsPopulationScale];
            random.NextBytes(chosenBytes);
            byte max = chosenBytes.ToList().Max();
            double[] chosenNumbers = new double[parentsPopulationScale];

            // Normalising and scaling the values to (0.0,rho) interval
            for(int i = 0; i < chosenBytes.Length; i++)
            {
                chosenNumbers[i] = (double)chosenBytes[i] / (double)max * (parentsPopulationScale - 1);
            }

            for(int i = 0; i < parentsPopulationScale; i++)
            {
                // Parts of roulette represent each of members in the order they have in current population
                // Parts are ordered clockwise on the roulette
                int identifiedParentIndex = IdentifyParent(numberOfParents, chosenNumbers[i]);
                newGeneration.Add(currentPopulation[identifiedParentIndex]);
            }

            return newGeneration;
        }

        private int IdentifyParent(List<double> numberOfParents, double chosenNumber)
        {
            int counter = 0;
            double cumulator = 0;
            // Incrementally searches the roulette clockwise to find
            // which member was chosen with the randomly generated number
            while (true)
            {
                cumulator += numberOfParents[counter];
                if (cumulator > chosenNumber)
                    return counter;
                else
                    counter++;
            }
        }

        // Evaluates one generation of population
        private void EvaluatePopulation(int generation)
        {
            // Calculates counts for both attribute values
            int ACount = currentPopulation.Count(member => member.Attribute == 'A');
            int BCount = currentPopulation.Count(member => member.Attribute == 'B');

            // Checks which attribute has the majority and increments counts in the 
            // appropriate data fields
            // These counts are used to produce average value as the total amount of generations
            // is known
            if (ACount > BCount)
            {
                majorityCount[generation] += ACount;
                minorityCount[generation] += BCount;
            }
            else
            {
                majorityCount[generation] += BCount;
                minorityCount[generation] += ACount;
            }
        }

        private bool GeneticDriftDetected()
        {
            // Genetic drift happens if genetic diversity in population was reduced to a single individual
            int ACount = currentPopulation.Count(member => member.Attribute == 'A');
            int BCount = currentPopulation.Count(member => member.Attribute == 'B');

            if (ACount == 0 || BCount == 0)
                return true;
            else
                return false;
        }

        // Saves results to a text file and opens the file in notepad for viewing
        private void SaveResults(double geneticDriftRatio, double firstDriftedGeneration, double firstDriftedGenerationMedian,
            int selectionCycles, int generations, double[] timeElapsed, int earliestDriftedGeneration)
        {
            string fileName = DateTime.Now.Year + "_" + DateTime.Now.Month.ToString("00") + "_" + DateTime.Now.Day.ToString("00") + "_" +
                DateTime.Now.Hour.ToString("00") + "_" + DateTime.Now.Minute.ToString("00") + "_" + DateTime.Now.Second.ToString("00") + ".txt";

            FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);

            writer.WriteLine("Experiment involving stochastic selection with replacement.");
            writer.WriteLine("Total time elapsed: " + timeElapsed[0].ToString("00") + ":" + timeElapsed[1].ToString("00") + ":" + timeElapsed[2].ToString("00") + " minutes.");
            writer.WriteLine("Number of selections executed: " + selectionCycles + ".");
            writer.WriteLine("Number of generations evolved in every selection process: " + generations + ".");
            writer.WriteLine("Average chance of genetic drift affecting new generations: " + geneticDriftRatio + "%.");
            writer.WriteLine("First generation fully affected by genetic drift on average: " + firstDriftedGeneration + ".");
            writer.WriteLine("Median first generation fully affected by genetic drift: " + firstDriftedGenerationMedian + ".");
            writer.WriteLine("Earliest generation fully affected by genetic drift: " + earliestDriftedGeneration + ".");

            writer.WriteLine("Average selection progress throughout all generations.");
            for (int i = 0; i < generations; i++)
            {
                writer.WriteLine("Generation " + (i + 1) + ": Majority individuals average count: " + majorityCount[i]
                    + " Minority individuals average count: " + minorityCount[i] + ".");
            }

            writer.Close();
            stream.Close();

            Process.Start(fileName);
        }
    }
}

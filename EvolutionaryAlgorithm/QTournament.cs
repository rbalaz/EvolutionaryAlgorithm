using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace EvolutionaryAlgorithm
{
    class QTournament
    {
        // tournament parameter setting the cardinality of members competing
        private int q;
        // data members representing population
        private List<Member> initialPopulation;
        private List<Member> currentPopulation;
        // statistics 
        private int successCount;
        private int partialSuccessCount;
        private int failureCount;
        // average development of successful takeover statistics
        private double twentyPercentAverage;
        private double fortyPercentAverage;
        private double sixtyPercentAverage;
        private double eightyPercentAverage;
        private double hundredPercentAverage;
        // population type parameters to provide more detailed output information
        private string populationType;
        private double[] parameters;

        public QTournament(int q, List<Member> firstGeneration, string populationType, double[] parameters)
        {
            this.q = q;
            this.populationType = populationType;
            this.parameters = parameters;
            initialPopulation = firstGeneration;
            currentPopulation = new List<Member>();
            successCount = 0;
            partialSuccessCount = 0;
            failureCount = 0;
            twentyPercentAverage = 0;
            fortyPercentAverage = 0;
            sixtyPercentAverage = 0;
            eightyPercentAverage = 0;
            hundredPercentAverage = 0;
        }

        public void Experiment(int selectionCycles, int generations)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < selectionCycles; i++)
            {
                ExecuteSelectionCycle(generations);
                currentPopulation.Clear();
                Console.WriteLine(i);
            }
            watch.Stop();

            // Evaluation tasks
            // Task 0.) total time needed for experiment
            double[] timeElapsed = new double[3];
            timeElapsed[0] = watch.Elapsed.Minutes;
            timeElapsed[1] = watch.Elapsed.Seconds;
            timeElapsed[2] = watch.Elapsed.Milliseconds;

            // Task 1.) average development of complete take over process
            twentyPercentAverage /= successCount;
            fortyPercentAverage /= successCount;
            sixtyPercentAverage /= successCount;
            eightyPercentAverage /= successCount;
            hundredPercentAverage /= successCount;

            // Save generated results to a file
            SaveResults(selectionCycles,generations,timeElapsed);
        }

        private void ExecuteSelectionCycle(int generations)
        {
            // Initial population is filled as 0 generation into current population
            // which is then shuffled
            CloneList(currentPopulation, initialPopulation);
            ShufflePopulation(currentPopulation);

            // There are 5 possible thresholds that can occur during the selection
            // If all 5 thresholds are reached, it means a full take over occured
            List<int> thresholds = new List<int>();
            double currentRatio = PercentageEvaluation();

            // Execute defined amount of tournament cycles 
            for (int i = 0; i < generations; i++)
            {
                currentPopulation = ExecuteQTournaments(currentPopulation.Count);

                currentRatio = PercentageEvaluation();
                while (thresholds.Count < (int)currentRatio / 20)
                {
                    thresholds.Add(i + 1);
                }
            }

            if (thresholds.Count == 5)
            {
                twentyPercentAverage += thresholds[0];
                fortyPercentAverage += thresholds[1];
                sixtyPercentAverage += thresholds[2];
                eightyPercentAverage += thresholds[3];
                hundredPercentAverage += thresholds[4];
            }

            EvaluatePopulation();
        }

        // Used in every selection cycle to clone the initial population
        public static void CloneList(List<Member> newList, List<Member> oldList)
        {
            for (int i = 0; i < oldList.Count; i++)
            {
                newList.Add(new Member(oldList[i].Attribute, oldList[i].Fitness));
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

        private List<Member> ExecuteQTournaments(int amountOfParents)
        {
            List<Member> parentsPopulation = new List<Member>();
            Random random = new Random();

            for (int i = 0; i < amountOfParents; i++)
            {
                // Step 1: choose q random members from current population
                int[] indices = new int[q];
                for (int j = 0; j < q; j++)
                    indices[j] = random.Next(0, currentPopulation.Count);

                // Step 2: calculate tournament winner
                int maximumFitnessIndex = indices[0];
                for (int j = 1; j < q; j++)
                {
                    if (currentPopulation[indices[j]].Fitness > currentPopulation[maximumFitnessIndex].Fitness)
                        maximumFitnessIndex = indices[j];
                }

                // Step 3: assign tournament winner to parents population
                parentsPopulation.Add(currentPopulation[maximumFitnessIndex]);
            }

            return parentsPopulation;
        }

        // Returns in ration of population already taken by the dominant member
        private double PercentageEvaluation()
        {
            double maxFitness = initialPopulation.Max(member => member.Fitness);
            int maxFitnessCount = currentPopulation.Count(member => member.Fitness == maxFitness);
            return (double)maxFitnessCount / (double)currentPopulation.Count * 100.0;
        }

        private void EvaluatePopulation()
        {
            // Check how many copies does the most fitting member have
            double maxFitness = initialPopulation.Max(member => member.Fitness);
            int maxFitnessCount = currentPopulation.Count(member => member.Fitness == maxFitness);
            // Possible situations:
            // a) dominant member has completely taken over the population
            if (maxFitnessCount == currentPopulation.Count)
                successCount++;
            // b) dominant member failed to completely taken over the population
            else if (maxFitnessCount > 0)
                partialSuccessCount++;
            // c) dominant member died out
            else
                failureCount++;
        }

        private void SaveResults(int selectionCycles, int generations, double[] timeElapsed)
        {
            string fileName = DateTime.Now.Year + "_" + DateTime.Now.Month.ToString("00") + "_" + DateTime.Now.Day.ToString("00") + "_" +
                 DateTime.Now.Hour.ToString("00") + "_" + DateTime.Now.Minute.ToString("00") + "_" + DateTime.Now.Second.ToString("00") + ".txt";

            FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);

            writer.WriteLine("Experiment involving q-tournament with replacement with monitoring take-over effect.");
            writer.WriteLine("Total time elapsed: " + timeElapsed[0].ToString("00") + ":" + timeElapsed[1].ToString("00") + ":" + timeElapsed[2].ToString("00") + " minutes.");
            writer.WriteLine("Parameter q value: " + q + ".");
            writer.WriteLine("Number of selections executed: " + selectionCycles + ".");
            writer.WriteLine("Number of generations evolved in every selection process: " + generations + ".");
            writer.WriteLine();
            writer.WriteLine("Information about population");
            if (parameters.Length == 2)
            {
                writer.WriteLine("Population consisting of 99 members with fitness " + parameters[0] + " and one " +
                    "individual with fitness " + parameters[1] + ".");
            }
            else if (parameters.Length == 3)
            {
                writer.WriteLine("Population consisting of 99 members with linearly increasing fitness from value " +
                    parameters[0] + " to fitness value " + parameters[1] + ".");
                writer.WriteLine("The final individual has fitness " + parameters[2] + ".");
            }
            writer.WriteLine();
            writer.WriteLine("Number of successful take-overs: " + successCount + ".");
            writer.WriteLine("Number of partially successful take-overs: " + partialSuccessCount + ".");
            writer.WriteLine("Number of failures: " + failureCount + ".");
            writer.WriteLine("The following results were calculated based on data aquired from successful take-overs.");
            writer.WriteLine("Average first generation affected by at least 20% take-over: " + twentyPercentAverage + ".");
            writer.WriteLine("Average first generation affected by at least 40% take-over: " + fortyPercentAverage + ".");
            writer.WriteLine("Average first generation affected by at least 60% take-over: " + sixtyPercentAverage + ".");
            writer.WriteLine("Average first generation affected by at least 80% take-over: " + eightyPercentAverage + ".");
            writer.WriteLine("Average first generation affected by at least 100% take-over: " + hundredPercentAverage + ".");

            writer.Close();
            stream.Close();

            Process.Start(fileName);
        }
    }
}
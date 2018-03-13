using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EvolutionaryAlgorithm
{
    class Cut
    {
        private List<Member> initialPopulation;
        private List<Member> currentPopulation;

        // Stores for each member how many times it survived during the selection cycles
        private List<Tuple<Member, int>> survivorsMap;
        // Stores values for average material preservation
        private double materialLoss;

        public Cut(List<Member> initialPopulation)
        {
            this.initialPopulation = initialPopulation;
            currentPopulation = new List<Member>(initialPopulation.Count);

            // Fills survivors map with initial data
            survivorsMap = new List<Tuple<Member, int>>(initialPopulation.Count);
            foreach (Member member in initialPopulation)
            {
                Member clone = Member.CloneMember(member);
                Tuple<Member, int> mapItem = new Tuple<Member, int>(clone, 0);
                survivorsMap.Add(mapItem);
            }
            materialLoss = 0;
        }

        public void Experiment(int selectionCycles, int generations, double threshold, bool replacement)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < selectionCycles; i++)
            {
                ExecuteSelectionCycle(generations, threshold, replacement);
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

            // Task 1.) average material loss
            materialLoss /= selectionCycles;

            // Task 2.) average single member selection loss
            double selectionLoss = 0;
            foreach (Tuple<Member, int> mapItem in survivorsMap)
            {
                selectionLoss += (double)(selectionCycles - mapItem.Item2) / (double)selectionCycles;
            }
            selectionLoss /= initialPopulation.Count;

            SaveResults(selectionCycles, generations, selectionLoss, replacement, threshold, timeElapsed);
        }

        private void ExecuteSelectionCycle(int generations, double threshold, bool replacement)
        {
            CloneList(currentPopulation, initialPopulation);

            for (int i = 0; i < generations; i++)
            {
                CutPopulation(threshold, replacement);
            }

            EvaluatePopulation();
        }

        private void CutPopulation(double threshold, bool replacement)
        {
            // Before cutting, population needs to ordered by fitness in ascending order
            currentPopulation.OrderBy(member => member.Fitness);

            // All members who have their cumulative distribution function value
            // lower than (1-T)*ni cannot be picked as parents
            // Indexing from 0 needs to be taken into account
            int cutIndex = (int)Math.Ceiling((1-threshold)*currentPopulation.Count);
            List<Member> survivors = currentPopulation.Skip(cutIndex).ToList();

            List<Member> newPopulation = new List<Member>(currentPopulation.Count); 
            // Parents are selected using selection method specified by replacement parameter
            if (replacement == false)
            {
                // Stochastic selection without replacement
                List<int> remainingSelections = new List<int>(survivors.Count);
                for (int i = 0; i < survivors.Count; i++)
                {
                    // Every member that survived the cut is chosen with the exact same probability
                    // Since population of parents and the actual population has the same amount
                    // of members, the resulting expected amount of parents for each member
                    // is rho/(ni*T) => rho == ni => 1/T
                    // Every member can have at most Ceil(1/T)
                    remainingSelections.Add((int)Math.Ceiling(1.0 / threshold));
                }
                Random random = new Random();
                for (int i = 0; i < currentPopulation.Count; i++)
                {
                    bool correctMemberChosen = false;
                    int chosenMember = -1;
                    while (!(correctMemberChosen))
                    {
                        chosenMember = random.Next(0, survivors.Count);
                        if (remainingSelections[chosenMember] > 0)
                            correctMemberChosen = true;
                    }
                    // Once a member with remaining parent places has be selected, it is placed
                    // in the new population and its parent count is decremented by 1
                    Member newMember = Member.CloneMember(survivors[chosenMember]);
                    newPopulation.Add(newMember);
                    remainingSelections[chosenMember] -= 1;
                }
            }
            else
            {
                // Stochastic selection with replacement
                Random random = new Random();
                // Replacement selection simply resembles a random number generator
                // that is run as many times as many parents are needed to be chosen
                for (int i = 0; i < currentPopulation.Count; i++)
                {
                    int chosenMember = random.Next(0, survivors.Count);
                    Member newMember = Member.CloneMember(survivors[chosenMember]);
                    newPopulation.Add(newMember);
                }
            }

            currentPopulation = newPopulation;
        }

        private void EvaluatePopulation()
        {
            List<Member> uniquePopulation = new List<Member>();
            // Checks how many unique individuals are present in population
            int diversityCount = 0;
            for (int i = 0; i < currentPopulation.Count; i++)
            {
                bool matchFound = false;
                for (int j = i + 1; j < currentPopulation.Count; j++)
                {
                    if (Member.CompareMembers(currentPopulation[i], currentPopulation[j]))
                    {
                        matchFound = true;
                        break;
                    }
                }
                if (matchFound == false)
                    diversityCount++;

                // Constructs a list of members that omits any clones generated during selection
                // Used in the second statistics
                matchFound = false;
                foreach (Member member in uniquePopulation)
                {
                    if (Member.CompareMembers(member, currentPopulation[i]))
                        matchFound = true;
                }
                if (matchFound == false)
                    uniquePopulation.Add(currentPopulation[i]);
            }
            materialLoss += (double)(currentPopulation.Count - diversityCount) / (double)currentPopulation.Count;

            // Check which members specifically survived
            foreach (Member member in uniquePopulation)
            {
                for(int i = 0; i < survivorsMap.Count; i++)
                {
                    Tuple<Member, int> mapItem = survivorsMap[i];
                    if (Member.CompareMembers(mapItem.Item1, member))
                    {
                        Tuple<Member, int> newItem = new Tuple<Member, int>(mapItem.Item1, mapItem.Item2 + 1);
                        survivorsMap[i] = newItem;
                    }
                }
            }
        }

        // Saves results to a text file and opens the file in notepad for viewing
        private void SaveResults(int selectionCycles, int generations, double selectionLoss, bool replacement, 
            double threshold, double[] timeElapsed)
        {
            string fileName = DateTime.Now.Year + "_" + DateTime.Now.Month.ToString("00") + "_" + DateTime.Now.Day.ToString("00") + "_" +
                DateTime.Now.Hour.ToString("00") + "_" + DateTime.Now.Minute.ToString("00") + "_" + DateTime.Now.Second.ToString("00") + ".txt";

            FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);

            string selectionType = replacement ? "with replacement" : "without replacement";
            writer.WriteLine("Experiment involing cutting and using stoachstic selection " + selectionType + ".");
            writer.WriteLine("Threshold value used: " + threshold + ".");
            writer.WriteLine("Total time elapsed: " + timeElapsed[0].ToString("00") + ":" + timeElapsed[1].ToString("00") + ":" + timeElapsed[2].ToString("00") + " minutes.");
            writer.WriteLine("Number of selections executed: " + selectionCycles + ".");
            writer.WriteLine("Number of generations evolved in every selection process: " + generations + ".");
            writer.WriteLine("Average material loss: " + materialLoss + ".");
            writer.WriteLine("Average selection loss of individual members: " + selectionLoss + ".");
            writer.WriteLine();
            writer.WriteLine(">>>Individual selection ratio for members<<<");
            foreach (Tuple<Member, int> survivor in survivorsMap)
            {
                writer.WriteLine("Member with fitness " + survivor.Item1.Fitness + " was selected in " +
                    100.0*(double)survivor.Item2 / (double)selectionCycles + "% cases.");
            }

            writer.Close();
            stream.Close();

            Process.Start(fileName);
        }

        // Used in every selection cycle to clone the initial population
        public static void CloneList(List<Member> newList, List<Member> oldList)
        {
            for (int i = 0; i < oldList.Count; i++)
            {
                newList.Add(new Member(oldList[i].Attribute, oldList[i].Fitness));
            }
        }
    }
}

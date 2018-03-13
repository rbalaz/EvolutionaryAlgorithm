using System;
using System.Collections.Generic;

namespace EvolutionaryAlgorithm
{
    class Initializer
    {
        public static List<Member> InitializeConstantPopulation(double constantFitness, double topFitness, int count)
        {
            List<Member> population = new List<Member>();

            for (int i = 0; i < count; i++)
            {
                if (i == count / 2)
                {
                    Member member = new Member(topFitness);
                    population.Add(member);
                }
                else
                {
                    Member member = new Member(constantFitness);
                    population.Add(member);
                }
            }

            return population;
        }

        public static List<Member> InitializeLinearPopulation(double lowerFitness, double upperFitness, double maxFitness, 
            int count)
        {
            // 1 -> fi = lowerFitness
            // count - 1 -> fi = upperFitness
            // y = y1 + m*(x - x1)
            // m = (y2 - y1)/(x2 - x1)
            List<Member> population = new List<Member>();

            for (int i = 0; i < count - 1; i++)
            {
                double slope = (upperFitness - lowerFitness) / (count - 2);
                double fitness = lowerFitness + slope * i;
                Member member = new Member(fitness);
                population.Add(member);
            }
            population.Add(new Member(maxFitness));

            return population;
        }

        public static List<Member> InitializeNonLinearPopulation(int count)
        {
            // fi(ai) = sqrt(i)
            List<Member> population = new List<Member>();

            for (int i = 0; i < count; i++)
            {
                Member member = new Member(Math.Sqrt(i + 1));
                population.Add(member);
            }

            return population;
        }
    }
}

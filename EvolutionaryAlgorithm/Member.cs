namespace EvolutionaryAlgorithm
{
    public class Member
    {
        // Attribute value for generation member
        public char Attribute { get; private set; }
        // Fitness value for generation member
        public double Fitness { get; private set; }

        public Member(char type, double fitness)
        {
            Attribute = type;
            Fitness = fitness;
        }

        public Member(double fitness)
        {
            Fitness = fitness;
        }
    }
}

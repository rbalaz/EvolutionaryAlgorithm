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

        public static Member CloneMember(Member member)
        {
            return new Member(member.Fitness);
        }

        public static bool CompareMembers(Member first, Member second)
        {
            if (first.Fitness == second.Fitness)
                return true;
            else
                return false;
        }
    }
}

namespace LinqLab
{
    public class Student
    {
        public string Name          { get; set; }
        public string Group         { get; set; }
        public int    Age           { get; set; }
        public double GPA           { get; set; }
        public int    PassedCredits { get; set; }

        public override string ToString()
        {
            return $"{Name,-10} | {Group,-6} | {Age,2} | {GPA,4:F1} | {PassedCredits,2}";
        }
    }
}
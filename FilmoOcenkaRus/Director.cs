namespace FilmoOcenkaRus
{
    public class Director
    {
        public string name { get; set; }
        public int movieCount { get; set; }
        public double avarageRating { get; set; }
        public string years { get; set; }

        public Director(string name, int movieCount, double avarageRating, string years)
        {
            this.name = name;
            this.movieCount = movieCount;
            this.avarageRating = avarageRating;
            this.years = years;
        }
    }
}

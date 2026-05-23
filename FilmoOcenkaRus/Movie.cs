namespace FilmoOcenkaRus
{
    public class Movie
    {
        public string title { get; set; }
        public string genre { get; set; }
        public string director { get; set; }
        public int year { get; set; }
        public double rating { get; set; }

        public Movie(string title, string genre, int year, string director, double rating)
        {
            this.title = title;
            this.genre = genre;
            this.director = director;
            this.year = year;
            this.rating = rating;
        }
    }
}

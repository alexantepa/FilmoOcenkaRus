using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;

namespace FilmoOcenkaRus
{
    public class MainViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Movie> allMovies { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<Movie> FilteredMovie { get; set; } = new ObservableCollection<Movie>();
        public ObservableCollection<Director> Directors { get; set; } = new ObservableCollection<Director>();
        public ObservableCollection<string> Genres { get; set; } = new ObservableCollection<string>()
        {
            "Все",
            "Комедия",
            "Драма",
            "Триллер",
            "Боевик",
            "Ужасы",
            "Фантастика"
        };

        public Movie _selectedMovie;
        public Movie selectedMovie
        {
            get => _selectedMovie;
            set
            {
                _selectedMovie = value;
                OnPropertyChanged(nameof(selectedMovie));
            }
        }

        //Ввод
        public string NewTitle { get; set; } = "";
        public string NewGenre { get; set; } = "";
        public string NewDirector { get; set; } = "";
        public string NewYear { get; set; } = "";
        public string NewRating { get; set; } = "";

        //Фильтры
        public string SelectedGenre { get; set; } = "Все";
        public string YearFrom { get; set; } = "";
        public String YearTo { get; set; } = "";

        //Команды
        public ICommand addMovie { get; }
        public ICommand deleteMovie { get; }
        public ICommand showAllMovies { get; }
        public ICommand applyGenreFilter { get; }
        public ICommand searchByYear { get; }
        public ICommand sortToBigger { get; }
        public ICommand sortToSmaller { get; }
        public ICommand exportToExcel { get; }

        public MainViewModel()
        {
            addMovie = new RelayCommand(AddMovie);
            deleteMovie = new RelayCommand(DeleteMovie);
            showAllMovies = new RelayCommand(ShowAll);
            applyGenreFilter = new RelayCommand(ApplyGenreFilter);
            searchByYear = new RelayCommand(SearchByYear);
            sortToBigger = new RelayCommand(() => SortByRating(true));
            sortToSmaller = new RelayCommand(() => SortByRating(false));
            exportToExcel = new RelayCommand(ExportToExcel);

            LoadTestData();
        }

        private void LoadTestData()
        {
            allMovies.Add(new Movie("Интерстеллар", "Фантастика", 2014, "Кристофер Нолан", 8.7));
            allMovies.Add(new Movie("Начло", "Фантастика", 2010, "Кристофер Нолан", 8.7));
            allMovies.Add(new Movie("Побег из Шоушенга", "Драма", 1994, "Фрэнк Дарабонт", 9.1));
            allMovies.Add(new Movie("Дьявол носит Prada 2", "Драма", 2026, "Дэвид Фрэнкел", 6.9));
            allMovies.Add(new Movie("Вершина", "Боевик", 2026, "Бальтасар Кормакур", 6.2));
            allMovies.Add(new Movie("Вершина", "Боевик", 2026, "Бальтасар Кормакур", 6.2));
            allMovies.Add(new Movie("Королек мой любви", "Комедия", 2026, "Марюс Вайсберг", 5.9));
            allMovies.Add(new Movie("Горничная", "Триллер", 2025, "Пол Фиг", 7.7));

            UpdateGenres();
            ShowAll();
            UpdateDirectors();
        }

        public void AddMovie()
        {
            if (string.IsNullOrWhiteSpace(NewTitle) || string.IsNullOrWhiteSpace(NewGenre)) return;

            int year = int.TryParse(NewYear, out int y) ? y : DateTime.Now.Year;
            double rating = double.TryParse(NewRating, out double r) ? r : 5.0;

            allMovies.Add(new Movie(NewTitle, NewGenre, year, NewDirector, rating));

            ClearAddFilters();
            ShowAll();
            UpdateDirectors();
        }

        public void DeleteMovie()
        {
            if (selectedMovie == null) return;
            allMovies.Remove(selectedMovie);
            ShowAll();
            UpdateDirectors();
        }

        public void ShowAll()
        {
            FilteredMovie.Clear();
            foreach (var movie in allMovies)
            {
                FilteredMovie.Add(movie);
            }
        }

        public void ApplyGenreFilter()
        {
            if (SelectedGenre == "Все")
            {
                ShowAll();
                return;
            }

            FilteredMovie.Clear();
            foreach (var m in allMovies.Where(m => m.genre == SelectedGenre))
                FilteredMovie.Add(m);
        }

        public void SearchByYear()
        {
            if (!int.TryParse(YearFrom, out int fout) || !int.TryParse(YearTo, out int fto))
            {
                ShowAll();
                return;
            }

            FilteredMovie.Clear();
            foreach (var m in allMovies.Where(m => m.year >= fout && m.year <= fto))
                FilteredMovie.Add(m);
        }

        public void SortByRating(bool ascending)
        {
            var sorted = ascending
                ? FilteredMovie.OrderBy(m => m.rating).ToList()
                : FilteredMovie.OrderByDescending(m => m.rating).ToList();
            FilteredMovie.Clear();
            foreach (var m in sorted)
                FilteredMovie.Add(m);
        }

        private void UpdateGenres()
        {
            Genres.Clear();
            Genres.Add("Все");
            foreach (var g in allMovies.Select(m => m.genre).Distinct().OrderBy(x => x))
                Genres.Add(g);
        }

        public void UpdateDirectors()
        {
            Directors.Clear();
            var group = allMovies.GroupBy(m => m.director);

            foreach (var g in group)
            {
                Directors.Add(new Director(g.Key, g.Count(), Math.Round(g.Average(m => m.rating), 2),
                    $"{g.Min(m => m.year)}-{g.Max(m => m.year)}"));
            }
        }

        private void ClearAddFilters()
        {
            NewTitle=NewYear= NewGenre = NewDirector = NewRating = "";

            OnPropertyChanged(nameof(NewTitle));
            OnPropertyChanged(nameof(NewYear));
            OnPropertyChanged(nameof(NewGenre));
            OnPropertyChanged(nameof(NewDirector));
            OnPropertyChanged(nameof(NewRating));
        }

        public void ExportToExcel()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = "csv",
                FileName = "movies.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var sb = new StringBuilder();
                // Заголовки
                sb.AppendLine("Название;Жанр;Год;Режиссер;Рейтинг");
                
                // Данные всех фильмов
                foreach (var movie in allMovies)
                {
                    sb.AppendLine($"{movie.title};{movie.genre};{movie.year};{movie.director};{movie.rating}");
                }

                File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
            }
        }
        
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

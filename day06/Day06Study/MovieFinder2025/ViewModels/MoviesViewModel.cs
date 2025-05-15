using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MovieFinder2025.Helpers;
using MovieFinder2025.Models;
using MovieFinder2025.Views;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Threading;

namespace MovieFinder2025.ViewModels
{
    public partial class MoviesViewModel : ObservableObject
    {

        private readonly IDialogCoordinator dialogCoordinator;

        public MoviesViewModel(IDialogCoordinator coordinator)
        {
            this.dialogCoordinator = coordinator;

            Common.LOGGER.Info("MovieFinder2025 Start.");

            PosterUri = new Uri("/No_Picture.png", UriKind.RelativeOrAbsolute);

            // 시계작업
            CurrDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");    // 1초가 지나기전에 화면 출력을 위해

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);  // 1초마다 변경
            _timer.Tick += (sender, e) =>
            {
                CurrDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            _timer.Start();
        }

        // ViewModel내에서만 사용
        private string _base_url = "https://image.tmdb.org/t/p/w300_and_h450_bestv2";
        private readonly DispatcherTimer _timer;    

        private string _currDateTime;

        public string CurrDateTime
        {
            get => _currDateTime;
            set => SetProperty(ref _currDateTime, value);
        }

        private string _searchResult;

        public string SearchResult
        {
            get => _searchResult;
            set => SetProperty(ref _searchResult, value);
        }


        private string _moviename;

        public string MovieName {
            get => _moviename;
            set => SetProperty(ref _moviename, value);
        }

        private ObservableCollection<MovieItem> _movieItems;

        public ObservableCollection<MovieItem> MovieItems
        {
            get => _movieItems;
            set => SetProperty(ref _movieItems, value);
        }

        private MovieItem _selectedMovieItem;

        public MovieItem SelectedMovieItem
        {
            get => _selectedMovieItem;
            set
            {
                SetProperty(ref _selectedMovieItem, value);
                Common.LOGGER.Info($"Selected Movie Item > {_base_url}{value.Poster_path}");
                PosterUri = new Uri($"{_base_url}{value.Poster_path}", UriKind.Absolute);
            }
        }

        private Uri _posterUri;
        public Uri PosterUri
        {
            get => _posterUri;
            set => SetProperty(ref _posterUri, value);
        }
               


        [RelayCommand]
        public async Task SearchMovie()
        {
            //await this.dialogCoordinator.ShowMessageAsync(this, "영화검색", Moviename);
            if (string.IsNullOrEmpty(_moviename))
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "영화검색", "영화명을 입력하세요!");
                return;
            }

            var controller = await dialogCoordinator.ShowProgressAsync(this, "대기중", "검색 중 ..");
            controller.SetIndeterminate();
            SearchMovie(MovieName);
            await Task.Delay(1000);
            await controller.CloseAsync();
        }

        private async Task SearchMovie(string moviename)
        {
            string tmdb_apikey = "4b20af0a77f20667244c7777a416d514";    // TMDB에서 신청한 API키
            string encoding_movieName = HttpUtility.UrlEncode(moviename,Encoding.UTF8);
            string openApiUri = $"https://api.themoviedb.org/3/search/movie?api_key={tmdb_apikey}" +
                                $"&language=ko-KR&page=1&include_adult=false&query={encoding_movieName}";
            //Debug.WriteLine(openApiUri);
            Common.LOGGER.Info($"TMDB URI : {openApiUri}");

            string result = string.Empty;

            // openAPI 실행할 웹 객체 : WebRequest, WebResponse -> Deprecated 추후 삭제될 예정
            // HttpClient로 대체 할 것
            //WebRequest req = null;
            //WebResponse res = null;
            var client = new HttpClient();
            ObservableCollection<MovieItem> movieItems = new ObservableCollection<MovieItem>();

            string reader; // 응담을 받은 결과를 담는 객체

            try
            {
                //response = await client.GetAsync(openApiUri);
                var response = await client.GetFromJsonAsync<MovieSearchResponse>(openApiUri);

                foreach (MovieItem movie in response.Results)
                {
                    //Common.LOGGER.Info($"{movie.Title}, {movie.Release_Date.ToString("yyyy-MM-dd")}");
                    movieItems.Add(movie);
                }

                SearchResult = $"영화 검색 건수 : {response.Total_results}건";
                Common.LOGGER.Info(MovieName + " / " + SearchResult + "검색 완료");
            }
            catch (Exception ex)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "예외", "API 요청 실패 !");
                Common.LOGGER.Fatal(ex.Message);
                SearchResult = "오류 발생";
            }

            MovieItems = movieItems;
        }

        [RelayCommand]
        public async Task MovieItemDoubleClick()
        {
            var currMovie = SelectedMovieItem;
            if (currMovie != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"{currMovie.Original_title} ({currMovie.Release_date.ToString("yyyy-MM-dd")})\n");
                sb.Append($"평점 : {currMovie.Vote_count:F2}\n\n");
                sb.Append(currMovie.Overview);

                Common.LOGGER.Info($"{SelectedMovieItem.Title} 영화 정보 확인");

                await this.dialogCoordinator.ShowMessageAsync(this, "영화 정보", sb.ToString());
            }
            else
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "알림", "선택된 영화가 없습니다.");
            }
        }

        [RelayCommand]
        public async Task AddFavoriteMovies()
        {
            //await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기 추가", "");
            if (SelectedMovieItem == null)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기 추가", "추가할 영화를 선택하세요");
                return;
            }

            try
            {
                var query = @"INSERT INTO movieitems
                            (id, adult, backdrop_path, original_language, original_title, overview,
                            popularity, poster_path, release_date, title, vote_average, vote_count)
                            VALUES
                            (@id, @adult, @backdrop_path, @original_language, @original_title, @overview,
                            @popularity,  @poster_path, @release_date, @title, @vote_average, @vote_count)";

                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", SelectedMovieItem.Id);
                    cmd.Parameters.AddWithValue("@adult", SelectedMovieItem.Adult);
                    cmd.Parameters.AddWithValue("@backdrop_path", SelectedMovieItem.Backdrop_path);
                    cmd.Parameters.AddWithValue("@original_language", SelectedMovieItem.Original_language);
                    cmd.Parameters.AddWithValue("@original_title", SelectedMovieItem.Original_title);
                    cmd.Parameters.AddWithValue("@overview", SelectedMovieItem.Overview);
                    cmd.Parameters.AddWithValue("@popularity", SelectedMovieItem.Popularity);
                    cmd.Parameters.AddWithValue("@poster_path", SelectedMovieItem.Poster_path);
                    cmd.Parameters.AddWithValue("@release_date", SelectedMovieItem.Release_date);
                    cmd.Parameters.AddWithValue("@title", SelectedMovieItem.Title);
                    cmd.Parameters.AddWithValue("@vote_average", SelectedMovieItem.Vote_average);
                    cmd.Parameters.AddWithValue("@vote_count", SelectedMovieItem.Vote_count);

                    var resultCnt = cmd.ExecuteNonQuery();

                    if(resultCnt > 0)
                    {
                        Common.LOGGER.Info($"{SelectedMovieItem.Title} 즐겨찾기 추가");
                        await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기추가", "즐겨찾기 추가 성공");
                    }
                    else
                    {
                        await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기추가", "즐겨찾기 추가 실패");
                    }
                }
            }
            catch(MySqlException ex)
            {
                if(ex.Message.ToUpper().Contains("DUPLICATE ENTRY")){
                    Common.LOGGER.Warn($"{SelectedMovieItem.Title} 이미 추가된 영화");

                    await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기추가", "이미 추가된 즐겨찾기입니다.");
                }
                await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
                Common.LOGGER.Fatal(ex.Message);
            }
            catch (Exception ex)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
                Common.LOGGER.Fatal(ex.Message);
            }


        }

        [RelayCommand]
        public async Task ViewFavoriteMovies()
        {
            ObservableCollection<MovieItem> movieItems = new ObservableCollection<MovieItem>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();

                    var query = @"select id, adult, backdrop_path, original_language, original_title, overview,
                                popularity, poster_path, release_date, title, vote_average, vote_count
                                FROM movieitems";

                    MySqlCommand cmd = new MySqlCommand(query,conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        movieItems.Add(new MovieItem
                        {
                            Id = reader.GetInt32("id"),
                            Adult = reader.GetBoolean("adult"),
                            Backdrop_path = reader.IsDBNull(2)?string.Empty : reader.GetString("backdrop_path"),
                            Original_language = reader.GetString("original_language"),
                            Original_title = reader.GetString("original_title"),
                            Overview = reader.GetString("overview"),
                            Popularity = reader.GetDouble("popularity"),
                            Poster_path = reader.GetString("poster_path"),
                            Release_date = reader.GetDateTime("release_date"),
                            Title = reader.GetString("title"),
                            Vote_average = reader.GetDouble("vote_average"),
                            Vote_count = reader.GetInt32("vote_count"),
                        });
                    }

                }

                MovieItems = movieItems;
                SearchResult = $"즐겨찾기 검색 건수 : {MovieItems.Count}건";
                Common.LOGGER.Info(SearchResult + " 검색완료");
            }
            catch (Exception ex)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
                Common.LOGGER.Fatal(ex.Message);
            }
        }

        [RelayCommand]
        public async Task DelFavoriteMovies()
        {
            if (SelectedMovieItem == null)
            {
                Common.LOGGER.Info($"{SelectedMovieItem.Title} 즐겨찾기 삭제");
                await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기 삭제", "삭제할 영화를 선택하세요");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(Common.CONNSTR))
                {
                    conn.Open();
                    var query = @"DELETE FROM movieitems WHERE id = @id";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", SelectedMovieItem.Id);

                    var resultCnt = cmd.ExecuteNonQuery();

                    if (resultCnt > 0)
                    {
                        await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기삭제", "즐겨찾기 삭제 성공");
                    }
                    else
                    {
                        await this.dialogCoordinator.ShowMessageAsync(this, "즐겨찾기삭제", "즐겨찾기 삭제 실패");
                    }
                }
            }
            catch (Exception ex)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "오류", ex.Message);
                Common.LOGGER.Fatal(ex.Message);
            }

            await ViewFavoriteMovies(); // 삭제 후 즐겨찾기를 다시보기
        }

        [RelayCommand]
        public async Task ViewMovieTrailer()
        {
            if (SelectedMovieItem == null)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "예고편 보기","영화를 선택하세요");
                return;
            }

            var movieTitle = SelectedMovieItem.Title;

            var viewModel = new TrailerViewModel(Common.DIALOGCOODINATOR, movieTitle);
            viewModel.MovieTitle = movieTitle;
            var view = new TrailerView
            {
                DataContext = viewModel,
            };
            view.Owner = Application.Current.MainWindow;    // 부모창의 정중앙위치

            Common.LOGGER.Info($"{SelectedMovieItem.Title} 유튜브 트레일러 실행");
            view.ShowDialog();
        }

    }
}

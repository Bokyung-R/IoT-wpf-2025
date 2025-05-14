using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MovieFinder2025.Helpers;
using MovieFinder2025.Models;
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

namespace MovieFinder2025.ViewModels
{
    public partial class MoviesViewModel : ObservableObject
    {

        private readonly IDialogCoordinator dialogCoordinator;

        public MoviesViewModel(IDialogCoordinator dialogCoordinator)
        {
            this.dialogCoordinator = dialogCoordinator;
            
            Common.LOGGER.Info("MovieFinder2025 Start.");

            PosterUri = new Uri("/No_Picture.png", UriKind.RelativeOrAbsolute);
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
                Common.LOGGER.Info($"Selected Movie Item {value.Poster_path}");
                PosterUri = new Uri($"{_base_url}{value.Poster_path}", UriKind.Absolute);
            }
        }

        private string _base_url = "https://image.tmdb.org/t/p/w300_and_h450_bestv2";

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
                    Common.LOGGER.Info($"{movie.Title}, {movie.Release_Date.ToString("yyyy-MM-dd")}");
                    movieItems.Add(movie);
                }
            }
            catch (Exception ex)
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "예외", "API 요청 실패 !");
                Common.LOGGER.Fatal(ex.Message);
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
                sb.Append($"{currMovie.Original_title} ({currMovie.Release_Date.ToString("yyyy-MM-dd")})\n");
                sb.Append($"평점 : {currMovie.Vote_count:F2}\n\n");
                sb.Append(currMovie.Overview);

                await this.dialogCoordinator.ShowMessageAsync(this, "영화 정보", sb.ToString());
            }
            else
            {
                await this.dialogCoordinator.ShowMessageAsync(this, "알림", "선택된 영화가 없습니다.");
            }
        }

    }
}

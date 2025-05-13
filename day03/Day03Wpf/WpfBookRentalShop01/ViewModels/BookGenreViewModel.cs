using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using WpfBookRentalShop01.Models;

namespace WpfBookRentalShop01.ViewModels
{
    public partial class BookGenreViewModel : ObservableObject
    {
        private ObservableCollection<Genre> _genres;
        public ObservableCollection<Genre> Genres
        {
            get => _genres;
            set => SetProperty(ref _genres, value);
        }

        private Genre _selectedGenre;
        public Genre SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                SetProperty(ref _selectedGenre, value);
                _isUpdate = true;  // 수정할 상태
            }
        }

        private bool _isUpdate;

        public BookGenreViewModel()
        {
            SelectedGenre = new Genre();
            SelectedGenre.Names = string.Empty;
            SelectedGenre.Division = string.Empty;
            // 순서가 중요
            _isUpdate = false; // 신규 상태

            LoadGridFromDb();
        }

        private void LoadGridFromDb()
        {
            try
            {
                string connectionString = "Server=localhost;Database=bookrentalshop;Uid=root;Pwd=12345;Charset=utf8;";
                string query = "SELECT division, names FROM divtbl";

                ObservableCollection<Genre> genres = new ObservableCollection<Genre>();

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var division = reader.GetString("division");
                        var names = reader.GetString("names");

                        genres.Add(new Genre
                        {
                            Division = division,
                            Names = names
                        });
                    }
                }

                Genres = genres;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        // SetInitCommand, SaveDataCommand, DelDataCommand
        [RelayCommand]
        public void SetInit()
        {
            _isUpdate = false;
            SelectedGenre = new Genre();
            SelectedGenre.Names = string.Empty;
            SelectedGenre.Division = string.Empty;
        }

        [RelayCommand]
        public void SaveData()
        {
            //// 신규추가 / 기존데이터 수정
            //Debug.WriteLine(SelectedGenre.Names);
            //Debug.WriteLine(SelectedGenre.Division);
            //Debug.WriteLine(_isUpdate);

            try
            {
                string connectionString = "Server=localhost;Database=bookrentalshop;Uid=root;Pwd=12345;Charset=utf8;";
                string query = string.Empty;

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    if (_isUpdate)  // 기존 데이터 수정
                    {
                        query = "UPDATE divtbl SET names = @names WHERE division = @division";
                    }
                    else  // 신규 등록
                    {
                        query = "INSERT INTO divtbl VALUES (@division, @names)";
                    }
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@division", SelectedGenre.Division);
                    cmd.Parameters.AddWithValue("@names", SelectedGenre.Names);

                    int resultCnt = cmd.ExecuteNonQuery();

                    if (resultCnt > 0)
                    {
                        MessageBox.Show("저장성공!~");
                    }
                    else
                    {
                        MessageBox.Show("저장실패!!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            LoadGridFromDb();   // 저장이 끝난 후 다시 DB내용을 그리드에 그리기

        }

        [RelayCommand]
        public void DelData()
        {
            if (_isUpdate == false)
            {
                MessageBox.Show("선택된 데이터가 아니면 삭제할 수 없습니다.");
                return;
            }

            try
            {
                string connectionString = "Server=localhost;Database=bookrentalshop;Uid=root;Pwd=12345;Charset=utf8;";
                string query = "DELETE FROM divtbl WHERE division = @division";

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@division", SelectedGenre.Division);

                    int resultCnt = cmd.ExecuteNonQuery(); // 한건 삭제가되면 resultCnt = 1, 안지워지면 resultCnt = 0

                    if (resultCnt > 0)
                    {
                        MessageBox.Show("삭제성공!~");
                    }
                    else
                    {
                        MessageBox.Show("삭제실패!!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            LoadGridFromDb();

        }
    }
}
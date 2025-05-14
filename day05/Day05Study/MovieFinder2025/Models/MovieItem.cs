using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace MovieFinder2025.Models
{
    public partial class MovieItem : ObservableObject
    {
        private bool _adult;
        private string _backdrop_path;
        private List<int> _genre_ids;
        private int _id;
        private string _original_language;
        private string _original_title;
        private string _overview;
        private double _popularity;
        private string _poster_path;
        private DateTime _release_date;
        private string _title;
        private bool _video;
        private double _vote_average;
        private int _vote_count;

        public bool Adult
        {
            get => _adult;
            set => SetProperty(ref _adult, value);
        }

        public string Backdrop_path
        {
            get => _backdrop_path;
            set => SetProperty(ref _backdrop_path, value);
        }

        public List<int> Genre_ids
        {
            get => _genre_ids;
            set => SetProperty(ref _genre_ids, value);
        }

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Original_language
        {
            get => _original_language;
            set => SetProperty(ref _original_language, value);
        }

        public string Original_title
        {
            get => _original_title;
            set => SetProperty(ref _original_title, value);
        }

        public string Overview
        {
            get => _overview;
            set => SetProperty(ref _overview, value);
        }

        public double Popularity
        {
            get => _popularity;
            set => SetProperty(ref _popularity, value);
        }

        public string Poster_path
        {
            get => _poster_path;
            set => SetProperty(ref _poster_path, value);
        }

        public DateTime Release_Date
        {
            get => _release_date;
            set => SetProperty(ref _release_date, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool Video
        {
            get => _video;
            set => SetProperty(ref _video, value);
        }

        public double Vote_average
        {
            get => _vote_average;
            set => SetProperty(ref _vote_average, value);
        }

        public int Vote_count
        {
            get => _vote_count;
            set => SetProperty(ref _vote_count, value);
        }
    }
}

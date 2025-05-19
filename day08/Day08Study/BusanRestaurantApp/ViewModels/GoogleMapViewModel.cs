using BusanRestaurantApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BusanRestaurantApp.ViewModels
{
    public class GoogleMapViewModel : ObservableObject
    {
        private BusanItem _selectedMatjbItem;
        private string _matjibLocation;

        public GoogleMapViewModel()
        {
            MatjibLocation = "";            
        }

        public BusanItem SelectedMatjbItem
        {
            get => _selectedMatjbItem;
            set {
                SetProperty(ref _selectedMatjbItem, value);
                // 위도(Latitude/Lat), 경도(Longitude/Lng)
                MatjibLocation = $"https://google.com/maps/place/{SelectedMatjbItem.Lat},{SelectedMatjbItem.Lng}";
            }
        }

        public string MatjibLocation
        {
            get => _matjibLocation;
            set => SetProperty(ref _matjibLocation, value);
        }


    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using StafflyApp.Helpers;

namespace StafflyApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isAdmin;

        [ObservableProperty]
        private bool _isHR;

        public MainViewModel()
        {
            CheckPermissions();
        }

        private void CheckPermissions()
        {
            var currentUser = UserSession.Instance;
            IsAdmin = currentUser.RoleID == 1;
            IsHR = currentUser.RoleID == 1 || currentUser.RoleID == 2;
        }
    }
}
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cronophobia
{
    public partial class SettingsWindow : Window
    {
        private bool _isLoadingProfile;
        private readonly MainWindow _main;

        private ProfilesContainer _profiles = null!;
        private ProfileSettings _currentProfile = null!;
        private ProfileSettings _workingCopy = null!;

        public SettingsWindow(MainWindow main)
        {
            InitializeComponent();
            var screenWidth  = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            // centro de pantalla + desplazamiento a la derecha
            Left = (screenWidth - Width) / 2 + 200;
            Top  = (screenHeight - Height) / 2;

            _main = main;

            Closed += SettingsWindow_Closed;

            LoadOctoberCrowFont();
            LoadProfiles();
            HookEvents();
        }

        private void SettingsWindow_Closed(object? sender, EventArgs e)
        {
            // Forzar reconstrucción del Z-Order
            _main.Topmost = false;
            _main.Topmost = true;

            _main.Activate();
        }


        // ================= LOAD PROFILES =================
        private void LoadProfiles()
        {
            _profiles = SettingsService.Load();

            SettingsService.Save(_profiles);

            ProfilesCombo.ItemsSource = _profiles.Profiles;
            ProfilesCombo.DisplayMemberPath = "ProfileName";

            _currentProfile = _profiles.Profiles
                .FirstOrDefault(p => p.ProfileName == _profiles.ActiveProfileName)
                ?? _profiles.Profiles.First();

            ProfilesCombo.SelectedItem = _currentProfile;

            _workingCopy = _currentProfile.Clone();
            LoadValues();
        }

        // ================= FONT =================
        private void LoadOctoberCrowFont()
        {
            var uri = new Uri("pack://application:,,,/Assets/Fonts/");
            var font = new FontFamily(uri, "./#October Crow");
            SettingsTitle.FontFamily = font;
        }

        // ================= LOAD VALUES =================
        private void LoadValues()
        {
            ShowTitleCheck.IsChecked = _workingCopy.ShowTitle;
            ShowIconsCheck.IsChecked = _workingCopy.ShowIcons;

            TitleSizeSlider.Value = _workingCopy.TitleFontSize;
            TimerSizeSlider.Value = _workingCopy.TimerFontSize;

            ColorCombo.SelectedIndex = _workingCopy.TextColor switch
            {
                "Red" => 1,
                "Green" => 2,
                "Blue" => 3,
                _ => 0
            };
        }

        // ================= EVENTS =================
        private void HookEvents()
        {
            ProfilesCombo.SelectionChanged += ProfileChanged;

            ShowTitleCheck.Checked += AnyChanged;
            ShowTitleCheck.Unchecked += AnyChanged;

            ShowIconsCheck.Checked += AnyChanged;
            ShowIconsCheck.Unchecked += AnyChanged;

            TitleSizeSlider.ValueChanged += AnyChanged;
            TimerSizeSlider.ValueChanged += AnyChanged;

            ColorCombo.SelectionChanged += AnyChanged;
        }

        // ================= PROFILE CHANGED =================
        private void ProfileChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfilesCombo.SelectedItem is not ProfileSettings profile)
                return;

            _isLoadingProfile = true;

            _currentProfile = profile;
            _workingCopy = profile.Clone();

            _profiles.ActiveProfileName = profile.ProfileName;
            SettingsService.Save(_profiles);

            LoadValues();          // solo UI
            _isLoadingProfile = false;

            ApplyPreview();        // UNA sola aplicación
            ReloadCurrentProfile();
        }

        // ================= LIVE PREVIEW =================
        private void AnyChanged(object sender, RoutedEventArgs e)
        {
            if (_isLoadingProfile)
                return;

            UpdateWorkingCopy();
            ApplyPreview();
        }


        private void UpdateWorkingCopy()
        {
            _workingCopy.ShowTitle = ShowTitleCheck.IsChecked == true;
            _workingCopy.ShowIcons = ShowIconsCheck.IsChecked == true;

            _workingCopy.TitleFontSize = TitleSizeSlider.Value;
            _workingCopy.TimerFontSize = TimerSizeSlider.Value;

            if (ColorCombo.SelectedItem is ComboBoxItem item)
            {
                _workingCopy.TextColor = item.Content.ToString() switch
                {
                    "Red" => "Red",
                    "Green" => "Green",
                    "Blue" => "Blue",
                    _ => "White"
                };
            }
        }

        private void ApplyPreview()
        {
            _main.ApplyProfile(_workingCopy);
        }

        // ================= BUTTONS =================
       private void Save_Click(object sender, RoutedEventArgs e)
        {
            // capturar posición ACTUAL del cronómetro
            _workingCopy.WindowLeft = _main.Left;
            _workingCopy.WindowTop  = _main.Top;

            if (_currentProfile.IsDefault)
            {
                if (_profiles.Profiles.Count(p => !p.IsDefault) >= 4)
                    return;

                // ===== MOSTRAR VENTANA NOMBRE PERFIL (SIN ROMPER FOCO) =====
                bool wasTopMost = _main.Topmost;
                _main.Topmost = false;

                var dialog = new NamePromptWindow
                {
                    Owner = this
                };

                bool? result = dialog.ShowDialog();

                // ===== RECUPERAR OVERLAY DEL CRONÓMETRO =====
                _main.Topmost = true;
                _main.Activate();
                _main.Focus();
                _main.Topmost = wasTopMost;

                if (result != true)
                    return;

                var name = dialog.Result;
                if (string.IsNullOrWhiteSpace(name))
                    return;
                // ==========================================================

                var newProfile = _workingCopy.Clone();
                newProfile.ProfileName = name;
                newProfile.IsDefault = false;

                _profiles.Profiles.Add(newProfile);
                _currentProfile = newProfile;
            }
            else
            {
                _currentProfile.CopyFrom(_workingCopy);
            }

            _profiles.ActiveProfileName = _currentProfile.ProfileName;
            SettingsService.Save(_profiles);

            // refrescar combo inmediatamente
            ProfilesCombo.Items.Refresh();
            ProfilesCombo.SelectedItem = _currentProfile;
        }


        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProfile.IsDefault)
                return;

            _profiles.Profiles.Remove(_currentProfile);

            _currentProfile = _profiles.Profiles.First();
            _profiles.ActiveProfileName = _currentProfile.ProfileName;

            SettingsService.Save(_profiles);

            ProfilesCombo.ItemsSource = null;
            ProfilesCombo.ItemsSource = _profiles.Profiles;
            ProfilesCombo.SelectedItem = _currentProfile;

            _workingCopy = _currentProfile.Clone();
            LoadValues();
            _main.ApplyProfile(_currentProfile);
        }


        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ReloadCurrentProfile();

            // segundo pase forzado (layout + Top)
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ReloadCurrentProfile();

                // recuperar foco del cronómetro
                _main.Activate();   
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

        }
        private void ReloadCurrentProfile()
        {
            _isLoadingProfile = true;

            _workingCopy = _currentProfile.Clone();
            LoadValues();

            _isLoadingProfile = false;

            ApplyPreview();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

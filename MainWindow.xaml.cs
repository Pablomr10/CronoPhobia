using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Forms;

namespace Cronophobia
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;

    public void ForceToFront()
{
    var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
    ShowWindow(handle, SW_RESTORE);
    SetForegroundWindow(handle);
}

        // ================= PROFILES =================
        private ProfilesContainer _profiles = null!;
        private ProfileSettings _activeProfile = null!;

        private SettingsWindow? _settingsWindow;

        // ================= TIMER =================
        private readonly TimerService _timerService = new();

        // ================= HOTKEYS =================
        private const int HOTKEY_ID_START = 1;
        private const int HOTKEY_ID_STOP  = 2;
        private const int HOTKEY_ID_RESET = 3;
        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // ================= CONSTRUCTOR =================
        public MainWindow()
        {
            InitializeComponent();

            _profiles = SettingsService.Load();

            if (_profiles.Profiles.Count == 0)
                _profiles = SettingsService.Load(); // fallback seguro


            _activeProfile = _profiles.Profiles
                .FirstOrDefault(p => p.ProfileName == _profiles.ActiveProfileName)
                ?? _profiles.Profiles.First();

            ApplyProfile(_activeProfile);

            Loaded += (_, _) => RestoreWindowPosition();

            LoadOctoberCrowFont();
            HookTimer();

            LocationChanged += (_, _) => SaveWindowPositionToProfile();

            Topmost = true;
        }

        // ================= WINDOW POSITION =================
        private void RestoreWindowPosition()
        {
            var x = _activeProfile.WindowLeft;
            var y = _activeProfile.WindowTop;

            bool insideAnyScreen = Screen.AllScreens.Any(s =>
                x >= s.WorkingArea.Left &&
                x <= s.WorkingArea.Right &&
                y >= s.WorkingArea.Top &&
                y <= s.WorkingArea.Bottom);

            if (insideAnyScreen)
            {
                Left = x;
                Top  = y;
            }
            else
            {
                // Centrar en pantalla principal si estaba fuera
                Left = SystemParameters.PrimaryScreenWidth / 2 - ActualWidth / 2;
                Top  = SystemParameters.PrimaryScreenHeight / 2 - ActualHeight / 2;
            }
        }



        // ================= FONT =================
        private void LoadOctoberCrowFont()
        {
            var uri = new Uri("pack://application:,,,/Assets/Fonts/");
            var font = new FontFamily(uri, "./#October Crow");

            TitleText.FontFamily = font;
            TimerText.FontFamily = font;
            IconPlay.FontFamily  = font;
            IconStop.FontFamily  = font;
            IconReset.FontFamily = font;
        }

        // ================= TIMER =================
        private void HookTimer()
        {
            _timerService.TimeUpdated += time =>
            {
                Dispatcher.Invoke(() => TimerText.Text = time);
            };
        }

        // ================= ICON STATE =================
        private void SetActiveIcon(string action)
        {
            IconPlay.Opacity = IconStop.Opacity = IconReset.Opacity = 0.3;

            if (action == "start") IconPlay.Opacity = 1;
            if (action == "stop")  IconStop.Opacity = 1;
            if (action == "reset") IconReset.Opacity = 1;
        }

        // ================= HOTKEYS =================
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd)?.AddHook(WndProc);

            RegisterHotKey(hwnd, HOTKEY_ID_START, 0, 0x37);
            RegisterHotKey(hwnd, HOTKEY_ID_STOP,  0, 0x38);
            RegisterHotKey(hwnd, HOTKEY_ID_RESET, 0, 0x39);
        }

        private IntPtr WndProc(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            if (msg != WM_HOTKEY) return IntPtr.Zero;

            switch (wParam.ToInt32())
            {
                case HOTKEY_ID_START:
                    _timerService.Start();
                    SetActiveIcon("start");
                    break;

                case HOTKEY_ID_STOP:
                    _timerService.Stop();
                    SetActiveIcon("stop");
                    break;

                case HOTKEY_ID_RESET:
                    _timerService.Reset();
                    SetActiveIcon("reset");
                    break;
            }

            handled = true;
            return IntPtr.Zero;
        }

        // ================= DRAG =================
        private void Window_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        // ================= SETTINGS WINDOW =================
        private void OpenSettings(object sender, MouseButtonEventArgs e)
        {
            if (_settingsWindow != null) return;

            _settingsWindow = new SettingsWindow(this)
            {
                Owner = this,
                ShowActivated = true
            };

            _settingsWindow.Closed += (_, _) =>
            {
                _settingsWindow = null;
                ForceToFront(); // 👈 AQUÍ ESTÁ LA CLAVE
            };

            _settingsWindow.Show();
        }


        // ================= APPLY PROFILE =================
        public void ApplyProfile(ProfileSettings profile)
        {
            _activeProfile = profile;

            // APLICAR POSICIÓN DEL PERFIL (AQUÍ ESTABA EL FALLO)
            if (profile.WindowLeft > 0 || profile.WindowTop > 0)
            {
                Left = profile.WindowLeft;
                Top  = profile.WindowTop;
            }

            TitleText.Visibility = profile.ShowTitle
                ? Visibility.Visible
                : Visibility.Collapsed;

            IconPlay.Visibility =
            IconStop.Visibility =
            IconReset.Visibility = profile.ShowIcons
                ? Visibility.Visible
                : Visibility.Collapsed;

            TitleText.FontSize = profile.TitleFontSize;
            TimerText.FontSize = profile.TimerFontSize;

            var brush = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(profile.TextColor));

            TitleText.Foreground = brush;
            TimerText.Foreground = brush;
            IconPlay.Foreground  = brush;
            IconStop.Foreground  = brush;
            IconReset.Foreground = brush;

            // aplicar posición DEL PERFIL (horizontal y vertical juntas)
            if (profile.WindowLeft > 0 && profile.WindowTop > 0)
            {
                Left = profile.WindowLeft;
                Top  = profile.WindowTop;
            }

        }



        private void SaveWindowPositionToProfile()
        {
            if (_activeProfile == null)
                return;

            _activeProfile.WindowLeft = Left;
            _activeProfile.WindowTop  = Top;
        }


        // ================= CLEANUP =================
        protected override void OnClosed(EventArgs e)
        {
           if (!_activeProfile.IsDefault)
            {
                _activeProfile.WindowLeft = Left;
                _activeProfile.WindowTop  = Top;
            }

            _profiles.ActiveProfileName = _activeProfile.ProfileName;
            SettingsService.Save(_profiles);

            var hwnd = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(hwnd, HOTKEY_ID_START);
            UnregisterHotKey(hwnd, HOTKEY_ID_STOP);
            UnregisterHotKey(hwnd, HOTKEY_ID_RESET);

            base.OnClosed(e);
        }

    }
}

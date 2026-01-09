namespace Cronophobia
{
    public class ProfileSettings : AppSettings
    {
        public string ProfileName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }

        public ProfileSettings()
        {
        }

        public ProfileSettings Clone()
        {
            return new ProfileSettings
            {
                ProfileName = this.ProfileName,
                IsDefault = this.IsDefault,

                // AppSettings
                ShowTitle = this.ShowTitle,
                ShowIcons = this.ShowIcons,
                TitleFontSize = this.TitleFontSize,
                TimerFontSize = this.TimerFontSize,
                TextColor = this.TextColor,
                WindowLeft = this.WindowLeft,
                WindowTop = this.WindowTop
            };
        }

        public void CopyFrom(ProfileSettings other)
        {
            ProfileName = other.ProfileName;
            IsDefault = other.IsDefault;

            // AppSettings
            ShowTitle = other.ShowTitle;
            ShowIcons = other.ShowIcons;
            TitleFontSize = other.TitleFontSize;
            TimerFontSize = other.TimerFontSize;
            TextColor = other.TextColor;
            WindowLeft = other.WindowLeft;
            WindowTop = other.WindowTop;
        }
    }
}

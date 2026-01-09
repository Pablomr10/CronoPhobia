using System.Windows.Media;
using System.Windows;

namespace Cronophobia
{
    public class SettingsState
    {
        public double TitleFontSize { get; set; }
        public double TimerFontSize { get; set; }

        public bool ShowTitle { get; set; }
        public bool ShowIcons { get; set; }

        public Color TextColor { get; set; }

        public Point WindowPosition { get; set; }

        public SettingsState Clone()
        {
            return (SettingsState)MemberwiseClone();
        }
    }
}

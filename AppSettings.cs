public class AppSettings
{
    public bool ShowTitle { get; set; } = true;
    public bool ShowIcons { get; set; } = true;

    public double TitleFontSize { get; set; } = 48;
    public double TimerFontSize { get; set; } = 84;

    public string TextColor { get; set; } = "White";

    public double WindowLeft { get; set; }
    public double WindowTop { get; set; }

    public void CopyFrom(AppSettings other)
    {
        ShowTitle = other.ShowTitle;
        ShowIcons = other.ShowIcons;
        TitleFontSize = other.TitleFontSize;
        TimerFontSize = other.TimerFontSize;
        TextColor = other.TextColor;
        WindowLeft = other.WindowLeft;
        WindowTop = other.WindowTop;
    }
}

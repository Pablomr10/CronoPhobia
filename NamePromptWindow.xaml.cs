using System.Windows;

namespace Cronophobia
{
    public partial class NamePromptWindow : Window
    {
        public string? Result { get; private set; }

       public NamePromptWindow()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                Topmost = true;     // mantenerla siempre delante
                Activate();
                NameBox.Focus();
            };

            Closed += (_, _) =>
            {
                Topmost = false;    // soltar TopMost al cerrar
            };
        }


        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
                return;

            Result = NameBox.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

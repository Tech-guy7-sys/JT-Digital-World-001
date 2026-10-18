using System.Windows;

namespace AgentJeff;

public partial class VoiceCompanionWindow : Window
{
    public VoiceCompanionWindow()
    {
        InitializeComponent();
    }

    private void Hide_Click(object sender, RoutedEventArgs e) => Close();
}
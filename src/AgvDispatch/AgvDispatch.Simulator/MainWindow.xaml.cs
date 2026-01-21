using System.Windows;

namespace AgvDispatch.Simulator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeAgvPanels();
    }

    private void InitializeAgvPanels()
    {
        AgvPanel1.SetDefaultAgvId("V001");
        AgvPanel2.SetDefaultAgvId("V002");
        AgvPanel3.SetDefaultAgvId("V003");
        AgvPanel4.SetDefaultAgvId("V099");
    }

    protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        await AgvPanel1.CleanupAsync();
        await AgvPanel2.CleanupAsync();
        await AgvPanel3.CleanupAsync();
        await AgvPanel4.CleanupAsync();
        base.OnClosing(e);
    }
}

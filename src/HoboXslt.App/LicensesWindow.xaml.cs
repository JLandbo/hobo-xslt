using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using HoboXslt.Core.Languages;

namespace HoboXslt.App;

public partial class LicensesWindow : Window
{
    private readonly Translator _translator;

    public LicensesWindow(Translator translator)
    {
        _translator = translator;
        InitializeComponent();
    }

    private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        e.Handled = true;
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch (Win32Exception ex)
        {
            MessageBox.Show(_translator.Format("Document.OpenFailed", e.Uri.AbsoluteUri, ex.Message), "hobo-xslt", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

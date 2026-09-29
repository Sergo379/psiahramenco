using System.IO;
using System.Threading;
using System.Windows;

namespace ExamTicketGenerator;

public partial class App : Application
{
    private Mutex? _instance;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if (e.Args.Contains("--create-demo"))
        {
            DemoData.Create(AppContext.BaseDirectory);
            Shutdown();
            return;
        }
        if (e.Args.Contains("--verify-ui"))
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            try { UiVerification.Run(AppContext.BaseDirectory); Shutdown(0); }
            catch (Exception ex) { File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "ui-verification-error.txt"), ex.ToString()); Shutdown(1); }
            return;
        }
        _instance = new Mutex(true, "Local\\LoftTicketStudio", out var firstInstance);
        if (!firstInstance)
        {
            MessageBox.Show("Ticket Studio уже запущен. Переключитесь в открытое окно.", "LOFT / STUDIO");
            Shutdown();
            return;
        }
        new MainWindow().Show();
    }
    protected override void OnExit(ExitEventArgs e)
    {
        _instance?.Dispose();
        base.OnExit(e);
    }
}

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ExamTicketGenerator;

// Run against an isolated directory: validates WPF states and renders real controls.
internal static class UiVerification
{
    public static void Run(string output)
    {
        var isolated = Path.Combine(Path.GetTempPath(), "ticket-studio-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(isolated);
        try
        {
            var missing = new MainWindow(isolated);
            missing.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            Check(!((Button)missing.FindName("GenerateButton")).IsEnabled, "missing sources");
            Check(File.Exists(Path.Combine(isolated, "results.xlsx")), "journal created at startup");
            missing.Close();
            DemoData.Create(isolated);
            var window = new MainWindow(isolated);
            window.ShowInTaskbar = false;
            window.Opacity = 0;
            window.Show();
            window.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Loaded);
            var groups = (ComboBox)window.FindName("GroupCombo");
            var students = (ComboBox)window.FindName("StudentCombo");
            var button = (Button)window.FindName("GenerateButton");
            Check(groups.Items.Count == 3 && !button.IsEnabled, "initial state");
            groups.SelectedIndex = 2;
            Check(students.Items.Count == 0 && !button.IsEnabled, "empty group");
            groups.SelectedIndex = 0;
            Check(students.Items.Count == 4 && !button.IsEnabled, "group selection");
            students.SelectedIndex = 0;
            Check(button.IsEnabled, "student selection");
            Render((FrameworkElement)window.Content, 1120, 748, Path.Combine(output, "preview-main.png"));
            for (var attempt = 0; attempt < 2; attempt++)
            {
                students.SelectedIndex = 0;
                var resultSeen = false;
                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                timer.Tick += (_, _) =>
                {
                    var visible = Application.Current.Windows.OfType<TicketWindow>().FirstOrDefault();
                    if (visible is null) return;
                    using (var saved = new ClosedXML.Excel.XLWorkbook(Path.Combine(isolated, "results.xlsx")))
                        Check(saved.Worksheet(1).LastRowUsed()!.RowNumber() == attempt + 2, "save before showing ticket");
                    resultSeen = true;
                    visible.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(visible)!, 0, Key.Escape) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
                };
                timer.Start();
                try { button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); }
                finally { timer.Stop(); }
                Check(resultSeen && students.SelectedIndex == -1, "real generation and return to selection: " + ((TextBlock)window.FindName("StatusText")).Text);
            }
            using (var history = new ClosedXML.Excel.XLWorkbook(Path.Combine(isolated, "results.xlsx")))
            {
                Check(history.Worksheet(1).Cell(2, 4).GetValue<int>() == history.Worksheet(1).Cell(3, 4).GetValue<int>(), "repeat keeps number");
                Check(history.Worksheet(1).Cell(3, 6).GetString() == "да", "repeat appended");
            }
            students.SelectedIndex = 0;
            var ticket = TicketsReader.Read(Path.Combine(isolated, "tickets.docx"))[0];
            var result = new TicketWindow(ticket, new Student("Иванов", "Александр"), false);
            Render((FrameworkElement)result.Content, 690, 660, Path.Combine(output, "preview-ticket.png"));
            result.ShowInTaskbar = false;
            result.Opacity = 0;
            result.Show();
            var closed = false;
            result.Closed += (_, _) => closed = true;
            result.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(result)!, 0, Key.Escape) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
            Check(closed, "Escape closes result");
            Check(button.IsEnabled, "main window remains available");
            window.Close();
            File.WriteAllText(Path.Combine(output, "ui-verification.txt"), "PASS: missing sources, journal at startup, initial state, empty group, dependent lists, button states, real generation, immediate save, repeat history, result layout, Escape.\n");
        }
        finally { Directory.Delete(isolated, true); }
    }
    private static void Check(bool condition, string name) { if (!condition) throw new InvalidOperationException("UI check failed: " + name); }
    private static void Render(FrameworkElement element, int width, int height, string path)
    {
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(0, 0, width, height));
        element.UpdateLayout();
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}

using DayTrack.Services;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace DayTrack;

public partial class App : System.Windows.Application
{
    private Mutex? _mutex;
    private EventWaitHandle? _activateEvent;
    private CancellationTokenSource? _activationCancel;
    private AppHost? _host;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;

        try
        {
            RuntimeLog.Write($"Process start | exe={Environment.ProcessPath}");

            _mutex = new Mutex(
                true,
                @"Local\DayTrack.SingleInstance",
                out var created);

            if (!created)
            {
                RuntimeLog.Write("Second instance detected; requesting existing widget.");

                try
                {
                    using var existing =
                        EventWaitHandle.OpenExisting(@"Local\DayTrack.Activate");
                    existing.Set();
                }
                catch (Exception ex)
                {
                    RuntimeLog.WriteException("Could not signal existing instance", ex);
                    System.Windows.MessageBox.Show(
                        "DayTrack is already running in the background.",
                        "DayTrack");
                }

                Shutdown();
                return;
            }

            _activateEvent = new EventWaitHandle(
                false,
                EventResetMode.AutoReset,
                @"Local\DayTrack.Activate");

            _activationCancel = new CancellationTokenSource();

            _ = Task.Run(() => ActivationLoop(_activationCancel.Token));

            _host = new AppHost();
            _host.Start(e.Args);
        }
        catch (Exception ex)
        {
            RuntimeLog.WriteException("Fatal startup error", ex);

            System.Windows.MessageBox.Show(
                $"DayTrack could not start.\n\n{ex.Message}\n\nLog:\n{RuntimeLog.FilePath}",
                "DayTrack startup error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
        }
    }

    private void ActivationLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                _activateEvent?.WaitOne();

                if (token.IsCancellationRequested)
                    break;

                Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        try
                        {
                            _host?.ShowWidget();
                        }
                        catch (Exception ex)
                        {
                            RuntimeLog.WriteException("Activate existing widget", ex);
                        }
                    }));
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                RuntimeLog.WriteException("Activation loop", ex);
                Thread.Sleep(250);
            }
        }
    }

    private void OnDispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e)
    {
        RuntimeLog.WriteException("Unhandled UI error", e.Exception);

        System.Windows.MessageBox.Show(
            $"DayTrack error:\n\n{e.Exception.Message}\n\nLog:\n{RuntimeLog.FilePath}",
            "DayTrack error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        try
        {
            _activationCancel?.Cancel();
            _activateEvent?.Set();
        }
        catch { }

        _host?.Dispose();

        try { _activateEvent?.Dispose(); } catch { }
        try { _activationCancel?.Dispose(); } catch { }

        try { _mutex?.ReleaseMutex(); } catch { }
        _mutex?.Dispose();

        RuntimeLog.Write("Process exit.");

        base.OnExit(e);
    }
}

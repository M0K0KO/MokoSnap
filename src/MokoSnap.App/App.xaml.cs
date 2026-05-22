using System.Windows;

namespace MokoSnap.App;

public partial class App : System.Windows.Application
{
    private const string SingleInstanceMutexName = @"Local\MokoSnap.SingleInstance";
    private const string ActivationEventName = @"Local\MokoSnap.ActivateExistingInstance";
    private Mutex? _singleInstanceMutex;
    private EventWaitHandle? _activationEvent;
    private CancellationTokenSource? _activationListenerCts;
    private bool _ownsSingleInstanceMutex;

    protected override async void OnStartup(StartupEventArgs e)
    {
        if (!TryClaimSingleInstance())
        {
            Shutdown(0);
            return;
        }

        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        StartActivationListener();
        MainWindow window = new();
        MainWindow = window;
        bool startMinimized = e.Args.Any(arg => arg.Equals("--minimized", StringComparison.OrdinalIgnoreCase));
        await window.StartAsync(startMinimized);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _activationListenerCts?.Cancel();
        _activationEvent?.Dispose();
        _activationListenerCts?.Dispose();
        if (_ownsSingleInstanceMutex)
        {
            _singleInstanceMutex?.ReleaseMutex();
        }

        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }

    private bool TryClaimSingleInstance()
    {
        _singleInstanceMutex = new Mutex(true, SingleInstanceMutexName, out bool createdNew);
        _ownsSingleInstanceMutex = createdNew;
        if (createdNew)
        {
            _activationEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ActivationEventName);
            return true;
        }

        _singleInstanceMutex.Dispose();
        _singleInstanceMutex = null;
        SignalExistingInstance();
        return false;
    }

    private static void SignalExistingInstance()
    {
        try
        {
            using EventWaitHandle activationEvent = EventWaitHandle.OpenExisting(ActivationEventName);
            activationEvent.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private void StartActivationListener()
    {
        if (_activationEvent is null)
        {
            return;
        }

        _activationListenerCts = new CancellationTokenSource();
        CancellationToken token = _activationListenerCts.Token;
        EventWaitHandle activationEvent = _activationEvent;
        _ = Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!activationEvent.WaitOne(TimeSpan.FromMilliseconds(250)))
                    {
                        continue;
                    }

                    Dispatcher.BeginInvoke(() =>
                    {
                        if (MainWindow is MainWindow window)
                        {
                            window.ActivateFromExternalLaunch();
                        }
                    });
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }, token);
    }
}

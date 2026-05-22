using System.Drawing;
using MokoSnap.App.ViewModels;
using Forms = System.Windows.Forms;

namespace MokoSnap.App.Services;

public sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Func<IReadOnlyList<PresetEditorViewModel>> _getPresets;
    private readonly Action _openMainWindow;
    private readonly Func<Task> _openQuickSwitcherAsync;
    private readonly Func<string, Task> _runPresetAsync;
    private readonly Action _exit;
    private bool _disposed;

    public TrayIconService(
        Func<IReadOnlyList<PresetEditorViewModel>> getPresets,
        Action openMainWindow,
        Func<Task> openQuickSwitcherAsync,
        Func<string, Task> runPresetAsync,
        Action exit)
    {
        _getPresets = getPresets;
        _openMainWindow = openMainWindow;
        _openQuickSwitcherAsync = openQuickSwitcherAsync;
        _runPresetAsync = runPresetAsync;
        _exit = exit;
        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "MokoSnap",
            ContextMenuStrip = new Forms.ContextMenuStrip(),
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => _openMainWindow();
        _notifyIcon.ContextMenuStrip.Opening += (_, _) => RebuildContextMenu();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _disposed = true;
    }

    private void RebuildContextMenu()
    {
        Forms.ContextMenuStrip menu = _notifyIcon.ContextMenuStrip!;
        menu.Items.Clear();

        menu.Items.Add(CreateMenuItem("Open MokoSnap", (_, _) => _openMainWindow()));
        menu.Items.Add(CreateMenuItem("Quick Switcher", async (_, _) => await _openQuickSwitcherAsync()));
        menu.Items.Add(new Forms.ToolStripSeparator());

        Forms.ToolStripMenuItem runPresetMenu = new("Run Preset");
        IReadOnlyList<PresetEditorViewModel> presets = _getPresets();
        if (presets.Count == 0)
        {
            Forms.ToolStripMenuItem emptyItem = new("(No presets)") { Enabled = false };
            runPresetMenu.DropDownItems.Add(emptyItem);
        }
        else
        {
            foreach (PresetEditorViewModel preset in presets.OrderBy(preset => preset.Name))
            {
                string presetId = preset.Id;
                string presetName = string.IsNullOrWhiteSpace(preset.Name) ? "Unnamed preset" : preset.Name;
                runPresetMenu.DropDownItems.Add(CreateMenuItem(
                    presetName,
                    async (_, _) => await _runPresetAsync(presetId)));
            }
        }

        menu.Items.Add(runPresetMenu);
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(CreateMenuItem("Exit", (_, _) => _exit()));
    }

    private static Forms.ToolStripMenuItem CreateMenuItem(string text, EventHandler onClick)
    {
        Forms.ToolStripMenuItem item = new(text);
        item.Click += onClick;
        return item;
    }
}

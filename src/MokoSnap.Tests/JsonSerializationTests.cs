using System.Text.Json;
using MokoSnap.Core.Hotkeys;
using MokoSnap.Core.Models;
using MokoSnap.Core.Storage;

namespace MokoSnap.Tests;

public class JsonSerializationTests
{
    [Fact]
    public void AppSettingsRoundTripPreservesPresetAndTargets()
    {
        AppSettings settings = new()
        {
            Presets =
            [
                new Preset
                {
                    Id = "work",
                    Name = "Work",
                    Description = "Daily work setup",
                    Hotkey = new HotkeyGesture { Ctrl = true, Alt = true, Key = "W" },
                    ClosePolicy = ClosePolicy.CloseVisibleWindowsOnly,
                    CloseConfirmationPolicy = CloseConfirmationPolicy.SkipConfirmation,
                    Targets =
                    [
                        new TargetConfig
                        {
                            Type = TargetType.Application,
                            DisplayName = "Editor",
                            ExecutablePath = @"C:\Tools\editor.exe",
                            Arguments = "--reuse-window",
                            WorkingDirectory = @"C:\Work",
                            LaunchDelayMs = 250,
                            RunAsAdmin = false,
                            WindowPlacement = new WindowPlacementSnapshot
                            {
                                Enabled = true,
                                ShowState = WindowPlacementShowState.Maximized,
                                Left = 10,
                                Top = 20,
                                Width = 1200,
                                Height = 800,
                                MonitorDeviceName = @"\\.\DISPLAY1",
                                WasProbablySnapped = true
                            }
                        },
                        new TargetConfig
                        {
                            Type = TargetType.Chrome,
                            DisplayName = "Docs",
                            ProfileName = "Default",
                            OpenInNewWindow = true,
                            Urls = ["https://example.com/docs"]
                        },
                        new TargetConfig
                        {
                            Type = TargetType.Notion,
                            DisplayName = "Plan",
                            PreferDesktopApp = true,
                            PageUrls = ["https://notion.so/example"]
                        },
                        new TargetConfig
                        {
                            Type = TargetType.Url,
                            DisplayName = "Dashboard",
                            Url = "https://example.com"
                        },
                        new TargetConfig
                        {
                            Type = TargetType.Folder,
                            DisplayName = "Workspace",
                            Path = @"C:\Work"
                        }
                    ]
                }
            ]
        };

        string json = JsonSerializer.Serialize(settings, FileJsonStorage<AppSettings>.CreateJsonSerializerOptions());
        AppSettings? roundTripped = JsonSerializer.Deserialize<AppSettings>(
            json,
            FileJsonStorage<AppSettings>.CreateJsonSerializerOptions());

        Assert.NotNull(roundTripped);
        Preset preset = Assert.Single(roundTripped.Presets);
        Assert.Equal("work", preset.Id);
        Assert.Equal("Work", preset.Name);
        Assert.Equal(ClosePolicy.CloseVisibleWindowsOnly, preset.ClosePolicy);
        Assert.Equal(CloseConfirmationPolicy.SkipConfirmation, preset.CloseConfirmationPolicy);
        Assert.NotNull(preset.Hotkey);
        Assert.True(preset.Hotkey.Ctrl);
        Assert.True(preset.Hotkey.Alt);
        Assert.Equal("W", preset.Hotkey.Key);
        Assert.Equal(5, preset.Targets.Count);
        TargetConfig applicationTarget = Assert.Single(preset.Targets, target => target.Type == TargetType.Application);
        Assert.NotNull(applicationTarget.WindowPlacement);
        Assert.True(applicationTarget.WindowPlacement.Enabled);
        Assert.Equal(WindowPlacementShowState.Maximized, applicationTarget.WindowPlacement.ShowState);
        Assert.Equal(10, applicationTarget.WindowPlacement.Left);
        Assert.Equal(20, applicationTarget.WindowPlacement.Top);
        Assert.Equal(1200, applicationTarget.WindowPlacement.Width);
        Assert.Equal(800, applicationTarget.WindowPlacement.Height);
        Assert.Equal(@"\\.\DISPLAY1", applicationTarget.WindowPlacement.MonitorDeviceName);
        Assert.True(applicationTarget.WindowPlacement.WasProbablySnapped);
        Assert.Contains(preset.Targets, target => target.Type == TargetType.Chrome);
        Assert.Contains(preset.Targets, target => target.Type == TargetType.Notion);
        Assert.Contains(preset.Targets, target => target.Type == TargetType.Url);
        Assert.Contains(preset.Targets, target => target.Type == TargetType.Folder);
    }

    [Fact]
    public void LaunchHistoryRoundTripPreservesEntries()
    {
        DateTimeOffset launchedAt = new(2026, 5, 23, 1, 0, 0, TimeSpan.Zero);
        LaunchHistory history = new()
        {
            Entries =
            [
                new LaunchHistoryEntry
                {
                    PresetId = "work",
                    LaunchedAt = launchedAt,
                    Succeeded = true,
                    Message = "Launched"
                }
            ]
        };

        string json = JsonSerializer.Serialize(history, FileJsonStorage<LaunchHistory>.CreateJsonSerializerOptions());
        LaunchHistory? roundTripped = JsonSerializer.Deserialize<LaunchHistory>(
            json,
            FileJsonStorage<LaunchHistory>.CreateJsonSerializerOptions());

        Assert.NotNull(roundTripped);
        LaunchHistoryEntry entry = Assert.Single(roundTripped.Entries);
        Assert.Equal("work", entry.PresetId);
        Assert.Equal(launchedAt, entry.LaunchedAt);
        Assert.True(entry.Succeeded);
        Assert.Equal("Launched", entry.Message);
    }

    [Fact]
    public void AppSettingsRoundTripPreservesDigitHotkey()
    {
        AppSettings settings = new()
        {
            Presets =
            [
                new Preset
                {
                    Id = "gaming",
                    Name = "Gaming",
                    Hotkey = HotkeyGestureFormatter.Parse("Ctrl+Alt+1")
                }
            ]
        };

        string json = JsonSerializer.Serialize(settings, FileJsonStorage<AppSettings>.CreateJsonSerializerOptions());
        AppSettings? roundTripped = JsonSerializer.Deserialize<AppSettings>(
            json,
            FileJsonStorage<AppSettings>.CreateJsonSerializerOptions());

        Assert.NotNull(roundTripped);
        Preset preset = Assert.Single(roundTripped.Presets);
        Assert.NotNull(preset.Hotkey);
        Assert.True(preset.Hotkey.Ctrl);
        Assert.True(preset.Hotkey.Alt);
        Assert.Equal("1", preset.Hotkey.Key);
        Assert.Equal("Ctrl+Alt+1", HotkeyGestureFormatter.Format(preset.Hotkey));
    }
}

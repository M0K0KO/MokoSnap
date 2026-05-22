using MokoSnap.Core.Hotkeys;
using MokoSnap.Core.Models;

namespace MokoSnap.Tests;

public class HotkeyValidatorTests
{
    [Fact]
    public void EmptyHotkeyIsValid()
    {
        HotkeyValidationResult result = HotkeyValidator.ValidateGesture(null);

        Assert.True(result.IsValid);
        Assert.Empty(result.Messages);
    }

    [Fact]
    public void SingleNonModifierKeyIsInvalid()
    {
        HotkeyValidationResult result = HotkeyValidator.ValidateGesture(new HotkeyGesture { Key = "F9" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Message.Contains("must include", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AltF4IsInvalid()
    {
        HotkeyValidationResult result = HotkeyValidator.ValidateGesture(new HotkeyGesture
        {
            Key = "F4",
            Alt = true
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Message.Contains("Alt+F4", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DuplicatePresetHotkeysAreInvalid()
    {
        Preset first = new()
        {
            Id = "first",
            Name = "First",
            Hotkey = new HotkeyGesture { Key = "W", Ctrl = true, Alt = true }
        };
        Preset second = new()
        {
            Id = "second",
            Name = "Second",
            Hotkey = new HotkeyGesture { Key = "w", Ctrl = true, Alt = true }
        };

        HotkeyValidationResult result = HotkeyValidator.ValidatePresets([first, second]);

        Assert.False(result.IsValid);
        HotkeyValidationMessage duplicate = Assert.Single(result.Errors);
        Assert.Equal("First", duplicate.ConflictingPresetName);
    }

    [Fact]
    public void CommonCtrlShortcutProducesWarningOnly()
    {
        HotkeyValidationResult result = HotkeyValidator.ValidateGesture(new HotkeyGesture
        {
            Key = "C",
            Ctrl = true
        });

        Assert.True(result.IsValid);
        Assert.Single(result.Warnings);
    }

    [Theory]
    [InlineData("Ctrl+Alt+Space", "Ctrl+Alt+Space")]
    [InlineData("control + windows + w", "Ctrl+Win+W")]
    [InlineData("Alt+Esc", "Alt+Escape")]
    public void ParserNormalizesHotkeyText(string input, string expected)
    {
        HotkeyGesture? gesture = HotkeyGestureFormatter.Parse(input);

        Assert.Equal(expected, HotkeyGestureFormatter.Format(gesture));
    }
}

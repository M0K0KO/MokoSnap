namespace MokoSnap.Tests;

public class AppShellSourceTests
{
    [Fact]
    public void CommandPaletteServiceDoesNotShowMainWindowBeforeOpeningQuickSwitcher()
    {
        string appProjectPath = FindAppProjectPath();
        string source = File.ReadAllText(Path.Combine(appProjectPath, "Services", "CommandPaletteService.cs"));

        Assert.DoesNotContain("_owner.Show()", source);
        Assert.DoesNotContain("ActivateAndFocus(_owner", source);
    }

    private static string FindAppProjectPath()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = Path.Combine(directory.FullName, "src", "MokoSnap.App");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find src/MokoSnap.App from the test output directory.");
    }
}

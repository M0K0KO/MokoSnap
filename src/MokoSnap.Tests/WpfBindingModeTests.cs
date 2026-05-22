using System.Xml;
using System.Xml.Linq;

namespace MokoSnap.Tests;

public class WpfBindingModeTests
{
    [Fact]
    public void ReadOnlyTextBoxTextBindingsDeclareOneWayMode()
    {
        string appProjectPath = FindAppProjectPath();
        string[] xamlFiles = Directory.GetFiles(appProjectPath, "*.xaml", SearchOption.AllDirectories);
        List<string> failures = [];

        foreach (string xamlFile in xamlFiles)
        {
            XDocument document = XDocument.Load(xamlFile, LoadOptions.SetLineInfo);
            foreach (XElement textBox in document.Descendants().Where(element => element.Name.LocalName == "TextBox"))
            {
                string? isReadOnly = textBox.Attributes().FirstOrDefault(attribute => attribute.Name.LocalName == "IsReadOnly")?.Value;
                string? textBinding = textBox.Attributes().FirstOrDefault(attribute => attribute.Name.LocalName == "Text")?.Value;
                if (!string.Equals(isReadOnly, "True", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrWhiteSpace(textBinding) ||
                    !textBinding.TrimStart().StartsWith("{Binding", StringComparison.Ordinal))
                {
                    continue;
                }

                if (!textBinding.Contains("Mode=OneWay", StringComparison.Ordinal))
                {
                    IXmlLineInfo lineInfo = (IXmlLineInfo)textBox;
                    string relativePath = Path.GetRelativePath(appProjectPath, xamlFile);
                    failures.Add($"{relativePath}:{lineInfo.LineNumber} read-only TextBox.Text binding must declare Mode=OneWay.");
                }
            }
        }

        Assert.Empty(failures);
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

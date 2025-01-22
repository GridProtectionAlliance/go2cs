// Move up from here: "go2cs\src\Utilities\UpdateTestTargets\bin\Debug\net9.0"
const string RootPath = @"..\..\..\..\..\";

// Scan all behavioral test folders
string[] behavioralTestDirs = Directory.GetDirectories(RootPath + @"Tests\Behavioral");

List<string> targetTests = [];

foreach (string testDir in behavioralTestDirs)
{
    if (testDir.EndsWith("Tests"))
        continue;

    // Get last subdirectory name which is the test project name
    string[] dirParts = testDir.Split(Path.DirectorySeparatorChar);
    targetTests.Add(dirParts[^1]);
}

string[] testClasses =
[
    "TranspileTests",           // Tests transpilation of Go code to C# code
    "CompileTests",             // Tests compilation of transpiled C# code
    "TargetComparisonTests"     // Tests comparison of transpiled C# code to expected target
];

foreach (string testClass in testClasses)
{
    string testFile = Path.GetFullPath($@"{RootPath}Tests\Behavioral\BehavioralTests\{testClass}.cs");
    string[] testFileLines = File.ReadAllLines(testFile);
    int startLineIndex = -1;
    int endLineIndex = -1;

    for (int i = 0; i < testFileLines.Length; i++)
    {
        if (testFileLines[i].Contains("// <TestMethods>"))
        {
            startLineIndex = i + 1;
            continue;
        }
        
        if (testFileLines[i].Contains("// </TestMethods>"))
        {
            endLineIndex = i;
            break;
        }
    }

    if (startLineIndex >= 0 && endLineIndex >= 0 && startLineIndex < endLineIndex)
    {
        // Add all lines up to the start of the test methods
        List<string> lines = [ ..testFileLines[..startLineIndex] ] ;

        // Add new test methods for each target test
        lines.AddRange(targetTests.Select(targetTest => 
            $"\r\n    [TestMethod]\r\n    public void Check{targetTest}() => CheckTarget(\"{targetTest}\");"));

        lines.Add("");

        // Add all lines after the end of the test methods
        lines.AddRange(testFileLines[endLineIndex..]);

        File.WriteAllLines(testFile, lines);
    }
    else
    {
        throw new InvalidOperationException($"Could not find '<TestMethods>...</TestMethods>' section in \"{testFile}\"");
    }
}

if (args.Length > 0 && args[0] == "--createTargetFiles")
{
    // For each Go file converted to C#, create a target file for regression testing comparisons
    foreach (string targetTest in targetTests)
    {
        string projPath = Path.GetFullPath($"{RootPath}Tests\\Behavioral\\{targetTest}");
        string transpiledFile = $@"{projPath}\{targetTest}.cs";
        string targetFile = $"{transpiledFile}.target";

        if (!File.Exists(transpiledFile))
            Console.Error.WriteLine($"WARNING: Transpiled file \"{transpiledFile}\" does not exist -- skipping target file creation...");
        else
            File.Copy(transpiledFile, targetFile, true);
    }
}

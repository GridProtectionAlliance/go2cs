using Microsoft.VisualStudio.TestTools.UnitTesting;
using go2cs.AST;
using System.IO;
using System.Text.Json;

namespace DeserializeTests
{
    [TestClass]
    public class Tests
    {
        const string RootPath = @"..\..\..";

        [TestMethod]
        public void TestASTDeserialization()
        {
            JsonSerializerOptions options = new() { MaxDepth = int.MaxValue };

            foreach (string file in  Directory.GetFiles($@"{RootPath}", "*.json"))
            {
                using FileStream stream = File.OpenRead(file);
                JsonSerializer.Deserialize<FileNode>(stream, options);
            }
        }
    }
}

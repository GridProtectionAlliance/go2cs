//******************************************************************************************************
//  Converter.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/01/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using go2cs.Metadata;
using go2cs.Templates;
using static go2cs.Common;

#pragma warning disable SCS0018 // Path traversal

namespace go2cs
{
    /// <summary>
    /// Represents a converter used to convert Go source code to C#.
    /// </summary>
    public partial class Converter : ScannerBase
    {
        public const string StandardLibrary = "GoStandardLibrary";
        private const string UsingsMarker = ">>MARKER:USINGS<<";
        private const string UnsafeMarker = ">>MARKER:UNSAFE<<";

        private StringBuilder m_targetFile = new StringBuilder();

        public Dictionary<string, (string targetImport, string targetUsing)> ImportAliases { get; }

        public Dictionary<string, FolderMetadata> ImportMetadata { get; }

        public Converter(BufferedTokenStream tokenStream, GoParser parser, Options options, string fileName) : base(tokenStream, parser, options, fileName)
        {
            if (Metadata is null)
                throw new InvalidOperationException($"Failed to load metadata for \"{fileName}\" - file conversion canceled.");

            Package = Metadata.Package;
            PackageImport = Metadata.PackageImport;
            ImportAliases = Metadata.ImportAliases;
            ImportMetadata = new Dictionary<string, FolderMetadata>(StringComparer.Ordinal);
        }

        public override void Scan(bool showParseTree)
        {
            // Base class walks parse tree
            base.Scan(showParseTree);

            if (!WroteLineFeed)
                m_targetFile.AppendLine();

            // Close class and namespaces as begun during Converter_TopLevelDecl visit
            m_targetFile.AppendLine($"{Spacing(indentLevel: 1)}}}");
            m_targetFile.AppendLine(m_namespaceFooter);

            string targetFile = m_targetFile.ToString();

            // Find usings marker
            int index = targetFile.IndexOf(UsingsMarker, StringComparison.Ordinal);

            // Insert required usings
            if (index > -1 && RequiredUsings.Count > 0)
                targetFile = targetFile.Insert(index, $"{Environment.NewLine}{string.Join(Environment.NewLine, RequiredUsings.Select(usingType => $"using {usingType};"))}{Environment.NewLine}");

            // Remove code markers
            targetFile = targetFile.Replace(UsingsMarker, "");
            targetFile = targetFile.Replace(UnsafeMarker, UsesUnsafePointers ? "unsafe " : "");

            using StreamWriter writer = File.CreateText(TargetFileName);
            writer.Write(targetFile);
        }

        protected override void BeforeScan()
        {
            Console.WriteLine($"Converting from{Environment.NewLine}    \"{SourceFileName}\" to{Environment.NewLine}    \"{TargetFileName}\"...");
        }

        protected override void AfterScan()
        {
            if (!PackageImport.Equals("main"))
                Console.WriteLine($"        import \"{PackageImport}\" ==> using {PackageUsing}");

            Console.WriteLine("    Finished.");
        }

        protected override void SkippingScan()
        {
            Console.WriteLine($"Skipping convert for{Environment.NewLine}    \"{SourceFileName}\", target{Environment.NewLine}    \"{TargetFileName}\" already exists.");
        }

        protected override void SkippingImport(string import)
        {
            Console.WriteLine($"Skipping convert for Go standard library import package \"{import}\".");
            Console.WriteLine();
        }

        private static readonly HashSet<string> s_mainPackageFiles;
        private static readonly Dictionary<string, Dictionary<string, (string nameSpace, HashSet<string> fileNames)>> s_packageInfo;

        static Converter()
        {
            s_mainPackageFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            s_packageInfo = new Dictionary<string, Dictionary<string, (string, HashSet<string>)>>(StringComparer.OrdinalIgnoreCase);
        }

        public static void Convert(Options options)
        {
            if (options.OnlyUpdateMetadata)
                return;

            ResetScanner();
            Scan(options, options.ShowParseTree, CreateNewConverter);
            WriteProjectFiles(options);
        }

        private static ScannerBase CreateNewConverter(BufferedTokenStream tokenStream, GoParser parser, Options options, string fileName)
        {
            return new Converter(tokenStream, parser, options, fileName);
        }

        private static void WriteProjectFiles(Options options)
        {
            try
            {
                // Map of package names to list of package path and file names
                Dictionary<string, List<(string path, string[] fileNames)>> groupedPackageData;

                // Process import packages - these become shared projects
                groupedPackageData = CreateGroupedPackageData();

                // Process packages with "main" functions - these become standard projects
                ProcessMainProjectPackages(options);

                if (options.ConvertStandardLibrary && options.RecurseSubdirectories && AddPathSuffix(options.SourcePath).Equals(GoPath))
                    ProcessStandardLibraryPackages(options, groupedPackageData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write project files: {ex.Message}");
            }
        }

        private static Dictionary<string, List<(string, string[])>> CreateGroupedPackageData()
        {
            Dictionary<string, List<(string path, string[] fileNames)>> groupedPackageData = new Dictionary<string, List<(string, string[])>>(StringComparer.Ordinal);

            foreach (KeyValuePair<string, Dictionary<string, (string nameSpace, HashSet<string> fileNames)>> kvp in s_packageInfo)
            {
                string packagePath = kvp.Key;

                // Depending on the scope of the conversion, the same package name may exist in multiple paths
                foreach (KeyValuePair<string, (string nameSpace, HashSet<string> fileNames)> fileGroup in kvp.Value)
                {
                    string package = fileGroup.Key;
                    string[] packageFileNames = fileGroup.Value.fileNames.ToArray();

                    List<(string, string[])> groupPackageData = groupedPackageData.GetOrAdd(package, _ => new List<(string, string[])>());
                    groupPackageData.Add((packagePath, packageFileNames));
                }
            }

            return groupedPackageData;
        }

        private static void ProcessMainProjectPackages(Options options)
        {
            foreach (string mainPackageFile in s_mainPackageFiles)
            {
                string mainPackageFileName = Path.GetFileName(mainPackageFile) ?? "";
                string mainPackagePath = Path.GetDirectoryName(mainPackageFile) ?? "";
                string assemblyName = Path.GetFileNameWithoutExtension(mainPackageFileName);

                FolderMetadata folderMetadata = GetFolderMetadata(options, mainPackageFile);
                string sourceFileName = Path.Combine(Path.GetDirectoryName(mainPackageFile) ?? "", $"{Path.GetFileNameWithoutExtension(mainPackageFile)}.go");

                if (folderMetadata is null || !folderMetadata.Files.TryGetValue(sourceFileName, out FileMetadata metadata))
                    throw new InvalidOperationException($"Failed to load metadata for \"{sourceFileName}\" - file conversion canceled.");

                string mainProjectFile = Path.Combine(mainPackagePath, $"{assemblyName}.csproj");
                string mainProjectFileContent = new MainProjectTemplate
                {
                    AssemblyName = assemblyName,
                    Imports = metadata.ImportAliases.Select(kvp => kvp.Value.targetImport)
                }.TransformText();

                // Build main project file
                if (File.Exists(mainProjectFile) && GetMD5HashFromFile(mainProjectFile) == GetMD5HashFromString(mainProjectFileContent))
                    continue;

                using StreamWriter writer = File.CreateText(mainProjectFile);
                writer.Write(mainProjectFileContent);
            }
        }

        private static void ProcessStandardLibraryPackages(Options options, Dictionary<string, List<(string path, string[] fileNames)>> groupedPackageData)
        {
            foreach (KeyValuePair<string, List<(string path, string[] fileNames)>> packageData in groupedPackageData)
            {
                foreach ((string path, string[] fileNames) rootPackage in packageData.Value.Where(info => info.path.StartsWith(GoRoot)))
                {
                    foreach (string fileName in rootPackage.fileNames)
                    {
                        if (fileName.EndsWith("_test.go"))
                            continue;

                        string assemblyName = packageData.Key;
                        string libraryProjectFile = Path.Combine(rootPackage.path, $"{assemblyName}.csproj");

                        FolderMetadata folderMetadata = GetFolderMetadata(options, fileName);
                        string sourceFileName = Path.Combine(Path.GetDirectoryName(fileName) ?? "", $"{Path.GetFileNameWithoutExtension(fileName)}.go");

                        if (folderMetadata is null || !folderMetadata.Files.TryGetValue(sourceFileName, out FileMetadata metadata))
                            throw new InvalidOperationException($"Failed to load metadata for \"{sourceFileName}\" - file conversion canceled.");

                        string libraryProjectFileContent = new LibraryProjectTemplate
                        {
                            AssemblyName = assemblyName,
                            Imports = metadata.ImportAliases.Select(kvp => kvp.Value.targetImport)
                        }.TransformText();

                        // Build library project file
                        if (File.Exists(libraryProjectFile) && GetMD5HashFromFile(libraryProjectFile) == GetMD5HashFromString(libraryProjectFileContent))
                            continue;

                        using StreamWriter writer = File.CreateText(libraryProjectFile);
                        writer.Write(libraryProjectFileContent);
                    }
                }
            }
        }

        private string GetPackageNamespace(string packageImport)
        {
            string[] paths = packageImport.Split('/').Select(SanitizedIdentifier).ToArray();
            return $"{RootNamespace}.{string.Join(".", paths)}{ClassSuffix}";
        }

        private static void AddFileToPackage(string package, string fileName, string nameSpace)
        {
            // Since the same package name may exist at multiple paths, we track details by path
            Dictionary<string, (string, HashSet<string>)> packageInfo = s_packageInfo.GetOrAdd(Path.GetDirectoryName(fileName), _ => new Dictionary<string, (string, HashSet<string>)>(StringComparer.Ordinal));
            (string, HashSet<string> fileNames) fileGroup = packageInfo.GetOrAdd(package, _ => (nameSpace, new HashSet<string>(StringComparer.OrdinalIgnoreCase)));
            fileGroup.fileNames.Add(fileName);
        }
    }
}

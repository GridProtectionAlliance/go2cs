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
using go2cs.Metadata;
//using go2cs.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public FileMetadata Metadata { get; }

        public Dictionary<string, (string targetImport, string targetUsing)> ImportAliases { get; }

        public Dictionary<string, FolderMetadata> ImportMetadata { get; }

        public Converter(BufferedTokenStream tokenStream, GoParser parser, Options options, string fileName) : base(tokenStream, parser, options, fileName)
        {
            FolderMetadata folderMetadata = GetFolderMetadata(Options, SourceFileName);

            if (folderMetadata is null || !folderMetadata.Files.TryGetValue(fileName, out FileMetadata metadata))
                throw new InvalidOperationException($"Failed to load metadata for \"{fileName}\" - file conversion canceled.");

            Metadata = metadata;
            Package = metadata.Package;
            PackageImport = metadata.PackageImport;
            ImportAliases = metadata.ImportAliases;
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
            // Map of package names to list of package path and file names
            Dictionary<string, List<(string path, string[] fileNames)>> groupedPackageData;

            // Process import packages - these become shared projects
            groupedPackageData = ProcessSharedProjectPackages();

            // Process packages with "main" functions - these become standard projects
            ProcessMainProjectPackages(groupedPackageData);

            // If converting the full Go standard library, create shared and standard projects for the complete library
            if (options.ConvertStandardLibrary && options.RecurseSubdirectories && AddPathSuffix(options.SourcePath).Equals(GoPath))
                ProcessStandardLibraryPackages(options, groupedPackageData);
        }

        private static Dictionary<string, List<(string, string[])>> ProcessSharedProjectPackages()
        {
            Dictionary<string, List<(string path, string[] fileNames)>> groupedPackageData = new Dictionary<string, List<(string, string[])>>(StringComparer.Ordinal);

            foreach (KeyValuePair<string, Dictionary<string, (string nameSpace, HashSet<string> fileNames)>> kvp in s_packageInfo)
            {
                string packagePath = kvp.Key;

                // Depending on the scope of the conversion, the same package name may exist in multiple paths
                foreach (KeyValuePair<string, (string nameSpace, HashSet<string> fileNames)> fileGroup in kvp.Value)
                {
                    string package = fileGroup.Key;
                    string packageNamespace = fileGroup.Value.nameSpace ?? RootNamespace;
                    string[] packageFileNames = fileGroup.Value.fileNames.ToArray();

                    List<(string, string[])> groupPackageData = groupedPackageData.GetOrAdd(package, _ => new List<(string, string[])>());
                    groupPackageData.Add((packagePath, packageFileNames));

                    if (package.Equals("main"))
                        continue;

                    //string sharedProjectItems = Path.Combine(packagePath, $"{package}.projitems");
                    //string sharedProjectFile = Path.Combine(packagePath, $"{package}.shproj");
                    //string uniqueProjectID = GetProjectGuid(sharedProjectItems, "SharedGUID");

                    //string sharedProjectItemsContent = new SharedProjectFileItemsTemplate
                    //{
                    //    UniqueProjectID = uniqueProjectID,
                    //    RootNamespace = packageNamespace,
                    //    FileNames = packageFileNames.Select(Path.GetFileName).ToArray()
                    //}
                    //.TransformText();

                    //string sharedProjectFileContent = new SharedProjectFileTemplate
                    //{
                    //    UniqueProjectID = uniqueProjectID,
                    //    PackageName = package
                    //}
                    //.TransformText();

                    //// Build a shared project items file (this is the shared project that normal projects will reference)
                    //if (!File.Exists(sharedProjectItems) || GetMD5HashFromFile(sharedProjectItems) != GetMD5HashFromString(sharedProjectItemsContent))
                    //    using (StreamWriter writer = File.CreateText(sharedProjectItems))
                    //        writer.Write(sharedProjectItemsContent);

                    //// Build a shared project file - this can be added to a solution to easily access reference code 
                    //if (!File.Exists(sharedProjectFile) || GetMD5HashFromFile(sharedProjectFile) != GetMD5HashFromString(sharedProjectFileContent))
                    //    using (StreamWriter writer = File.CreateText(sharedProjectFile))
                    //        writer.Write(sharedProjectFileContent);
                }
            }

            return groupedPackageData;
        }

        private static void ProcessMainProjectPackages(Dictionary<string, List<(string, string[])>> groupedPackageData)
        {
            foreach (string mainPackageFile in s_mainPackageFiles)
            {
                string mainPackageFileName = Path.GetFileName(mainPackageFile) ?? "";
                string mainPackagePath = Path.GetDirectoryName(mainPackageFile) ?? "";
                string assemblyName = Path.GetFileNameWithoutExtension(mainPackageFileName);
                string[] projectFiles = null;

                List<(string, string[])> groupPackageData = groupedPackageData["main"];

                foreach ((string path, string[] fileNames) packageData in groupPackageData)
                {
                    if (packageData.path.Equals(mainPackagePath, StringComparison.OrdinalIgnoreCase))
                    {
                        projectFiles = packageData.fileNames;
                        break;
                    }
                }

                if (projectFiles is null)
                    throw new InvalidOperationException($"Failed to find project files for main package file: {mainPackageFile}");

                // When multiple Go source files from the same folder contain a main method,
                // make sure to only include the target project file
                List<string> checkedProjectFiles = new List<string>();
                bool multipleSinglePathMainTargets = false;

                foreach (string projectFile in projectFiles)
                {
                    bool isMainPackage = s_mainPackageFiles.Contains(projectFile);

                    // TODO: Make sure to exclude ancillary files not associated with current project - need ID method (check <auto-generated> in header and match file prefix?)
                    if (projectFile.Equals(mainPackageFile))
                        checkedProjectFiles.Add(projectFile);
                    else if (isMainPackage)
                        multipleSinglePathMainTargets = true;
                    else if (!projectFile.EndsWith("_test.go"))
                        checkedProjectFiles.Add(projectFile);
                }

                string mainProjectFile = Path.Combine(mainPackagePath, $"{assemblyName}.csproj");
                //string uniqueProjectID = RemoveSurrounding(GetProjectGuid(mainProjectFile, "ProjectGuid"), "{", "}");
                string mainProjectAssemblyInfoFile;

                if (multipleSinglePathMainTargets)
                {
                    // When multiple main projects exist in the same folder, just manually add a unique assembly name
                    mainProjectAssemblyInfoFile = Path.Combine(mainPackagePath, $"{assemblyName}_AssemblyInfo.cs");
                }
                else
                {
                    // Otherwise AssemblyInfo can appear with its default name and location
                    string mainProjectAssemblyInfoFilePath = Path.Combine(mainPackagePath, "Properties");

                    if (!Directory.Exists(mainProjectAssemblyInfoFilePath))
                        Directory.CreateDirectory(mainProjectAssemblyInfoFilePath);

                    mainProjectAssemblyInfoFile = Path.Combine(mainProjectAssemblyInfoFilePath, "AssemblyInfo.cs");
                }

                //string mainProjectAssemblyInfoFileContent = new MainProjectAssemblyInfoTemplate
                //{
                //    AssemblyName = assemblyName,
                //    UniqueProjectID = uniqueProjectID
                //}
                //.TransformText();

                // Build a main project assembly info file (don't overwrite possible user changes)
                //if (!File.Exists(mainProjectAssemblyInfoFile))
                //    using (StreamWriter writer = File.CreateText(mainProjectAssemblyInfoFile))
                //        writer.Write(mainProjectAssemblyInfoFileContent);

                checkedProjectFiles.Add(mainProjectAssemblyInfoFile);

                //string mainProjectFileContent = new MainProjectTemplate
                //{
                //    AssemblyName = assemblyName,
                //    UniqueProjectID = uniqueProjectID,
                //    ProjectFiles = checkedProjectFiles.Select(fileName => GetRelativePath(fileName, mainPackagePath)).ToArray(),
                //    SharedProjectReferences = new[] { "$(GOPATH)\\src\\go2cs\\goutil\\goutil.projitems" }
                //}
                //.TransformText();

                // Build a main project file
                //if (!File.Exists(mainProjectFile) || GetMD5HashFromFile(mainProjectFile) != GetMD5HashFromString(mainProjectFileContent))
                //    using (StreamWriter writer = File.CreateText(mainProjectFile))
                //        writer.Write(mainProjectFileContent);
            }
        }

        private static void ProcessStandardLibraryPackages(Options options, Dictionary<string, List<(string path, string[] fileNames)>> groupedPackageData)
        {
            List<string> packageFileNames = new List<string>();

            foreach (KeyValuePair<string, List<(string path, string[] fileNames)>> packageData in groupedPackageData)
            {
                foreach ((string path, string[] fileNames) rootPackage in packageData.Value.Where(info => info.path.StartsWith(GoRoot)))
                {
                    packageFileNames.AddRange(rootPackage.fileNames.Where(fileName => !fileName.EndsWith("_test.go")));
                }
            }

            //string sharedProjectItems = Path.Combine(options.TargetGoSrcPath, $"{StandardLibrary}.projitems");
            //string sharedProjectFile = Path.Combine(options.TargetGoSrcPath, $"{StandardLibrary}.shproj");
            //string uniqueProjectID = GetProjectGuid(sharedProjectItems, "SharedGUID");
            //int rootIndex = GoPath.Length;

            //string sharedProjectItemsContent = new SharedProjectFileItemsTemplate
            //{
            //    UniqueProjectID = uniqueProjectID,
            //    RootNamespace = RootNamespace,
            //    FileNames = packageFileNames.Select(fileName => fileName.Substring(rootIndex)).ToArray()
            //}
            //.TransformText();

            //string sharedProjectFileContent = new SharedProjectFileTemplate
            //{
            //    UniqueProjectID = uniqueProjectID,
            //    PackageName = StandardLibrary
            //}
            //.TransformText();

            //// Build a shared project items file (this is the shared project that normal projects will reference)
            //if (!File.Exists(sharedProjectItems) || GetMD5HashFromFile(sharedProjectItems) != GetMD5HashFromString(sharedProjectItemsContent))
            //    using (StreamWriter writer = File.CreateText(sharedProjectItems))
            //        writer.Write(sharedProjectItemsContent);

            //// Build a shared project file - this can be added to a solution to easily access reference code 
            //if (!File.Exists(sharedProjectFile) || GetMD5HashFromFile(sharedProjectFile) != GetMD5HashFromString(sharedProjectFileContent))
            //    using (StreamWriter writer = File.CreateText(sharedProjectFile))
            //        writer.Write(sharedProjectFileContent);
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

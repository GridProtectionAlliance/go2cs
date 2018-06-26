//******************************************************************************************************
//  ScannerBase.cs - Gbtc
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
// ReSharper disable UnusedMember.Global

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using go2cs.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using static go2cs.Common;

#pragma warning disable SCS0018 // Path traversal
#pragma warning disable SCS0028 // Deserialization

namespace go2cs
{
    public delegate ScannerBase CreateNewScannerFunction(BufferedTokenStream tokenStream, GolangParser parser, Options options, string fileName);
    public delegate bool FileNeedsScanFunction(Options options, string fileName, out string message);
    public delegate void SkippedFileScanFunction(Options options, string fileName, bool showParseTree);

    /// <summary>
    /// Represents common Antlr scanning functionality.
    /// </summary>
    /// <remarks>
    /// A full source code pre-scan is needed in order to be aware of promotions in structures and
    /// interfaces. This base class represents the common scanning code that is used by both the
    /// <see cref="PreScanner"/> and <see cref="Converter"/> classes.
    /// </remarks>
    public abstract partial class ScannerBase : GolangBaseListener
    {
        public const string RootNamespace = "go";
        public const string ClassSuffix = "_package";

        private readonly List<string> m_warnings = new List<string>();

        protected readonly HashSet<string> RequiredUsings = new HashSet<string>(StringComparer.Ordinal);

        protected sealed class ParserErrorListener : IAntlrErrorListener<IToken>
        {
            private readonly ScannerBase m_scanner;

            public ParserErrorListener(ScannerBase scanner) => m_scanner = scanner;

            public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                m_scanner.m_warnings.Add($"{Path.GetFileName(m_scanner.SourceFileName)}:{line}:{charPositionInLine}: {msg}");
            }
        }

        public Options Options { get; }

        public BufferedTokenStream TokenStream { get; }

        public GolangParser Parser { get; }

        public string SourceFileName { get; }

        public string SourceFilePath { get; }

        public string TargetFileName { get; }

        public string TargetFilePath { get; }

        public string[] Warnings => m_warnings.ToArray();

        protected string CurrentImportPath { get; private set; }

        protected ScannerBase(BufferedTokenStream tokenStream, GolangParser parser, Options options, string fileName)
        {
            Options = options;

            if ((object)fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            if (!File.Exists(fileName))
                throw new FileNotFoundException($"WARNING: Source file \"{fileName}\" cannot be found.", fileName);

            TokenStream = tokenStream;
            Parser = parser;

            GetFilePaths(options, fileName, out string sourceFileName, out string sourceFilePath, out string targetFileName, out string targetFilePath);

            SourceFileName = sourceFileName;
            SourceFilePath = sourceFilePath;
            TargetFileName = targetFileName;
            TargetFilePath = targetFilePath;
        }

        public virtual void Scan(bool showParseTree)
        {
            IParseTree sourceTree = Parser.sourceFile();

            if (showParseTree)
                Console.WriteLine(sourceTree.ToStringTree(Parser));

            // Walk parsed source tree to start visiting nodes
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(this, sourceTree);
        }

        protected void AddWarning(ParserRuleContext context, string message)
        {
            m_warnings.Add($"{Path.GetFileName(SourceFileName)}:{context.Start.Line}:{context.Start.Column}: {message}");
        }

        protected virtual void BeforeScan()
        {
        }

        protected virtual void AfterScan()
        {
        }

        protected virtual void SkippingScan()
        {
        }

        protected virtual void SkippingImport(string import)
        {
        }

        protected static readonly bool IsPosix;
        protected static readonly string GoRoot;
        protected static readonly string GoPath;
        protected static readonly string[] NewLineDelimeters;
        protected static readonly HashSet<string> ImportQueue;

        private static readonly HashSet<string> s_processedFiles;
        private static readonly HashSet<string> s_processedImports;
        private static string s_currentFolderMetadataFileName;
        private static FolderMetadata s_currentFolderMetadata;

        static ScannerBase()
        {
            IsPosix = Path.DirectorySeparatorChar == '/';

            GoRoot = Environment.GetEnvironmentVariable("GOROOT");

            if (string.IsNullOrWhiteSpace(GoRoot))
                GoRoot = Path.GetFullPath($"{Path.DirectorySeparatorChar}Go");

            GoRoot = AddPathSuffix($"{AddPathSuffix(GoRoot)}src");

            if (!Directory.Exists(GoRoot))
                throw new InvalidOperationException($"Unable to resolve GOROOT src directory: \"{GoRoot}\". Validate that Go is properly installed.");

            GoPath = Environment.GetEnvironmentVariable("GOPATH");

            if (string.IsNullOrWhiteSpace(GoPath))
                GoPath = Environment.ExpandEnvironmentVariables(IsPosix ? "$HOME/go" : "%USERPROFILE%\\go");

            GoPath = AddPathSuffix($"{AddPathSuffix(GoPath)}src");

            if (!Directory.Exists(GoPath))
                Directory.CreateDirectory(GoPath);

            NewLineDelimeters = new[] { "\r\n", "\r", "\n" };

            s_processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            s_processedImports = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ImportQueue = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public static int TotalProcessedFiles => s_processedFiles.Count - TotalSkippedFiles;

        public static int TotalSkippedFiles { get; private set; }

        public static int TotalSkippedPackages { get; private set; }

        public static int TotalWarnings { get; private set; }

        protected static void ResetScanner()
        {
            s_processedFiles.Clear();
            s_processedImports.Clear();
            ImportQueue.Clear();
            TotalSkippedFiles = 0;
            TotalSkippedPackages = 0;
            TotalWarnings = 0;
        }

        protected static void Scan(Options options, bool showParseTree, CreateNewScannerFunction createNewScanner, FileNeedsScanFunction fileNeedsScan = null, SkippedFileScanFunction handleSkippedScan = null)
        {
            if ((object)fileNeedsScan == null)
                fileNeedsScan = DefaultFileNeedsScan;

            string sourcePath = GetAbsolutePath(options.SourcePath);

            if (File.Exists(sourcePath))
            {
                string filePath = Path.GetDirectoryName(sourcePath) ?? "";

                if (filePath.StartsWith(GoRoot, StringComparison.OrdinalIgnoreCase))
                    ScanFile(Options.Clone(options, options.OverwriteExistingFiles, sourcePath, Path.Combine(options.TargetGoSrcPath, filePath.Substring(GoRoot.Length))), sourcePath, showParseTree, createNewScanner, fileNeedsScan, handleSkippedScan);
                else
                    ScanFile(options, sourcePath, showParseTree, createNewScanner, fileNeedsScan, handleSkippedScan);
            }
            else if (Directory.Exists(sourcePath))
            {
                Regex excludeExpression = options.GetExcludeExpression();
                bool scanFileMatch(string fileName) => !excludeExpression.IsMatch(fileName);

                foreach (string fileName in Directory.EnumerateFiles(sourcePath, "*.go", SearchOption.TopDirectoryOnly))
                {
                    if (scanFileMatch(fileName))
                        ScanFile(options, fileName, showParseTree, createNewScanner, fileNeedsScan, handleSkippedScan);
                }

                if (options.RecurseSubdirectories)
                {
                    foreach (string subDirectory in Directory.EnumerateDirectories(sourcePath))
                    {
                        string targetDirectory = options.TargetPath;

                        if (!string.IsNullOrEmpty(targetDirectory))
                            targetDirectory = Path.Combine(targetDirectory, subDirectory.Substring(sourcePath.Length));

                        Scan(Options.Clone(options, options.OverwriteExistingPackages, subDirectory, targetDirectory), showParseTree, createNewScanner, fileNeedsScan, handleSkippedScan);
                    }
                }
            }
            else
            {
                throw new DirectoryNotFoundException($"WARNING: Source path \"{sourcePath}\" cannot be found.");
            }
        }

        protected static void ScanFile(Options options, string fileName, bool showParseTree, CreateNewScannerFunction createNewScanner, FileNeedsScanFunction fileNeedsScan = null, SkippedFileScanFunction handleSkippedScan = null)
        {
            if ((object)fileNeedsScan == null)
                fileNeedsScan = DefaultFileNeedsScan;

            if (s_processedFiles.Contains(fileName))
                return;

            s_processedFiles.Add(fileName);

            if (!fileNeedsScan(options, fileName, out string message))
            {
                Console.WriteLine(message);
                handleSkippedScan?.Invoke(options, fileName, showParseTree);                
                return;
            }

            AntlrInputStream inputStream;

            using (StreamReader reader = File.OpenText(fileName))
                inputStream = new AntlrInputStream(reader);

            GolangLexer lexer = new GolangLexer(inputStream);
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            GolangParser parser = new GolangParser(tokenStream);
            ScannerBase scanner = createNewScanner(tokenStream, parser, options, fileName);

            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ParserErrorListener(scanner));

            if (options.OverwriteExistingFiles || !File.Exists(scanner.TargetFileName))
            {
                scanner.BeforeScan();
                scanner.Scan(showParseTree);
                scanner.AfterScan();

            }
            else
            {
                scanner.SkippingScan();
                TotalSkippedFiles++;
            }

            string[] warnings = scanner.Warnings;

            if (warnings.Length > 0)
            {
                Console.WriteLine();
                Console.WriteLine("WARNINGS:");

                foreach (string warning in warnings)
                    Console.WriteLine($"    {warning}");

                TotalWarnings += warnings.Length;
            }

            Console.WriteLine();

            if (!options.LocalConvertOnly)
                ScanImports(scanner, showParseTree, createNewScanner, fileNeedsScan, handleSkippedScan);
        }

        protected static void ScanImports(ScannerBase scanner, bool showParseTree, CreateNewScannerFunction createNewScanner, FileNeedsScanFunction fileNeedsScan = null, SkippedFileScanFunction handleSkippedScan = null)
        {
            string[] imports = ImportQueue.ToArray();
            Options options = scanner.Options;

            foreach (string import in imports)
            {
                if (s_processedImports.Contains(import))
                    continue;

                s_processedImports.Add(import);

                string importPath = AddPathSuffix(import.Replace("/", "\\"));
                string goRootImport = Path.Combine(GoRoot, importPath);
                string goPathImport = Path.Combine(GoPath, importPath);
                string targetPath = null;

                if (Directory.Exists(goRootImport))
                {
                    targetPath = Path.Combine(options.TargetGoSrcPath, importPath);

                    if (options.ConvertStandardLibrary)
                    {
                        Scan(Options.Clone(options, options.OverwriteExistingPackages, goRootImport, targetPath), showParseTree, createNewScanner, fileNeedsScan, handleSkippedScan);
                    }
                    else
                    {
                        // Only count package conversion as skipped when there are no existing converted files
                        if (PathHasFiles(targetPath, "*.cs"))
                            continue;

                        scanner.SkippingImport(import);
                        TotalSkippedPackages++;
                    }
                }
                else if (Directory.Exists(goPathImport))
                {
                    if (!string.IsNullOrEmpty(options.TargetPath))
                        targetPath = Path.Combine(options.TargetPath, importPath);

                    Scan(Options.Clone(options, options.OverwriteExistingPackages, goPathImport, targetPath), showParseTree, createNewScanner, fileNeedsScan, handleSkippedScan);
                }
                else
                {
                    Console.WriteLine($"WARNING: Failed to locate package \"{import}\" at either:");
                    Console.WriteLine($"    {goRootImport} (from %GOROOT%)");
                    Console.WriteLine($"    {goPathImport} (from %GOPATH%)");
                    Console.WriteLine();
                }
            }
        }

        protected static void GetFilePaths(Options options, string fileName, out string sourceFileName, out string sourceFilePath, out string targetFileName, out string targetFilePath)
        {
            sourceFileName = Path.GetFullPath(fileName);
            sourceFilePath = Path.GetDirectoryName(sourceFileName) ?? "";
            targetFileName = $"{Path.GetFileNameWithoutExtension(sourceFileName)}.cs";
            targetFilePath = string.IsNullOrWhiteSpace(options.TargetPath) || sourceFilePath.StartsWith(options.TargetGoSrcPath) ? sourceFilePath : Path.GetFullPath(options.TargetPath);

            if (!Directory.Exists(targetFilePath))
                Directory.CreateDirectory(targetFilePath);

            targetFileName = Path.Combine(targetFilePath, targetFileName);
        }

        protected static string GetFolderMetadataFileName(Options options, string fileName)
        {
            GetFilePaths(options, fileName, out _, out _, out _, out string targetFilePath);
            targetFilePath = AddPathSuffix(targetFilePath);
            string lastDirName = GetLastDirectoryName(targetFilePath);
            return $"{targetFilePath}{lastDirName}.metadata";
        }

        protected static FolderMetadata GetFolderMetadata(Options options, string fileName)
        {
            string folderMetadataFileName = GetFolderMetadataFileName(options, fileName);

            if (!File.Exists(folderMetadataFileName))
                return null;

            if (folderMetadataFileName.Equals(s_currentFolderMetadataFileName, StringComparison.OrdinalIgnoreCase) && (object)s_currentFolderMetadata != null)
                return s_currentFolderMetadata;

            FolderMetadata folderMetadata;
            BinaryFormatter formatter = new BinaryFormatter();

            try
            {
                using (FileStream stream = File.OpenRead(folderMetadataFileName))
                    folderMetadata = formatter.Deserialize(stream) as FolderMetadata;
            }
            catch (Exception ex)
            {
                folderMetadata = null;

                if (!folderMetadataFileName.Equals(s_currentFolderMetadataFileName, StringComparison.OrdinalIgnoreCase))
                    Console.WriteLine($"Failed to load folder metadata from \"{folderMetadataFileName}\": {ex.Message}");
            }

            s_currentFolderMetadataFileName = folderMetadataFileName;
            s_currentFolderMetadata = folderMetadata;

            return folderMetadata;
        }

        protected static FolderMetadata LoadImportMetadata(Options options, string targetImport, out string warning)
        {
            int lastSlash = targetImport.LastIndexOf('/');
            string packageName = lastSlash > -1 ? targetImport.Substring(lastSlash + 1) : targetImport;
            string importPath = $"{AddPathSuffix(targetImport.Replace("/", "\\"))}{packageName}.go";
            string goRootImport = Path.Combine(options.TargetGoSrcPath, importPath);
            string goPathImport = Path.Combine(GoPath, importPath);
            FolderMetadata metadata;
            warning = default;

            metadata = GetFolderMetadata(options, goRootImport);

            if ((object)metadata != null)
                return metadata;

            metadata = GetFolderMetadata(options, goPathImport);

            if ((object)metadata != null)
                return metadata;

            StringBuilder loadWarning = new StringBuilder();

            loadWarning.AppendLine($"WARNING: Failed to locate package metadata for \"{targetImport}\" import at either:");
            loadWarning.AppendLine($"    {GetFolderMetadataFileName(options, goRootImport)} (from -g Go source target path)");
            loadWarning.AppendLine($"    {GetFolderMetadataFileName(options, goPathImport)} (from %GOPATH%)");

            warning = loadWarning.ToString();
            return null;
        }

        protected static string GetValidIdentifierName(string identifier)
        {
            int lastDotIndex = identifier.LastIndexOf('.');

            if (lastDotIndex > 0)
                identifier = identifier.Substring(lastDotIndex + 1);

            return SanitizedIdentifier(identifier);
        }

        protected string GetUniqueIdentifier<T>(IDictionary<string, T> source, string identifier)
        {
            int count = 0;
            string uniqueIdentifier = identifier;

            while (source.ContainsKey(uniqueIdentifier))
                uniqueIdentifier = $"{identifier}@@{++count}";

            return uniqueIdentifier;
        }

        protected TypeInfo ConvertByRefToBasicPointer(TypeInfo typeInfo)
        {
            if (!typeInfo.IsByRefPointer)
                return typeInfo;

            string primitiveName = typeInfo.PrimitiveName;
            string frameworkName = typeInfo.FrameworkName;

            string[] parts = primitiveName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
                primitiveName = $"Ptr<{parts[1]}>";

            parts = frameworkName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
                frameworkName = $"Ptr<{parts[1]}>";

            return new TypeInfo
            {
                Name = typeInfo.Name,
                PrimitiveName = primitiveName,
                FrameworkName = frameworkName,
                IsPointer = true,
                IsByRefPointer = false,
                TypeClass = TypeClass.Simple
            };
        }

        protected static string GetAbsolutePath(string filePath)
        {
            if (!Path.IsPathRooted(filePath))
                filePath = Path.Combine(GoPath, filePath);

            return filePath;
        }

        private static bool DefaultFileNeedsScan(Options options, string fileName, out string message)
        {
            message = null;
            return true;
        }
    }
}
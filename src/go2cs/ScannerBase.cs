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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using static go2cs.Common;

using CreateNewScannerFunction = System.Func<Antlr4.Runtime.BufferedTokenStream, GolangParser, go2cs.Options, string, go2cs.ScannerBase>;
using FileNeedsScanFunction = System.Func<string, bool>;

#pragma warning disable SCS0018 // Path traversal

namespace go2cs
{
    public abstract class ScannerBase : GolangBaseListener
    {
        private readonly List<string> m_warnings = new List<string>();

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

        public string PackagePath { get; private set; }

        public string[] Warnings => m_warnings.ToArray();

        protected ScannerBase(BufferedTokenStream tokenStream, GolangParser parser, Options options, string fileName)
        {
            Options = options;

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (!File.Exists(fileName))
                throw new FileNotFoundException($"WARNING: Source file \"{fileName}\" cannot be found.", fileName);

            TokenStream = tokenStream;
            Parser = parser;
            SourceFileName = Path.GetFullPath(fileName);
            SourceFilePath = Path.GetDirectoryName(SourceFileName) ?? "";
            TargetFileName = $"{Path.GetFileNameWithoutExtension(SourceFileName)}.cs";
            TargetFilePath = string.IsNullOrWhiteSpace(options.TargetPath) ? SourceFilePath : Path.GetFullPath(options.TargetPath);

            if (!Directory.Exists(TargetFilePath))
                Directory.CreateDirectory(TargetFilePath);

            TargetFileName = Path.Combine(TargetFilePath, TargetFileName);
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

        public override void EnterImportSpec(GolangParser.ImportSpecContext context)
        {
            // Remove quotes from package name
            PackagePath = RemoveSurrounding(ToStringLiteral(context.importPath().STRING_LIT().GetText()));

            // Add package to import queue
            s_importQueue.Add(PackagePath);
        }

        protected static readonly bool IsPosix;
        protected static readonly string GoRoot;
        protected static readonly string GoPath;
        protected static readonly string[] NewLineDelimeters;

        private static readonly HashSet<string> s_processedFiles;
        private static readonly HashSet<string> s_processedImports;
        private static readonly HashSet<string> s_importQueue;

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
            s_importQueue = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public static int TotalProcessedFiles => s_processedFiles.Count;

        public static int TotalSkippedFiles { get; private set; }

        public static int TotalSkippedPackages { get; private set; }

        public static int TotalWarnings { get; private set; }

        protected static void ResetScanner()
        {
            s_processedFiles.Clear();
            s_processedImports.Clear();
            s_importQueue.Clear();
            TotalSkippedFiles = 0;
            TotalSkippedPackages = 0;
            TotalWarnings = 0;
        }

        protected static void Scan(Options options, bool showParseTree, CreateNewScannerFunction createNewScanner, FileNeedsScanFunction fileNeedsScan = null)
        {
            if ((object)fileNeedsScan == null)
                fileNeedsScan = _ => true;

            string sourcePath = GetAbsolutePath(options.SourcePath);

            if (File.Exists(sourcePath))
            {
                string filePath = Path.GetDirectoryName(sourcePath) ?? "";

                if (filePath.StartsWith(GoRoot, StringComparison.OrdinalIgnoreCase))
                    ScanFile(Options.Clone(options, options.OverwriteExistingFiles, sourcePath, Path.Combine(options.TargetGoSrcPath, filePath.Substring(GoRoot.Length))), sourcePath, showParseTree, createNewScanner, fileNeedsScan);
                else
                    ScanFile(options, sourcePath, showParseTree, createNewScanner, fileNeedsScan);
            }
            else if (Directory.Exists(sourcePath))
            {
                Regex excludeExpression = options.GetExcludeExpression();
                bool scanFileMatch(string fileName) => !excludeExpression.IsMatch(fileName);

                foreach (string fileName in Directory.EnumerateFiles(sourcePath, "*.go", SearchOption.TopDirectoryOnly))
                {
                    if (scanFileMatch(fileName))
                        ScanFile(options, fileName, showParseTree, createNewScanner, fileNeedsScan);
                }

                if (options.RecurseSubdirectories)
                {
                    foreach (string subDirectory in Directory.EnumerateDirectories(sourcePath))
                    {
                        string targetDirectory = options.TargetPath;

                        if (!string.IsNullOrEmpty(targetDirectory))
                            targetDirectory = Path.Combine(targetDirectory, subDirectory.Substring(sourcePath.Length));

                        Scan(Options.Clone(options, options.OverwriteExistingPackages, subDirectory, targetDirectory), showParseTree, createNewScanner, fileNeedsScan);
                    }
                }
            }
            else
            {
                throw new DirectoryNotFoundException($"WARNING: Source path \"{sourcePath}\" cannot be found.");
            }
        }

        protected static void ScanFile(Options options, string fileName, bool showParseTree, CreateNewScannerFunction createNewScanner, FileNeedsScanFunction fileNeedsScan = null)
        {
            if ((object)fileNeedsScan == null)
                fileNeedsScan = _ => true;

            if (s_processedFiles.Contains(fileName))
                return;

            s_processedFiles.Add(fileName);

            if (!fileNeedsScan(fileName))
                return;

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
                ScanImports(scanner, showParseTree, createNewScanner, fileNeedsScan);
        }

        private static void ScanImports(ScannerBase scanner, bool showParseTree, CreateNewScannerFunction createNewScanner, FileNeedsScanFunction fileNeedsScan)
        {
            string[] imports = s_importQueue.ToArray();
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
                        Scan(Options.Clone(options, options.OverwriteExistingPackages, goRootImport, targetPath), showParseTree, createNewScanner, fileNeedsScan);
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

                    Scan(Options.Clone(options, options.OverwriteExistingPackages, goPathImport, targetPath), showParseTree, createNewScanner, fileNeedsScan);
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

        protected static string GetAbsolutePath(string filePath)
        {
            if (!Path.IsPathRooted(filePath))
                filePath = Path.Combine(GoPath, filePath);

            return filePath;
        }
    }
}
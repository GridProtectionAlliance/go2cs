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
// ReSharper disable InconsistentNaming

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Dahomey.Json;
using Dahomey.Json.Serialization.Conventions;
using go2cs.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static go2cs.Common;

#pragma warning disable SCS0018 // Path traversal
#pragma warning disable SCS0028 // Deserialization

namespace go2cs;

public delegate ScannerBase CreateNewScannerFunction(BufferedTokenStream tokenStream, GoParser parser, Options options, string fileName);
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
public abstract partial class ScannerBase : GoParserBaseListener
{
    public const string RootNamespace = "go";
    public const string ClassSuffix = "_package";
    public const string AddressPrefix = "_addr_";

    private readonly List<string> m_warnings = new();

    protected readonly DependencyCounter RequiredUsings = new();

    private static readonly Regex s_buildDirective = new(@"^\s*\/\/\s*\+build", RegexOptions.Singleline | RegexOptions.Compiled);

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

    public GoParser Parser { get; }

    public FileMetadata Metadata { get; } // Only available after pre-scan

    public string SourceFileName { get; }

    public string SourceFilePath { get; }

    public string TargetFileName { get; }

    public string TargetFilePath { get; }

    public string[] Warnings => m_warnings.ToArray();

    protected string CurrentImportPath { get; private set; }

    protected bool UsesUnsafePointers { get; set; }

    protected bool InFunction { get; set; }

    protected FunctionInfo CurrentFunction { get; set; }

    protected string OriginalFunctionName { get; set; }
        
    protected string CurrentFunctionName { get; set; }

    protected ScannerBase(BufferedTokenStream tokenStream, GoParser parser, Options options, string fileName)
    {
        Options = options;

        if (fileName is null)
            throw new ArgumentNullException(nameof(fileName));

        if (!File.Exists(fileName))
            throw new FileNotFoundException($"WARNING: Source file \"{fileName}\" cannot be found.", fileName);

        TokenStream = tokenStream;
        Parser = parser;

        GetFilePaths(options, null, fileName, out string sourceFileName, out string sourceFilePath, out string targetFileName, out string targetFilePath);

        SourceFileName = sourceFileName;
        SourceFilePath = sourceFilePath;
        TargetFileName = targetFileName;
        TargetFilePath = targetFilePath;

        FolderMetadata folderMetadata = GetFolderMetadata(Options, null, SourceFileName, targetFilePath);

        if (folderMetadata is not null && folderMetadata.Files.TryGetValue(fileName, out FileMetadata metadata))
            Metadata = metadata;
    }

    public virtual (bool, string) Scan(bool showParseTree)
    {
        IParseTree sourceTree = Parser.sourceFile();

        if (showParseTree)
            Console.WriteLine(sourceTree.ToStringTree(Parser));

        // Walk parsed source tree to start visiting nodes
        ParseTreeWalker walker = new();

        try
        {
            walker.Walk(this, sourceTree);
        }
        catch (CgoTargetException)
        {
            return (true, "Import \"C\" CGO Target");
        }

        return (true, null);
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
    protected static readonly List<string> Imports;

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

        NewLineDelimeters = new[] { "\r\n", "\n" };

        s_processedFiles = new(StringComparer.OrdinalIgnoreCase);
        s_processedImports = new(StringComparer.OrdinalIgnoreCase);
        ImportQueue = new(StringComparer.OrdinalIgnoreCase);
        Imports = new();
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
        Imports.Clear();
        TotalSkippedFiles = 0;
        TotalSkippedPackages = 0;
        TotalWarnings = 0;
    }

    protected static void Scan(Options options, bool showParseTree, CreateNewScannerFunction createNewScanner, FileNeedsScanFunction fileNeedsScan = null, SkippedFileScanFunction handleSkippedScan = null)
    {
        if (fileNeedsScan is null)
            fileNeedsScan = DefaultFileNeedsScan;

        string sourcePath = GetAbsolutePath(options.SourcePath);

        if (File.Exists(sourcePath))
        {
            string filePath = Path.GetDirectoryName(sourcePath) ?? string.Empty;

            if (filePath.StartsWith(GoRoot, StringComparison.OrdinalIgnoreCase))
                ScanFile(Options.Clone(options, options.OverwriteExistingFiles, sourcePath, Path.Combine(options.TargetGoSrcPath, filePath[GoRoot.Length..])), sourcePath, showParseTree, createNewScanner, fileNeedsScan, handleSkippedScan);
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
                        targetDirectory = Path.Combine(targetDirectory, RemovePathPrefix(subDirectory[sourcePath.Length..]));

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
        if (fileNeedsScan is null)
            fileNeedsScan = DefaultFileNeedsScan;

        if (!options.ParseGoOSTargets && IsGoOSTarget(fileName, out string suffix))
        {
            Console.WriteLine($"Encountered \"{suffix}\" OS target for \"{fileName}\", skipping scan...{Environment.NewLine}");
            handleSkippedScan?.Invoke(options, fileName, showParseTree);
            return;
        }

        if (!options.ParseGoArchTargets && IsGoArchTarget(fileName, out suffix))
        {
            Console.WriteLine($"Encountered \"{suffix}\" architecture target for \"{fileName}\", skipping scan...{Environment.NewLine}");
            handleSkippedScan?.Invoke(options, fileName, showParseTree);
            return;
        }

        if (!options.SkipBuildIgnoreDirectiveCheck || !options.ParseCgoTargets)
        {
            using StreamReader source = File.OpenText(fileName);
            bool foundIgnoreBuildDirective = false;
            bool foundCgoBuildDirective = false;
            string line;

            while ((line = source.ReadLine()) is not null)
            {
                // Directives must come before package definition
                if (line.TrimStart().StartsWith("package "))
                    break;

                if (!s_buildDirective.IsMatch(line))
                    continue;

                int index = line.IndexOf("+build", StringComparison.Ordinal);

                HashSet<string> directives = new(line[(index + 6)..].Split(' ', StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);

                if (!options.SkipBuildIgnoreDirectiveCheck && directives.Contains("ignore"))
                {
                    foundIgnoreBuildDirective = true;
                    break;
                }

                if (!options.ParseCgoTargets && directives.Any(directive => directive.Contains("cgo")))
                {
                    foundCgoBuildDirective = true;
                    break;
                }
            }

            if (foundIgnoreBuildDirective)
            {
                Console.WriteLine($"Encountered \"+build ignore\" directive for \"{fileName}\", skipping scan...{Environment.NewLine}");
                handleSkippedScan?.Invoke(options, fileName, showParseTree);
                return;
            }

            if (foundCgoBuildDirective)
            {
                Console.WriteLine($"Encountered \"+build cgo\" directive for \"{fileName}\", skipping scan...{Environment.NewLine}");
                handleSkippedScan?.Invoke(options, fileName, showParseTree);
                return;
            }
        }

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
            inputStream = new(reader);

        GoLexer lexer = new(inputStream);
        CommonTokenStream tokenStream = new(lexer);
        GoParser parser = new(tokenStream);
        ScannerBase scanner;

    #if !DEBUG
        try
        {
    #endif
        scanner = createNewScanner(tokenStream, parser, options, fileName);
    #if !DEBUG
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            TotalSkippedFiles++;
            return;
        }
    #endif

        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ParserErrorListener(scanner));

        if (options.OverwriteExistingFiles || !File.Exists(scanner.TargetFileName))
        {
            scanner.BeforeScan();

            (bool success, string result) = scanner.Scan(showParseTree);

            if (!success)
            {
                Console.WriteLine($"Encountered \"{result}\" for \"{fileName}\", skipping scan...{Environment.NewLine}");
                handleSkippedScan?.Invoke(options, fileName, showParseTree);
                return;
            }

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

            string importPath = AddPathSuffix(import.Replace("/", Path.DirectorySeparatorChar.ToString()));
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

    protected static void GetFilePaths(Options options, string rootSourcePath, string fileName, out string sourceFileName, out string sourceFilePath, out string targetFileName, out string targetFilePath)
    {
        string rootTargetPath = string.IsNullOrEmpty(options.RootTargetPath) ? options.RootTargetPath : Path.GetFullPath(options.RootTargetPath);

        rootTargetPath = string.IsNullOrWhiteSpace(rootTargetPath) ? Path.GetFullPath(options.TargetGoSrcPath) : rootTargetPath;

        if (string.IsNullOrWhiteSpace(rootSourcePath))
            rootSourcePath = GetAbsolutePath(options.RootSourcePath);

        sourceFileName = Path.GetFullPath(fileName);
        sourceFilePath = Path.GetDirectoryName(sourceFileName) ?? string.Empty;
        targetFileName = $"{Path.GetFileNameWithoutExtension(sourceFileName)}.cs";

        if (string.IsNullOrWhiteSpace(options.TargetPath))
            targetFilePath = !options.ConvertStandardLibrary || string.IsNullOrWhiteSpace(options.TargetGoSrcPath) ? sourceFilePath : Path.GetFullPath(options.TargetGoSrcPath);
        else
            targetFilePath = options.TargetPath;

        string targetSubDirectory = RemovePathSuffix(RemovePathPrefix(targetFilePath[rootTargetPath.Length..]));
        string sourceSubDirectory = sourceFilePath.StartsWith(rootSourcePath) ? RemovePathSuffix(RemovePathPrefix(sourceFilePath[rootSourcePath.Length..])) : string.Empty;

        if (!targetSubDirectory.Equals(sourceSubDirectory))
            targetFilePath = Path.Combine(targetFilePath, sourceSubDirectory!);

        targetFilePath = AddPathSuffix(targetFilePath);
        targetFileName = Path.Combine(targetFilePath, targetFileName);
    }

    protected static string GetFolderMetadataFileName(Options options, string rootSourcePath, string fileName, string targetFilePath = null)
    {
        if (string.IsNullOrWhiteSpace(targetFilePath))
            GetFilePaths(options, rootSourcePath, fileName, out _, out _, out _, out targetFilePath);

        string lastDirName = GetLastDirectoryName(targetFilePath);
        return $"{targetFilePath}{lastDirName}.metadata";
    }

    protected static FolderMetadata GetFolderMetadata(Options options, string rootSourcePath, string fileName, string targetFilePath = null)
    {
        string folderMetadataFileName = GetFolderMetadataFileName(options, rootSourcePath, fileName, targetFilePath);

        if (!File.Exists(folderMetadataFileName))
            return null;

        if (folderMetadataFileName.Equals(s_currentFolderMetadataFileName, StringComparison.OrdinalIgnoreCase) && s_currentFolderMetadata is not null)
            return s_currentFolderMetadata;

        FolderMetadata folderMetadata;

    #if !DEBUG
        try
        {
    #endif
        string serializedData = File.ReadAllText(folderMetadataFileName);
        folderMetadata = JsonSerializer.Deserialize<FolderMetadata>(serializedData, GetSerializationOptions());
    #if !DEBUG
        }
        catch (Exception ex)
        {
            folderMetadata = null;

            if (!folderMetadataFileName.Equals(s_currentFolderMetadataFileName, StringComparison.OrdinalIgnoreCase))
                Console.WriteLine($"Failed to load folder metadata from \"{folderMetadataFileName}\": {ex.Message}");
        }
    #endif

        s_currentFolderMetadataFileName = folderMetadataFileName;
        s_currentFolderMetadata = folderMetadata;

        return folderMetadata;
    }

    protected static JsonSerializerOptions GetSerializationOptions()
    {
        JsonSerializerOptions options = new()
        { IncludeFields = true };

        options.SetupExtensions();            
        DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            
        registry.ClearConventions();

        registry.RegisterConvention(new DefaultDiscriminatorConvention<DerivedTypeInfo>(options, "DerivedTypeInfo"));
        registry.RegisterType<PointerTypeInfo>();
        registry.RegisterType<ArrayTypeInfo>();
        registry.RegisterType<MapTypeInfo>();

        registry.RegisterConvention(new DefaultDiscriminatorConvention<DerivedFunctionSignature>(options, "DerivedFunctionSignature"));
        registry.RegisterType<MethodSignature>();

        return options;
    }

    protected static FolderMetadata LoadImportMetadata(Options options, string targetImport, out string warning)
    {
        int lastSlash = targetImport.LastIndexOf('/');
        string packageName = lastSlash > -1 ? targetImport[(lastSlash + 1)..] : targetImport;
        string importPath = $"{AddPathSuffix(targetImport.Replace("/", Path.DirectorySeparatorChar.ToString()))}{packageName}.go";
        string go2csPath = Path.Combine(GoPath, "go2cs");
        string goRootImport = Path.Combine(GoRoot, importPath);
        string goPathImport = Path.Combine(go2csPath, importPath);
        string targetPath = string.IsNullOrWhiteSpace(options.TargetGoSrcPath) ? go2csPath : options.TargetGoSrcPath;
        FolderMetadata metadata;

        warning = default;
        options = Options.Clone(options, options.OverwriteExistingFiles, GoRoot, targetPath);
        metadata = GetFolderMetadata(options, GoRoot, goRootImport);

        if (metadata is not null)
            return metadata;

        options = Options.Clone(options, options.OverwriteExistingFiles, go2csPath, targetPath);
        metadata = GetFolderMetadata(options, go2csPath, goPathImport);

        if (metadata is not null)
            return metadata;

        StringBuilder loadWarning = new();

        loadWarning.AppendLine($"WARNING: Failed to locate package metadata for \"{targetImport}\" import at either:");
        loadWarning.AppendLine($"    {GetFolderMetadataFileName(options, GoRoot, goRootImport)} (from -g Go source target path)");
        loadWarning.AppendLine($"    {GetFolderMetadataFileName(options, go2csPath, goPathImport)} (from %GOPATH%)");

        warning = loadWarning.ToString();
        return null;
    }

    protected static string GetValidIdentifierName(string identifier)
    {
        int lastDotIndex = identifier.LastIndexOf('.');

        if (lastDotIndex > 0)
            identifier = identifier[(lastDotIndex + 1)..];

        return SanitizedIdentifier(identifier);
    }

    protected string GetUniqueIdentifier<T>(IDictionary<string, T> source, string identifier)
    {
        if (identifier.Equals("_"))
            return identifier;

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

        string typeName = typeInfo.TypeName;
        string fullTypeName = typeInfo.FullTypeName;

        string[] parts = typeName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 2)
            typeName = $"ptr<{parts[1]}>";

        parts = fullTypeName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 2)
            fullTypeName = $"ptr<{parts[1]}>";

        return new PointerTypeInfo
        {
            Name = typeInfo.Name,
            TypeName = typeName,
            FullTypeName = fullTypeName,
            TypeClass = TypeClass.Simple,
            TargetTypeInfo = typeInfo
        };
    }

    protected TypeInfo ConvertByRefToNativePointer(TypeInfo typeInfo)
    {
        if (!typeInfo.IsByRefPointer)
            return typeInfo;

        string typeName = typeInfo.TypeName;
        string fullTypeName = typeInfo.FullTypeName;

        string[] parts = typeName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 2)
            typeName = $"*{parts[1]}";

        parts = fullTypeName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 2)
            fullTypeName = $"*{parts[1]}";

        return new PointerTypeInfo
        {
            Name = typeInfo.Name,
            TypeName = typeName,
            FullTypeName = fullTypeName,
            TypeClass = TypeClass.Simple,
            TargetTypeInfo = typeInfo
        };
    }

    protected static string GetAbsolutePath(string filePath)
    {
        if (!Path.IsPathRooted(filePath))
            filePath = Path.Combine(GoPath, filePath!);

        return Path.GetFullPath(filePath);
    }

    private static bool DefaultFileNeedsScan(Options options, string fileName, out string message)
    {
        message = null;
        return true;
    }
}

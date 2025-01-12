//******************************************************************************************************
//  Options.cs - Gbtc
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
//  05/16/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using static go2cs.Common;

namespace go2cs;

// Options class is immutable to prevent unintended updates
public class Options
{        
    public const string DefaultExcludeFiles = "$.^"; // Default to exclude none, never an end matched before begin on a single line

#if Windows
    public const string DefaultTargetGoSrcPath = "%GOPATH%\\src\\go2cs";
#else
    public const string DefaultTargetGoSrcPath = "%GOPATH%/src/go2cs";
#endif

    private readonly Regex m_excludeExpression;

    [Option('l', "local", Required = false, Default = false, HelpText = "Set to only convert local files in source path. Default is to recursively convert all encountered \"import\" packages.")]
    public bool LocalConvertOnly { get; }

    [Option('o', "reconvert-local", Required = false, Default = false, HelpText = "Set to overwrite, i.e., reconvert, any existing local converted files.")]
    public bool OverwriteExistingFiles { get; }

    [Option('i', "reconvert-imports", Required = false, Default = false, HelpText = "Set to overwrite, i.e., reconvert, any existing files from imported packages.")]
    public bool OverwriteExistingPackages { get; }

    [Option('h', "exclude-header", Required = false, Default = false, HelpText = "Set to exclude header conversion comments which include original source file path and conversion time.")]
    public bool ExcludeHeaderComments { get; }

    [Option('t', "show-parse-tree", Required = false, Default = false, HelpText = "Set to show syntax tree of parsed source file.")]
    public bool ShowParseTree { get; }

    [Option('e', "exclude", Required = false, Default = DefaultExcludeFiles, HelpText = "Regular expression to exclude certain files from conversion, e.g., \"^.+_test\\.go$\". Defaults to exclude none.")]
    public string ExcludeFiles { get; }

    // Default is false since it is desirable to be able to download a pre-converted set of standard library files
    [Option('s', "convert-stdlib", Required = false, Default = false, HelpText = "Set to convert needed packages from Go standard library files found in \"%GOROOT%\\src\".")]
    public bool ConvertStandardLibrary { get; }

    [Option('r', "recursive", Required = false, Default = false, HelpText = "Set to recursively convert source files in subdirectories when a Go source path is specified.")]
    public bool RecurseSubdirectories { get; }

    [Option('m', "update-metadata", Required = false, Default = false, HelpText = "Set to force update of pre-scan metadata.")]
    public bool ForceMetadataUpdate { get; }

    [Option('u', "skip-convert", Required = false, Default = false, HelpText = "Set to only update pre-scan metadata and skip conversion operations.")]
    public bool OnlyUpdateMetadata { get; }

    [Option('g', "stdlib-path", Required = false, Default = DefaultTargetGoSrcPath, HelpText = "Target path for converted Go standard library source files.")]
    public string TargetGoSrcPath { get; }

    [Option('c', "compatible", Required = false, Default = false, HelpText = "Set to target legacy compatible code, e.g., block scoped namespaces and per-file static \"go.builtin\" using. Required for code sets prior to C# 10.")]
    public bool WriteLegacyCompatibleCode { get; }

    [Option('a', "ansi-braces", Required = false, Default = false, HelpText = "Set to use ANSI brace style, i.e., start brace on new line, instead of K&R / Go brace style.")]
    public bool UseAnsiBraceStyle { get; }

    [Option('k', "skip-ignore-directive-check", Required = false, Default = false, HelpText = "Set to skip check for \"+build ignore\" directive and attempt conversion anyway.")]
    public bool SkipBuildIgnoreDirectiveCheck { get; }

    [Option('C', "cgo", Required = false, Default = false, HelpText = "Set to convert CGO files, i.e., skip check for \"+build cgo\" directive or import \"C\" and attempt conversion anyway.")]
    public bool ParseCgoTargets { get; }

    [Option('O', "goos", Required = false, Default = false, HelpText = "Set to convert Go OS targeted files, i.e., skip check for OS target file name suffixes and attempt conversion anyway.")]
    public bool ParseGoOSTargets { get; }

    [Option('A', "goarch", Required = false, Default = false, HelpText = "Set to convert Go architecture targeted files, i.e., skip check for architecture target file name suffixes and attempt conversion anyway.")]
    public bool ParseGoArchTargets { get; }

    [Value(0, Required = true, HelpText = "Go source path or file name to convert.")]
    public string SourcePath { get; }

    [Value(1, Required = false, HelpText = "Target path for converted files. If not specified, all files (except for Go standard library files) will be converted to source path.")]
    public string TargetPath { get; }

    [Option(Hidden = true)]
    public string RootSourcePath { get; }

    [Option(Hidden = true)]
    public string RootTargetPath { get; }

    public Options(
        /* l */ bool localConvertOnly,
        /* o */ bool overwriteExistingFiles,
        /* i */ bool overwriteExistingPackages,
        /* h */ bool excludeHeaderComments,
        /* t */ bool showParseTree,
        /* e */ string excludeFiles,
        /* s */ bool convertStandardLibrary,
        /* r */ bool recurseSubdirectories,
        /* m */ bool forceMetadataUpdate,
        /* u */ bool onlyUpdateMetadata,
        /* g */ string targetGoSrcPath,
        /* c */ bool writeLegacyCompatibleCode,
        /* a */ bool useAnsiBraceStyle,
        /* k */ bool skipBuildIgnoreDirectiveCheck,
        /* C */ bool parseCgoTargets,
        /* O */ bool parseGoOSTargets,
        /* A */ bool parseGoArchTargets,
        string sourcePath,
        string targetPath,
        string rootSourcePath,
        string rootTargetPath)
    {
        if (string.IsNullOrEmpty(excludeFiles))
            excludeFiles = DefaultExcludeFiles;

        if (string.IsNullOrEmpty(targetGoSrcPath))
            targetGoSrcPath = DefaultTargetGoSrcPath;

        /* l */ LocalConvertOnly = localConvertOnly;
        /* o */ OverwriteExistingFiles = overwriteExistingFiles;
        /* i */ OverwriteExistingPackages = overwriteExistingPackages;
        /* h */ ExcludeHeaderComments = excludeHeaderComments;
        /* t */ ShowParseTree = showParseTree;
        /* e */ ExcludeFiles = excludeFiles;
        /* s */ ConvertStandardLibrary = convertStandardLibrary;
        /* r */ RecurseSubdirectories = recurseSubdirectories;
        /* m */ ForceMetadataUpdate = forceMetadataUpdate;
        /* u */ OnlyUpdateMetadata = onlyUpdateMetadata;
        /* g */ TargetGoSrcPath = AddPathSuffix(Path.GetFullPath(Environment.ExpandEnvironmentVariables(targetGoSrcPath)));
        /* c */ WriteLegacyCompatibleCode = writeLegacyCompatibleCode;
        /* a */ UseAnsiBraceStyle = useAnsiBraceStyle;
        /* k */ SkipBuildIgnoreDirectiveCheck = skipBuildIgnoreDirectiveCheck;
        /* C */ ParseCgoTargets = parseCgoTargets;
        /* O */ ParseGoOSTargets = parseGoOSTargets;
        /* A */ ParseGoArchTargets = parseGoArchTargets;
        SourcePath = sourcePath is null ? null : Environment.ExpandEnvironmentVariables(sourcePath);
        TargetPath = targetPath is null ? null : Environment.ExpandEnvironmentVariables(targetPath);
        RootSourcePath = rootSourcePath ?? SourcePath ?? string.Empty;
        RootTargetPath = rootTargetPath ?? (string.IsNullOrWhiteSpace(TargetPath) && ConvertStandardLibrary ? TargetGoSrcPath : TargetPath) ?? string.Empty;

        m_excludeExpression = new Regex(ExcludeFiles, RegexOptions.Compiled | RegexOptions.Singleline);
    }

    public Regex GetExcludeExpression() => m_excludeExpression;

    public static Options Clone(Options options, bool overwriteExistingFiles, string sourcePath, string targetPath) => 
        new(
            /* l */ options.LocalConvertOnly,
            /* o */ overwriteExistingFiles,
            /* i */ options.OverwriteExistingPackages,
            /* h */ options.ExcludeHeaderComments,
            /* t */ options.ShowParseTree,
            /* e */ options.ExcludeFiles,
            /* s */ options.ConvertStandardLibrary,
            /* r */ options.RecurseSubdirectories,
            /* m */ options.ForceMetadataUpdate,
            /* u */ options.OnlyUpdateMetadata,
            /* g */ options.TargetGoSrcPath,
            /* c */ options.WriteLegacyCompatibleCode,
            /* a */ options.UseAnsiBraceStyle,
            /* k */ options.SkipBuildIgnoreDirectiveCheck,
            /* C */ options.ParseCgoTargets,
            /* O */ options.ParseGoOSTargets,
            /* A */ options.ParseGoArchTargets,
            sourcePath, 
            targetPath,
            options.RootSourcePath,
            options.RootTargetPath);

    // Private constructor only used by examples
    private Options(bool localConvertOnly, string sourcePath, bool convertStandardLibrary = false, bool recurseSubdirectories = false)
    {
        LocalConvertOnly = localConvertOnly;
        OverwriteExistingFiles = false;
        OverwriteExistingPackages = false;
        ShowParseTree = false;
        ExcludeFiles = null;
        ConvertStandardLibrary = convertStandardLibrary;
        RecurseSubdirectories = recurseSubdirectories;
        ForceMetadataUpdate = false;
        OnlyUpdateMetadata = false;
        TargetGoSrcPath = null;
        SourcePath = sourcePath;
        TargetPath = null;
    }

    [Usage]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("·\r\n--> Example to convert a single Go file", new Options(true, "Main.go"));
            yield return new Example("·\r\n--> Example to convert a Go project", new Options(false, "MyProject\\"));
            yield return new Example("·\r\n--> Example to convert Go Standard Library", new Options(false, "C:\\Go\\src\\", true, true));
        }
    }
}

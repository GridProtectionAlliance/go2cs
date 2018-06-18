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

using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using static go2cs.Common;

namespace go2cs
{
    // Options class is immutable to prevent unintended updates
    public class Options
    {        
        public const string DefaultExcludeFiles = "$.^"; // Default to exclude none, never an end matched before begin on a single line
        public const string DefaultTargetGoSrcPath = "%GOPATH%\\src\\go2cs";

        private readonly Regex m_excludeExpression;

        [Option('l', Required = false, Default = false, HelpText = "Set to only convert local files in source path. Default is to recursively convert all encountered \"import\" packages.")]
        public bool LocalConvertOnly
        {
            get;
        }

        [Option('o', Required = false, Default = false, HelpText = "Set to overwrite, i.e., reconvert, any existing local converted files.")]
        public bool OverwriteExistingFiles
        {
            get;
        }

        [Option('i', Required = false, Default = false, HelpText = "Set to overwrite, i.e., reconvert, any existing files from imported packages.")]
        public bool OverwriteExistingPackages
        {
            get;
        }

        [Option('t', Required = false, Default = false, HelpText = "Set to show syntax tree of parsed source file.")]
        public bool ShowParseTree
        {
            get;
        }

        [Option('e', Required = false, Default = DefaultExcludeFiles, HelpText = "Regular expression to exclude certain files from conversion, e.g., \"^.+_test\\.go$\". Defaults to exclude none.")]
        public string ExcludeFiles
        {
            get;
        }

        // Default is false since it is desirable to be able to download a pre-converted set of standard library files
        [Option('s', Required = false, Default = false, HelpText = "Set to convert needed packages from Go standard library files found in \"%GOROOT%\\src\".")]
        public bool ConvertStandardLibrary
        {
            get;
        }

        [Option('r', Required = false, Default = false, HelpText = "Set to recursively convert source files in subdirectories when a Go source path is specified.")]
        public bool RecurseSubdirectories
        {
            get;
        }

        [Option('m', Required = false, Default = false, HelpText = "Set to force update of pre-scan metadata.")]
        public bool ForceMetadataUpdate
        {
            get;
        }

        [Option('g', Required = false, Default = DefaultTargetGoSrcPath, HelpText = "Target path for converted Go standard library source files.")]
        public string TargetGoSrcPath
        {
            get;
        }

        [Value(0, Required = true, HelpText = "Go source path or file name to convert.")]
        public string SourcePath
        {
            get;
        }

        [Value(1, Required = false, HelpText = "Target path for converted files. If not specified, all files (except for Go standard library files) will be converted to source path.")]
        public string TargetPath
        {
            get;
        }

        public Options(
            bool localConvertOnly,
            bool overwriteExistingFiles,
            bool overwriteExistingPackages,
            bool showParseTree,
            string excludeFiles,
            bool convertStandardLibrary,
            bool recurseSubdirectories,
            bool forceMetadataUpdate,
            string targetGoSrcPath,
            string sourcePath,
            string targetPath)
        {
            if (string.IsNullOrEmpty(excludeFiles))
                excludeFiles = DefaultExcludeFiles;

            if (string.IsNullOrEmpty(targetGoSrcPath))
                targetGoSrcPath = DefaultTargetGoSrcPath;

            LocalConvertOnly = localConvertOnly;
            OverwriteExistingFiles = overwriteExistingFiles;
            OverwriteExistingPackages = overwriteExistingPackages;
            ShowParseTree = showParseTree;
            ExcludeFiles = excludeFiles;
            ConvertStandardLibrary = convertStandardLibrary;
            RecurseSubdirectories = recurseSubdirectories;
            ForceMetadataUpdate = forceMetadataUpdate;
            TargetGoSrcPath = AddPathSuffix(Path.GetFullPath(Environment.ExpandEnvironmentVariables(targetGoSrcPath)));
            SourcePath = sourcePath == null ? null : Environment.ExpandEnvironmentVariables(sourcePath);
            TargetPath = targetPath == null ? null : Environment.ExpandEnvironmentVariables(targetPath);

            m_excludeExpression = new Regex(ExcludeFiles, RegexOptions.Compiled | RegexOptions.Singleline);
        }

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
            TargetGoSrcPath = null;
            SourcePath = sourcePath;
            TargetPath = null;
       }

        public Regex GetExcludeExpression() => m_excludeExpression;

        public static Options Clone(Options options, bool overwriteExistingFiles, string sourcePath, string targetPath) => 
            new Options(
                options.LocalConvertOnly,
                overwriteExistingFiles,
                options.OverwriteExistingPackages,
                options.ShowParseTree, 
                options.ExcludeFiles,
                options.ConvertStandardLibrary,
                options.RecurseSubdirectories,
                options.ForceMetadataUpdate,
                options.TargetGoSrcPath, 
                sourcePath, 
                targetPath);

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
}
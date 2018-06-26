//******************************************************************************************************
//  PreScanner.cs - Gbtc
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
//  06/18/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using Antlr4.Runtime;
using go2cs.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace go2cs
{
    public partial class PreScanner :ScannerBase
    {
        private readonly Dictionary<string, (string targetImport, string targetUsing)> m_importAliases = new Dictionary<string, (string targetImport, string targetUsing)>(StringComparer.Ordinal);
        private readonly Dictionary<string, InterfaceInfo> m_interfaces = new Dictionary<string, InterfaceInfo>(StringComparer.Ordinal);
        private readonly Dictionary<string, StructInfo> m_structs = new Dictionary<string, StructInfo>(StringComparer.Ordinal);
        private readonly Dictionary<string, FunctionInfo> m_functions = new Dictionary<string, FunctionInfo>(StringComparer.Ordinal);

        public string FolderMetadataFileName { get; }

        public PreScanner(BufferedTokenStream tokenStream, GolangParser parser, Options options, string fileName) : base(tokenStream, parser, options, fileName)
        {
            FolderMetadataFileName = GetFolderMetadataFileName(options, fileName);
        }

        public override void Scan(bool _)
        {
            // Base class walks parse tree
            base.Scan(false);

            FolderMetadata folderMetadata = GetFolderMetadata(Options, SourceFileName) ?? new FolderMetadata();
            FileMetadata fileMetadata = folderMetadata.Files.GetOrAdd(SourceFileName, new FileMetadata());

            fileMetadata.Package = Package;
            fileMetadata.PackageImport = PackageImport;
            fileMetadata.SourceFileName = SourceFileName;
            fileMetadata.TargetFileName = TargetFileName;
            fileMetadata.ImportAliases = m_importAliases;
            fileMetadata.Interfaces = m_interfaces;
            fileMetadata.Structs = m_structs;
            fileMetadata.Functions = m_functions;
            fileMetadata.LastUpdate = DateTime.UtcNow;

            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream stream = File.Create(FolderMetadataFileName))
                formatter.Serialize(stream, folderMetadata);
        }

        protected override void BeforeScan()
        {
            Console.WriteLine($"Starting pre-scan of \"{SourceFileName}\"...");
        }

        protected override void AfterScan()
        {
            Console.WriteLine("    Finished.");
        }

        protected override void SkippingImport(string import)
        {
            Console.WriteLine($"Skipping pre-scan for Go standard library import package \"{import}\".");
            Console.WriteLine();
        }

        public static void Scan(Options options)
        {
            Console.WriteLine("Starting Go code pre-scan to update metadata...");
            Console.WriteLine();

            ResetScanner();
            Scan(options, false, CreateNewPreScanner, MetadataOutOfDate, HandleSkippedScan);
        }

        private static ScannerBase CreateNewPreScanner(BufferedTokenStream tokenStream, GolangParser parser, Options options, string fileName)
        {
            return new PreScanner(tokenStream, parser, options, fileName);
        }

        private static bool MetadataOutOfDate(Options options, string fileName, out string message)
        {
            message = null;

            if (options.ForceMetadataUpdate)
                return true;

            FolderMetadata folderMetadata = GetFolderMetadata(options, fileName);

            if ((object)folderMetadata == null || !folderMetadata.Files.TryGetValue(fileName, out FileMetadata fileMetadata))
                return true;

            if (File.GetLastWriteTimeUtc(fileName) > fileMetadata.LastUpdate)
                return true;

            message = $"Metadata for \"{fileName}\" is up to date.{Environment.NewLine}";

            return false;
        }

        private static void HandleSkippedScan(Options options, string fileName, bool showParseTree)
        {
            if (options.LocalConvertOnly)
                return;

            // Check meta-data status for imports
            GetFilePaths(options, fileName, out string sourceFileName, out string _, out string _, out string _);
            FolderMetadata folderMetadata = GetFolderMetadata(options, sourceFileName);

            if ((object)folderMetadata != null && folderMetadata.Files.TryGetValue(sourceFileName, out FileMetadata fileMetadata))
            {
                ImportQueue.UnionWith(fileMetadata.ImportAliases.Select(import => import.Value.targetImport));
                ScanImports(CreateNewPreScanner(null, null, options, fileName), showParseTree, CreateNewPreScanner, MetadataOutOfDate, HandleSkippedScan);
            }
        }
    }
}

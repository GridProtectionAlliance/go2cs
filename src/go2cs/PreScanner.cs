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
using System.Runtime.Serialization.Formatters.Binary;
using static go2cs.Common;

namespace go2cs
{
    public class PreScanner :ScannerBase
    {
        private readonly List<InterfaceInfo> m_interfaces = new List<InterfaceInfo>();
        private readonly List<StructInfo> m_structs = new List<StructInfo>();
        private readonly List<FunctionInfo> m_functions = new List<FunctionInfo>();

        public string FolderMetadataFileName { get; }

        public string Package { get; private set; }

        public string PackageImport { get; private set; }

        public PreScanner(BufferedTokenStream tokenStream, GolangParser parser, Options options, string fileName) : base(tokenStream, parser, options, fileName)
        {
            FolderMetadataFileName = GetFolderMetadataFileName(options, fileName);
        }

        public override void Scan(bool _)
        {
            // Base class walks parse tree
            base.Scan(false);

            FolderMetadata folderMetadata = GetFolderMetadata(Options, SourceFileName) ?? new FolderMetadata();
            FileMetadata fileInfo = folderMetadata.Files.GetOrAdd(SourceFileName, new FileMetadata());

            fileInfo.Package = Package;
            fileInfo.PackageImport = PackageImport;
            fileInfo.SourceFileName = SourceFileName;
            fileInfo.TargetFileName = TargetFileName;
            fileInfo.Interfaces = m_interfaces.ToArray();
            fileInfo.Structs = m_structs.ToArray();
            fileInfo.Functions = m_functions.ToArray();
            fileInfo.LastUpdate = DateTime.UtcNow;

            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream stream = File.Create(FolderMetadataFileName))
                formatter.Serialize(stream, folderMetadata);
        }
        public override void EnterPackageClause(GolangParser.PackageClauseContext context)
        {
            Package = SanitizedIdentifier(context.IDENTIFIER().GetText());

            if (Package.Equals("main"))
            {
                PackageImport = Package;
            }
            else
            {
                // Define package import path
                PackageImport = Path.GetDirectoryName(SourceFileName) ?? Package;
                PackageImport = PackageImport.Replace(GoRoot, "");
                PackageImport = PackageImport.Replace(GoPath, "");

                while (PackageImport.StartsWith(Path.DirectorySeparatorChar.ToString()) || PackageImport.StartsWith(Path.AltDirectorySeparatorChar.ToString()))
                    PackageImport = PackageImport.Substring(1);

                while (PackageImport.EndsWith(Path.DirectorySeparatorChar.ToString()) || PackageImport.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                    PackageImport = PackageImport.Substring(0, PackageImport.Length - 1);

                int lastSlash;

                if (Path.IsPathRooted(PackageImport))
                {
                    // File converted was outside %GOPATH% and %GOROOT%
                    lastSlash = PackageImport.LastIndexOf('\\');

                    if (lastSlash > -1)
                        PackageImport = $"{PackageImport.Substring(lastSlash + 1)}";
                }

                PackageImport = $"{PackageImport.Replace('\\', '/')}";

                lastSlash = PackageImport.LastIndexOf('/');
                string package = SanitizedIdentifier(lastSlash > -1 ? PackageImport.Substring(lastSlash + 1) : PackageImport);

                if (!package.Equals(Package))
                {
                    AddWarning(context, $"Defined package clause \"{Package}\" does not match file path \"{SourceFileName}\"");
                    PackageImport = lastSlash > -1 ? $"{PackageImport.Substring(0, lastSlash)}.{Package}" : Package;
                }
            }
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
            Scan(options, false, CreateNewPreScanner, MetadataOutOfDate);
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
    }
}

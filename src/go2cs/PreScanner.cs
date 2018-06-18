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
using System;
using System.IO;
using static go2cs.Common;

namespace go2cs
{
    public class PreScanner :ScannerBase
    {
        public string MetadataFileName { get; }

        public PreScanner(BufferedTokenStream tokenStream, GolangParser parser, Options options, string fileName) : base(tokenStream, parser, options, fileName)
        {
            MetadataFileName = GetMetadataFileName(fileName);
        }

        public override void Scan(bool _)
        {
            // Base class walks parse tree
            base.Scan(false);

            if (!File.Exists(MetadataFileName))
                File.Create(MetadataFileName);
        }

        protected override void BeforeScan()
        {
            Console.WriteLine($"Starting pre-scan of \"{SourceFileName}\"...");
        }

        protected override void AfterScan()
        {
            Console.WriteLine("    Completed pre-scan.");
        }

        protected override void SkippingScan()
        {
            Console.WriteLine($"Metadata for \"{SourceFileName}\" is up to date - skipping pre-scan.");
        }

        protected override void SkippingImport(string import)
        {
            Console.WriteLine($"Skipping pre-scan for Go standard library import package \"{import}\".");
            Console.WriteLine();
        }

        public static void Scan(Options options)
        {
            ResetScanner();
            Scan(options, false, CreateNewPreScanner, fileName => options.ForceMetadataUpdate || MetadataOutOfDate(fileName));
        }

        private static ScannerBase CreateNewPreScanner(BufferedTokenStream tokenStream, GolangParser parser, Options options, string fileName)
        {
            return new PreScanner(tokenStream, parser, options, fileName);
        }

        private static bool MetadataOutOfDate(string fileName)
        {
            string metadataFileName = GetMetadataFileName(fileName);

            if (!File.Exists(metadataFileName))
                return true;

            return File.GetLastWriteTimeUtc(fileName) > File.GetLastWriteTimeUtc(metadataFileName);
        }

        private static string GetMetadataFileName(string fileName)
        {
            string sourceFilePath = AddPathSuffix(Path.GetDirectoryName(fileName));
            string lastDirName = GetLastDirectoryName(sourceFilePath);
            return $"{sourceFilePath}{lastDirName}.metadata";
        }
    }
}

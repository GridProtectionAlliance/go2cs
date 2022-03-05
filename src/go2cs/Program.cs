﻿//******************************************************************************************************
//  Program.cs - Gbtc
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

namespace go2cs;

class Program
{
    static int Main(string[] args)
    {
        DateTime startTime = DateTime.UtcNow;
        Arguments arguments = Arguments.Parse(args);
        Options options = arguments.ParsedOptions;
        int exitCode = 1;

        if (arguments.ParseSuccess)
            exitCode = RunConversion(options);
            
        if (exitCode == 0)
        {
            Console.WriteLine($"Conversion complete for: go2cs {string.Join(" ", args)}");
            Console.WriteLine();
            Console.WriteLine($"Updated {PreScanner.TotalMetadataUpdates:N0} metadata files, {PreScanner.TotalUpToDateMetadata:N0} already up-to-date");

            if (!options.OnlyUpdateMetadata)
            {
                Console.WriteLine($"Converted {ScannerBase.TotalProcessedFiles:N0} Go files to C# with {ScannerBase.TotalWarnings:N0} total warnings");

                if ((!options.OverwriteExistingFiles || !options.OverwriteExistingPackages) && ScannerBase.TotalSkippedFiles > 0)
                    Console.WriteLine($"Skipped {ScannerBase.TotalSkippedFiles:N0} already converted files (-o or -i option not set)");

                if (!options.ConvertStandardLibrary && ScannerBase.TotalSkippedPackages > 0)
                    Console.WriteLine($"Skipped conversion of {ScannerBase.TotalSkippedPackages:N0} standard library packages (-s option not set)");
            }

            Console.WriteLine($"Processing time: {DateTime.UtcNow - startTime}");
        }

        return exitCode;
    }

    private static int RunConversion(Options options)
    {
        int exitCode = 0;

    #if !DEBUG
            try
            {
    #endif
        Common.RestoreResources(options.TargetGoSrcPath);
        PreScanner.Scan(options);
        Converter.Convert(options);
    #if !DEBUG
            }
            catch (TypeInitializationException ex)
            {
                Console.Error.WriteLine($"{Environment.NewLine}Error: {ex.InnerException?.Message ?? ex.Message}");
                exitCode = 2;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{Environment.NewLine}Error: {ex.Message}");
                exitCode = 3;
            }
    #endif

        return exitCode;
    }
}
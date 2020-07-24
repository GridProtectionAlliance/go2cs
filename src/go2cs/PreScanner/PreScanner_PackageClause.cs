//******************************************************************************************************
//  PreScanner_PackageClause.cs - Gbtc
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
//  05/03/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.IO;
using static go2cs.Common;

namespace go2cs
{
    public partial class PreScanner
    {
        public string Package { get; private set; }

        public string PackageImport { get; private set; }

        public override void EnterPackageClause(GoParser.PackageClauseContext context)
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
    }
}
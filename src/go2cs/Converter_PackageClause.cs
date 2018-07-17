//******************************************************************************************************
//  Converter_PackageClause.cs - Gbtc
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

using System;
using System.Collections.Generic;
using System.Linq;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        public string Package { get; }

        public string PackageImport { get; }

        public string PackageUsing { get; private set; }

        public string PackageNamespace { get; private set; }

        public string[] PackageNamespaces { get; private set; }

        public override void EnterPackageClause(GolangParser.PackageClauseContext context)
        {
            // Go package clause is the first keyword encountered - cache details that
            // will be written out after imports. C# import statements (i.e., usings)
            // typically occur before namespace and class definitions
            string[] paths = PackageImport.Split('/').Select(SanitizedIdentifier).ToArray();
            string packageNamespace = $"{RootNamespace}.{string.Join(".", paths)}";

            PackageUsing = $"{Package} = {packageNamespace}{ClassSuffix}";
            PackageNamespace = packageNamespace.Substring(0, packageNamespace.LastIndexOf('.'));

            // Track file name associated with package
            AddFileToPackage(Package, TargetFileName, PackageNamespace);

            // Define namespaces
            List<string> packageNamespaces = new List<string> { RootNamespace };

            if (paths.Length > 1)
            {
                packageNamespaces.AddRange(paths);
                packageNamespaces.RemoveAt(packageNamespaces.Count - 1);
            }

            PackageNamespaces = packageNamespaces.ToArray();

            string headerLevelComments = CheckForCommentsLeft(context);
            string packageLevelComments = CheckForCommentsRight(context);

            if (!string.IsNullOrWhiteSpace(headerLevelComments))
            {
                m_targetFile.Append(headerLevelComments);

                if (!EndsWithLineFeed(headerLevelComments))
                    m_targetFile.AppendLine();
            }

            m_targetFile.AppendLine($"// package {Package} -- go2cs converted at {DateTime.UtcNow:yyyy MMMM dd HH:mm:ss} UTC");

            if (!PackageImport.Equals("main"))
                m_targetFile.AppendLine($"// import \"{PackageImport}\" ==> using {PackageUsing}");

            m_targetFile.AppendLine($"// Original source: {SourceFileName}");
            m_targetFile.AppendLine();

            if (!string.IsNullOrWhiteSpace(packageLevelComments))
                m_targetFile.Append(packageLevelComments.TrimStart());

            // Add commonly required using statements
            RequiredUsings.Add("static go.builtin");
        }
    }
}

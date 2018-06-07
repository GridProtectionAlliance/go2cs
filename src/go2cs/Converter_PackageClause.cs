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
using System.IO;
using System.Linq;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        private string m_package;
        private string m_packageImport;
        private string m_packageUsing;
        private string m_packageNamespace;
        private string[] m_packageNamespaces;
        private readonly HashSet<string> m_requiredUsings = new HashSet<string>(StringComparer.Ordinal);

        public string Package => m_package;

        public string PackageImport => m_packageImport;

        public string PackageUsing => m_packageUsing;

        public override void EnterPackageClause(GolangParser.PackageClauseContext context)
        {
            // Go package clause is the first keyword encountered - cache details and comments
            // that will be written out after imports. C# import statements (i.e., usings)
            // typically occur before namespace and class definitions
            m_package = SanitizedIdentifier(context.IDENTIFIER().GetText());

            if (m_package.Equals("main"))
            {
                m_packageImport = m_package;
            }
            else
            {
                // Define package import path
                m_packageImport = Path.GetDirectoryName(SourceFileName) ?? m_package;
                m_packageImport = m_packageImport.Replace(GoRoot, "");
                m_packageImport = m_packageImport.Replace(GoPath, "");

                while (m_packageImport.StartsWith(Path.DirectorySeparatorChar.ToString()) || m_packageImport.StartsWith(Path.AltDirectorySeparatorChar.ToString()))
                    m_packageImport = m_packageImport.Substring(1);

                while (m_packageImport.EndsWith(Path.DirectorySeparatorChar.ToString()) || m_packageImport.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                    m_packageImport = m_packageImport.Substring(0, m_packageImport.Length - 1);

                int lastSlash;

                if (Path.IsPathRooted(m_packageImport))
                {
                    // File converted was outside %GOPATH% and %GOROOT%
                    lastSlash = m_packageImport.LastIndexOf('\\');

                    if (lastSlash > -1)
                        m_packageImport = $"{m_packageImport.Substring(lastSlash + 1)}";
                }

                m_packageImport = $"{m_packageImport.Replace('\\', '/')}";

                lastSlash = m_packageImport.LastIndexOf('/');
                string package = SanitizedIdentifier(lastSlash > -1 ? m_packageImport.Substring(lastSlash + 1) : m_packageImport);

                if (!package.Equals(m_package))
                {
                    AddWarning(context, $"Defined package clause \"{m_package}\" does not match file path \"{SourceFileName}\"");
                    m_packageImport = lastSlash > -1 ? $"{m_packageImport.Substring(0, lastSlash)}.{m_package}" : m_package;
                }
            }

            string[] paths = m_packageImport.Split('/').Select(SanitizedIdentifier).ToArray();
            string packageNamespace = $"{RootNamespace}.{string.Join(".", paths)}";

            m_packageUsing = $"{m_package} = {packageNamespace}{ClassSuffix}";
            m_packageNamespace = packageNamespace.Substring(0, packageNamespace.LastIndexOf('.'));

            // Track file name associated with package
            AddFileToPackage(m_package, TargetFileName, m_packageNamespace);

            // Define namespaces
            List<string> packageNamespaces = new List<string> { RootNamespace };

            if (paths.Length > 1)
            {
                packageNamespaces.AddRange(paths);
                packageNamespaces.RemoveAt(packageNamespaces.Count - 1);
            }

            m_packageNamespaces = packageNamespaces.ToArray();

            string headerLevelComments = CheckForCommentsLeft(context);
            string packageLevelComments = CheckForCommentsRight(context).TrimStart();

            if (!string.IsNullOrWhiteSpace(headerLevelComments))
            {
                m_targetFile.AppendLine(headerLevelComments);

                if (!headerLevelComments.EndsWith("\r") && !headerLevelComments.EndsWith("\n"))
                    m_targetFile.AppendLine();
            }

            m_targetFile.AppendLine($"// package {m_package} -- go2cs converted at {DateTime.UtcNow:yyyy MMMM dd HH:mm:ss} UTC");

            if (!m_packageImport.Equals("main"))
                m_targetFile.AppendLine($"// import \"{m_packageImport}\" ==> using {m_packageUsing}");

            m_targetFile.AppendLine($"// Original source: {SourceFileName}");
            m_targetFile.AppendLine();

            if (!string.IsNullOrWhiteSpace(packageLevelComments))
                m_targetFile.Append(packageLevelComments);

            // Add commonly required using statements
            m_requiredUsings.Add("static go.BuiltInFunctions");
        }
    }
}

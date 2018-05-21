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

namespace go2cs
{
    public partial class Converter
    {
        private string m_package;
        private string m_packageImport;
        private string m_packageUsing;
        private string[] m_packageNamespaces;
        private string m_headerLevelComments;
        private string m_packageLevelComments;

        public string Package => m_package;

        public string PackageImport => m_packageImport;

        public string PackageUsing => m_packageUsing;

        public override void EnterPackageClause(GolangParser.PackageClauseContext context)
        {
            // Go package clause is the first keyword encountered - cache details and comments
            // that will be written out after imports. C# import statements (i.e., usings)
            // typically occur before namespace and class definitions
            m_package = context.IDENTIFIER().GetText();

            if (m_package.Equals("main"))
            {
                m_packageImport = m_package;
            }
            else
            {
                // Define package import path
                m_packageImport = Path.GetDirectoryName(m_sourceFileName) ?? m_package;
                m_packageImport = m_packageImport.Replace(s_goRoot, "");
                m_packageImport = m_packageImport.Replace(s_goPath, "");

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
                string package = lastSlash > -1 ? m_packageImport.Substring(lastSlash + 1) : m_packageImport;

                if (!package.Equals(m_package))
                    AddWarning(context, $"Defined package clause \"{m_package}\" does not match file path \"{m_sourceFileName}\"");
            }

            m_packageUsing = $"{m_package} = {RootNamespace}.{m_packageImport.Replace('/', '.')}{ClassSuffix}";

            // Define namespaces
            List<string> packageNamespaces = new List<string>();

            packageNamespaces.Add(RootNamespace);

            string[] namespaces = m_packageImport.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            if (namespaces.Length > 1)
            {
                packageNamespaces.AddRange(namespaces);
                packageNamespaces.RemoveAt(packageNamespaces.Count - 1);
            }

            m_packageNamespaces = packageNamespaces.ToArray();

            m_headerLevelComments = CheckForCommentsLeft(context);
            m_packageLevelComments = CheckForCommentsRight(context).TrimStart();
        }
    }
}

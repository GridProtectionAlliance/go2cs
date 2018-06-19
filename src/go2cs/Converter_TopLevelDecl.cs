//******************************************************************************************************
//  Converter_TopLevelDecl.cs - Gbtc
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
using System.Text;

namespace go2cs
{
    public partial class Converter
    {
        private bool m_firstTopLevelDeclaration = true;
        private string m_nextDeclComments;
        private string m_namespaceHeader;
        private string m_namespaceFooter;

        // TopLevelDecl is visited once per each encountered Declaration, FunctionDecl or MethodDecl
        public override void EnterTopLevelDecl(GolangParser.TopLevelDeclContext context)
        {
            m_indentLevel++;

            if (m_firstTopLevelDeclaration)
            {
                StringBuilder namespaceHeader = new StringBuilder();

                // Begin namespaces
                for (int i = 0; i < PackageNamespaces.Length; i++)
                {
                    namespaceHeader.Append($"namespace {PackageNamespaces[i]}");
                    namespaceHeader.Append(i == PackageNamespaces.Length - 1 ? $"{Environment.NewLine}{{" : $" {{{Environment.NewLine}");
                }

                m_namespaceHeader = namespaceHeader.ToString();
                m_namespaceFooter = new string('}', PackageNamespaces.Length);

                // Mark end of using statements so that other usings and type aliases can be added later
                m_targetFile.AppendLine(UsingsMarker);

                m_targetFile.AppendLine(m_namespaceHeader);

                // Begin class
                m_targetFile.AppendLine($"{Spacing()}public static partial class {Package}{ClassSuffix}");
                m_targetFile.AppendLine($"{Spacing()}{{");

                // Check for comments after initial declaration
                string initialDeclComments = CheckForCommentsLeft(context);

                // Write any initial declaration comments created during Converter_ImportDecl visit 
                if (!string.IsNullOrWhiteSpace(initialDeclComments))
                    m_targetFile.Append(FixForwardSpacing(initialDeclComments, 1));

                // End class and namespace "}" occur as a last step in Convert() method
                m_indentLevel++;
            }
        }

        public override void ExitTopLevelDecl(GolangParser.TopLevelDeclContext context)
        {
            // There can be only one... first top level declaration
            if (m_firstTopLevelDeclaration)
                m_firstTopLevelDeclaration = false;

            m_nextDeclComments = CheckForCommentsRight(context);
            m_indentLevel--;
        }
    }
}
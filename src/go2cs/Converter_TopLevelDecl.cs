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

namespace go2cs
{
    public partial class Converter
    {
        private bool m_topLevelDeclaration = true;
        private string m_nextDeclComments;

        // TopLevelDecl is visited once per each encountered Declaration, FunctionDecl or MethodDecl
        public override void EnterTopLevelDecl(GolangParser.TopLevelDeclContext context)
        {
            m_indentLevel++;

            if (m_topLevelDeclaration)
            {
                // Begin namespaces
                for (int i = 0; i < m_packageNamespaces.Length; i++)
                {
                    m_targetFile.Append($"namespace {m_packageNamespaces[i]}");
                    m_targetFile.AppendLine(i == m_packageNamespaces.Length - 1 ? $"{Environment.NewLine}{{" : " {");
                }

                string scope = m_package.Equals("main") ? "private" : "public";

                // Begin class
                m_targetFile.AppendLine($"{Spacing()}{scope} static partial class {m_package}{ClassSuffix}");
                m_targetFile.AppendLine($"{Spacing()}{{");

                // Write any initial declaration comments created during Converter_ImportDecl visit 
                if (!string.IsNullOrWhiteSpace(m_initialDeclComments))
                    m_targetFile.Append(FixForwardSpacing(m_initialDeclComments, 1));

                // End class and namespace "}" occur as a last step in Convert() method
                m_indentLevel++;
            }
        }

        public override void ExitTopLevelDecl(GolangParser.TopLevelDeclContext context)
        {
            // There can be only one... top level declaration
            if (m_topLevelDeclaration)
                m_topLevelDeclaration = false;

            m_nextDeclComments = CheckForCommentsRight(context);
            m_indentLevel--;
        }
    }
}
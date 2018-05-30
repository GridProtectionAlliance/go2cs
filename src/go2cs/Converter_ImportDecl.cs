//******************************************************************************************************
//  Converter_ImportDecl.cs - Gbtc
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
        private readonly HashSet<string> m_requiredUsings = new HashSet<string>(StringComparer.Ordinal);
        private string m_initialDeclComments;

        public override void EnterImportDecl(GolangParser.ImportDeclContext context)
        {
            if (!string.IsNullOrWhiteSpace(m_headerLevelComments))
            {
                m_targetFile.AppendLine(m_headerLevelComments);

                if (!m_headerLevelComments.EndsWith("\r") && !m_headerLevelComments.EndsWith("\n"))
                    m_targetFile.AppendLine();
            }

            m_targetFile.AppendLine($"// package {m_package} -- go2cs converted at {DateTime.UtcNow:yyyy MMMM dd HH:mm:ss} UTC");

            if (!m_packageImport.Equals("main"))
                m_targetFile.AppendLine($"// import \"{m_packageImport}\" ==> using {m_packageUsing}");

            m_targetFile.AppendLine($"// Original source: {m_sourceFileName}");
            m_targetFile.AppendLine();

            if (!string.IsNullOrWhiteSpace(m_packageLevelComments))
                m_targetFile.Append(m_packageLevelComments);
        }

        public override void ExitImportDecl(GolangParser.ImportDeclContext context)
        {
            // Add commonly required using statements
            m_requiredUsings.Add("static go.BuiltInFunctions");
            m_requiredUsings.Add("System");

            // Mark end of using statements so that other usings and type aliases can be added later
            m_targetFile.AppendLine(UsingsMarker);

            // Check for comments after initial declaration
            m_initialDeclComments = CheckForCommentsRight(context);
        }

        public override void EnterImportPath(GolangParser.ImportPathContext context)
        {
            // Remove quotes from package name
            string packagePath = RemoveSurrounding(ToStringLiteral(context.STRING_LIT().GetText()));

            int lastSlash = packagePath.LastIndexOf('/');

            string packageName = SanitizedIdentifier(lastSlash > -1 ? packagePath.Substring(lastSlash + 1) : packagePath);

            // Add package to import queue
            s_importQueue.Add(packagePath);

            IEnumerable<string> paths = packagePath.Split('/').Select(SanitizedIdentifier);

            m_targetFile.Append($"using {packageName} = {RootNamespace}.{string.Join(".", paths)}{ClassSuffix};");

            m_targetFile.Append(CheckForCommentsRight(context));

            if (!m_wroteCommentWithLineFeed)
                m_targetFile.AppendLine();
        }
    }
}

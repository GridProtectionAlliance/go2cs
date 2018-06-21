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

using System.Linq;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        private bool m_firstImportSpec = true;
        private string m_lastImportSpecComment;
        private string m_lastEolImportSpecComment;

        public override void EnterImportSpec(GolangParser.ImportSpecContext context)
        {
            // Base class parses current import package path
            base.EnterImportSpec(context);

            string alternateName = SanitizedIdentifier(context.IDENTIFIER()?.GetText());
            bool useStatic = (object)alternateName == null && context.ChildCount > 1 && context.GetChild(0).GetText().Equals(".");

            int lastSlash = CurrentImportPath.LastIndexOf('/');
            string packageName = SanitizedIdentifier(lastSlash > -1 ? CurrentImportPath.Substring(lastSlash + 1) : CurrentImportPath);

            string targetUsing = $"{RootNamespace}.{string.Join(".", CurrentImportPath.Split('/').Select(SanitizedIdentifier))}{ClassSuffix}";

            if (!m_firstImportSpec)
            {
                if (!string.IsNullOrEmpty(m_lastImportSpecComment))
                    m_targetFile.Append(m_lastImportSpecComment);

                if (!m_wroteCommentWithLineFeed)
                    m_targetFile.AppendLine();
            }

            if (useStatic)
                m_targetFile.Append($"using static {targetUsing};");
            else if (alternateName?.Equals("_") ?? false)
                m_targetFile.Append($"using _{packageName}_ = {targetUsing};");
            else
                m_targetFile.Append($"using {alternateName ?? packageName} = {targetUsing};");

            m_lastImportSpecComment = CheckForCommentsRight(context);
            m_lastEolImportSpecComment = CheckForEndOfLineComment(context);

            // Check for comments on lines in-between imports
            if (!m_lastImportSpecComment.Equals(m_lastEolImportSpecComment))
            {
                if (m_lastImportSpecComment.StartsWith(m_lastEolImportSpecComment))
                    m_lastImportSpecComment = m_lastImportSpecComment.Substring(m_lastEolImportSpecComment.Length);
            }

            if (!string.IsNullOrEmpty(m_lastEolImportSpecComment))
                m_targetFile.Append(m_lastEolImportSpecComment);
        }

        public override void ExitImportSpec(GolangParser.ImportSpecContext context)
        {
            // There can be only one... first import spec
            if (m_firstImportSpec)
                m_firstImportSpec = false;
        }
    }
}
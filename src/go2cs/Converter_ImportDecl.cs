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

using Antlr4.Runtime.Misc;
using go2cs.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        private bool m_firstImportSpec = true;
        private string m_lastImportSpecComment;
        private string m_lastEolImportSpecComment = "";
        private readonly HashSet<string> m_usingStatements = new HashSet<string>(StringComparer.Ordinal);

        public override void EnterImportDecl(GolangParser.ImportDeclContext context)
        {
            m_usingStatements.UnionWith(RequiredUsings.Select(usingType => $"using {usingType};"));
        }

        public override void EnterImportSpec(GolangParser.ImportSpecContext context)
        {
            // Base class parses current import package path
            base.EnterImportSpec(context);

            if (!m_firstImportSpec)
            {
                if (!string.IsNullOrEmpty(m_lastImportSpecComment))
                    m_targetFile.Append(m_lastImportSpecComment);

                if (!WroteCommentWithLineFeed)
                    m_targetFile.AppendLine();
            }

            KeyValuePair<string, (string targetImport, string targetUsing)> importAlias = ImportAliases.FirstOrDefault(import => import.Value.targetImport.Equals(CurrentImportPath));

            if (!string.IsNullOrEmpty(importAlias.Key))
            {
                string alias = importAlias.Key;
                string targetUsing = importAlias.Value.targetUsing;
                string targetImport = importAlias.Value.targetImport;
                string usingStatement;

                if (alias.StartsWith("static ", StringComparison.Ordinal))
                    usingStatement = $"using {alias};";
                else
                    usingStatement = $"using {alias} = {targetUsing};";

                m_targetFile.Append(usingStatement);
                m_usingStatements.Add(usingStatement);

                FolderMetadata metadata = LoadImportMetadata(Options, targetImport, out string warning);

                if ((object)metadata == null)
                    AddWarning(context, warning);
                else
                    ImportMetadata[targetImport] = metadata;
            }
            else
            {
                m_targetFile.Append($"//using {RootNamespace}.{string.Join(".", CurrentImportPath.Split('/').Select(SanitizedIdentifier))}{ClassSuffix}; // ?? metadata not found");
                AddWarning(context, $"Could not find import metadata for \"{CurrentImportPath}\"");
            }

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
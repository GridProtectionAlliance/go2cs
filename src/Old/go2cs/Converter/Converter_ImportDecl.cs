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

using go2cs.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using static go2cs.Common;

namespace go2cs;

public partial class Converter
{
    private bool m_firstImportDecl = true;
    private bool m_firstImportSpec = true;
    private string m_postImportDeclComment = string.Empty;
    private string m_lastImportDeclComment;
    private string m_lastImportSpecComment;
    private string m_lastEolImportSpecComment = string.Empty;
    private readonly HashSet<string> m_usingStatements = new(StringComparer.Ordinal);

    public override void EnterImportDecl(GoParser.ImportDeclContext context)
    {
        if (!string.IsNullOrWhiteSpace(m_packageLevelComments))
        {
            m_targetFile.Append(m_packageLevelComments.TrimStart());

            if (!EndsWithLineFeed(m_packageLevelComments))
                m_targetFile.AppendLine();
        }

        if (m_firstImportDecl && context.L_PAREN() is null)
        {
            if (!EndsWithDuplicateLineFeed(m_targetFile.ToString()))
                m_targetFile.AppendLine();
        }

        m_usingStatements.UnionWith(RequiredUsings.Select(usingType => $"using {usingType};"));

        string importDeclComment = CheckForCommentsRight(context.IMPORT());

        if (!string.IsNullOrEmpty(importDeclComment))
        {
            if (!EndsWithLineFeed(m_targetFile.ToString()))
                m_targetFile.AppendLine();

            m_targetFile.Append(importDeclComment.TrimStart());
        }

        if (context.L_PAREN() is not null)
        {
            importDeclComment = CheckForCommentsRight(context.L_PAREN());

            if (!string.IsNullOrEmpty(importDeclComment))
                m_targetFile.Append(importDeclComment);
        }

        if (context.R_PAREN() is not null)
        {
            importDeclComment = CheckForCommentsLeft(context.R_PAREN());

            if (!string.IsNullOrEmpty(importDeclComment))
                m_postImportDeclComment = importDeclComment;

            importDeclComment = CheckForCommentsRight(context.R_PAREN());

            if (!string.IsNullOrEmpty(importDeclComment))
            {
                m_lastImportDeclComment = importDeclComment;
                m_postImportDeclComment += importDeclComment;
            }
        }

        m_packageLevelComments = m_lastImportSpecComment = m_lastEolImportSpecComment = string.Empty;
    }

    public override void EnterImportSpec(GoParser.ImportSpecContext context)
    {
        // Base class parses current import package path
        base.EnterImportSpec(context);

        if (!m_firstImportSpec)
        {
            if (!string.IsNullOrEmpty(m_lastImportSpecComment))
            {
                m_targetFile.Append(m_lastImportSpecComment);

                if (!EndsWithLineFeed(m_lastImportSpecComment))
                    m_targetFile.AppendLine();
            }
            else if (!WroteLineFeed)
            {
                m_targetFile.AppendLine();
            }
        }

        (string alias, (string targetImport, string targetUsing) value) = ImportAliases.FirstOrDefault(import => import.Value.targetImport.Equals(CurrentImportPath));

        if (!string.IsNullOrEmpty(alias))
        {
            string targetUsing = value.targetUsing;
            string targetImport = value.targetImport;
            string usingStatement, fullyQualifiedUsingStatement;

            if (targetUsing.StartsWith($"{RootNamespace}."))
                targetUsing = targetUsing[(RootNamespace.Length + 1)..];

            //                    0123456
            if (alias.StartsWith("static ", StringComparison.Ordinal))
            {
                string staticUsing = alias[7..];
                
                if (staticUsing.StartsWith($"{RootNamespace}."))
                    staticUsing = staticUsing[(RootNamespace.Length + 1)..];

                usingStatement = $"using static {staticUsing};";
                fullyQualifiedUsingStatement = $"using {alias};";
            }
            else
            {
                usingStatement = $"using {alias} = {targetUsing};";
                fullyQualifiedUsingStatement = $"using {alias} = {value.targetUsing};";
            }

            m_targetFile.Append(usingStatement);
            m_usingStatements.Add(fullyQualifiedUsingStatement);

            FolderMetadata metadata = LoadImportMetadata(Options, targetImport, out string warning);

            if (metadata is null)
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
                m_lastImportSpecComment = m_lastImportSpecComment[m_lastEolImportSpecComment.Length..];
        }

        if (!string.IsNullOrEmpty(m_lastEolImportSpecComment))
            m_targetFile.Append(m_lastEolImportSpecComment);
    }

    public override void ExitImportSpec(GoParser.ImportSpecContext context)
    {
        // There can be only one... first import spec
        if (m_firstImportSpec)
            m_firstImportSpec = false;
    }

    public override void ExitImportDecl(GoParser.ImportDeclContext context)
    {
        // There can be only one... first import decl
        if (m_firstImportDecl)
            m_firstImportDecl = false;

        if (!string.IsNullOrEmpty(m_postImportDeclComment))
        {
            if (context.R_PAREN() is null || m_postImportDeclComment.Trim().Length > 0)
                m_targetFile.Append(m_postImportDeclComment);
        }

        m_postImportDeclComment = string.Empty;
    }
}

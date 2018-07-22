//******************************************************************************************************
//  Converter_VarDecl.cs - Gbtc
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
//  07/12/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using go2cs.Metadata;
using System;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        private bool m_firstVarSpec;

        public override void EnterVarDecl(GolangParser.VarDeclContext context)
        {
            m_firstVarSpec = !m_inFunction;
        }

        public override void ExitVarSpec(GolangParser.VarSpecContext context)
        {
            // varSpec
            //     : identifierList(type('=' expressionList) ? | '=' expressionList)

            if (m_firstVarSpec)
            {
                m_firstVarSpec = false;

                string comments = CheckForCommentsLeft(context, preserveLineFeeds: m_inFunction);

                if (!string.IsNullOrEmpty(comments))
                    m_targetFile.Append(FixForwardSpacing(comments));
            }

            if (!Identifiers.TryGetValue(context.identifierList(), out string[] identifiers))
            {
                AddWarning(context, $"No identifiers specified in var specification expression: {context.GetText()}");
                return;
            }

            ExpressionLists.TryGetValue(context.expressionList(), out string[] expressions);

            if ((object)expressions != null && identifiers.Length != expressions.Length)
            {
                AddWarning(context, $"Encountered count mismatch for identifiers and expressions in var specification expression: {context.GetText()}");
                return;
            }

            Types.TryGetValue(context.type(), out TypeInfo typeInfo);

            string type = typeInfo?.TypeName ?? "var";
            int length = Math.Min(identifiers.Length, expressions?.Length ?? int.MaxValue);

            for (int i = 0; i < length; i++)
            {
                string identifier = SanitizedIdentifier(identifiers[i]);
                string expression = expressions?[i];

                m_targetFile.Append($"{Spacing()}");

                if (!m_inFunction)
                {
                    m_targetFile.Append(char.IsUpper(identifier[0]) ? "public " : "private ");

                    // TODO: Using dynamic type here is not ideal - need to use an expression type evaluator
                    if (type.Equals("var", StringComparison.Ordinal))
                        type = "dynamic";
                }

                m_targetFile.Append($"{type} {identifier}");

                if ((object)expression != null)
                    m_targetFile.Append($" = {expression}");

                // Since multiple specifications can be on one line, only check for comments after last specification
                if (i < length - 1)
                    m_targetFile.AppendLine(";");
                else
                    m_targetFile.Append($";{CheckForBodyCommentsRight(context)}");
            }
        }
    }
}
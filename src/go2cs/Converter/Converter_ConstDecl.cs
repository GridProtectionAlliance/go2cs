//******************************************************************************************************
//  Converter_ConstDecl.cs - Gbtc
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
//  05/04/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using go2cs.Metadata;
using System;

namespace go2cs
{
    public partial class Converter
    {
        private int m_iota;
        private int m_constIdentifierCount;
        private bool m_constMultipleDeclaration;

        public override void EnterConstDecl(GoParser.ConstDeclContext context)
        {
            // constDecl
            //     : 'const' ( constSpec | '(' ( constSpec eos )* ')' )

            m_constIdentifierCount = 0;
            m_constMultipleDeclaration = context.children.Count > 2;
            m_iota = 0;
        }

        public override void ExitConstDecl(GoParser.ConstDeclContext context)
        {
            // constDecl
            //     : 'const' ( constSpec | '(' ( constSpec eos )* ')' )

            if (m_constMultipleDeclaration && EndsWithLineFeed(m_targetFile.ToString()))
            {
                string removedLineFeed = RemoveLastLineFeed(m_targetFile.ToString());
                m_targetFile.Clear();
                m_targetFile.Append(removedLineFeed);
            }

            m_targetFile.Append(CheckForCommentsRight(context));
        }

        public override void ExitConstSpec(GoParser.ConstSpecContext context)
        {
            // constSpec
            //     : identifierList ( type ? '=' expressionList ) ?

            if (m_constIdentifierCount == 0 && m_constMultipleDeclaration)
                m_targetFile.Append(RemoveFirstLineFeed(CheckForCommentsLeft(context)));

            if (!Identifiers.TryGetValue(context.identifierList(), out string[] identifiers))
            {
                AddWarning(context, $"No identifiers specified in constant expression: {context.GetText()}");
                return;
            }

            ExpressionLists.TryGetValue(context.expressionList(), out ExpressionInfo[] expressions);

            if (!(expressions is null) && identifiers.Length != expressions.Length)
            {
                AddWarning(context, $"Encountered identifier to expression count mismatch in constant expression: {context.GetText()}");
                return;
            }

            Types.TryGetValue(context.type_(), out TypeInfo typeInfo);

            string type = typeInfo?.TypeName;
            int length = Math.Min(identifiers.Length, expressions?.Length ?? int.MaxValue);

            for (int i = 0; i < identifiers.Length; i++)
            {
                string identifier = identifiers[i];
                string expression = expressions?[i].Text ?? $"{m_iota++}";
                string typeName = type ?? expressions?[i].Type.TypeName ?? "var";
                string castAs = $"({typeName})";

                if (type?.Equals(expressions?[i].Type.TypeName) ?? false)
                    castAs = "";

                if (InFunction)
                    m_targetFile.Append($"{Spacing()}const {typeName} {identifier} = {castAs}{expression};");
                else
                    m_targetFile.Append($"{Spacing()}{(char.IsUpper(identifier[0]) ? "public" : "private")} static readonly {typeName} {identifier} = {castAs}{expression};");

                // Since multiple specifications can be on one line, only check for comments after last specification
                if (i < length - 1)
                    m_targetFile.AppendLine();
                else
                    m_targetFile.Append(CheckForCommentsRight(context));
            }

            m_constIdentifierCount++;
        }
    }
}

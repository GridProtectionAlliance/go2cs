//******************************************************************************************************
//  Converter_Statement.cs - Gbtc
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
using System.Collections.Generic;
using System.Text;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        public const string TypeSwitchExpressionMarker = ">>MARKER:TYPESWITCH_LEVEL_{0}<<";
        public const string TypeSwitchCaseTypeMarker = ">>MARKER:TYPESWITCHCASE_LEVEL_{0}<<";
        public const string TypeSwitchStatementMarker = ">>MARKER:TYPESWITCHSTATEMENT_LEVEL_{0}<<";

        private readonly Stack<StringBuilder> m_typeSwitchDefaultCase = new Stack<StringBuilder>();
        private int m_typeSwitchExpressionLevel;

        public override void EnterTypeSwitchStmt(GoParser.TypeSwitchStmtContext context)
        {
            // typeSwitchStmt
            //     : 'switch' (simpleStmt ';') ? typeSwitchGuard '{' typeCaseClause * '}'

            // typeSwitchGuard
            //     : ( IDENTIFIER ':=' )? primaryExpr '.' '(' 'type' ')'

            m_typeSwitchExpressionLevel++;

            if (!(context.simpleStmt() is null) && context.simpleStmt().emptyStmt() is null)
            {
                if (!(context.simpleStmt().shortVarDecl() is null))
                {
                    // Any declared variable will be scoped to switch statement, so create a sub-block for it
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    IndentLevel++;
                }

                m_targetFile.Append(string.Format(TypeSwitchStatementMarker, m_typeSwitchExpressionLevel));
            }

            m_targetFile.Append($"{Spacing()}switch ({string.Format(TypeSwitchExpressionMarker, m_typeSwitchExpressionLevel)}){Environment.NewLine}{Spacing()}{{");
            IndentLevel++;

            m_typeSwitchDefaultCase.Push(new StringBuilder());
        }

        public override void EnterTypeCaseClause(GoParser.TypeCaseClauseContext context)
        {
            // typeCaseClause
            //     : typeSwitchCase ':' statementList

            // typeSwitchCase
            //     : 'case' typeList | 'default'

            // typeList
            //     : type ( ',' type )*

            if (context.typeSwitchCase().typeList() is null)
            {
                GoParser.TypeSwitchStmtContext parent = context.Parent as GoParser.TypeSwitchStmtContext;
                string identifier = parent == null ? "" : SanitizedIdentifier(parent.typeSwitchGuard().IDENTIFIER().GetText());

                m_typeSwitchDefaultCase.Peek().Append($"{Environment.NewLine}{Spacing()}default:{Environment.NewLine}{Spacing()}{{{Environment.NewLine}");

                if (!string.IsNullOrEmpty(identifier))
                    m_typeSwitchDefaultCase.Peek().Append($"{Spacing(1)}var {identifier} = {string.Format(TypeSwitchExpressionMarker, m_typeSwitchExpressionLevel)};{Environment.NewLine}");
            }
            else
                m_targetFile.Append($"{Environment.NewLine}{Spacing()}case {string.Format(TypeSwitchCaseTypeMarker, m_typeSwitchExpressionLevel)}{Environment.NewLine}");
            
            IndentLevel++;

            PushBlock();
        }

        public override void ExitTypeCaseClause(GoParser.TypeCaseClauseContext context)
        {
            // typeCaseClause
            //     : typeSwitchCase ':' statementList

            // typeSwitchCase
            //     : 'case' typeList | 'default'

            // typeList
            //     : type ( ',' type )*

            IndentLevel--;

            if (context.typeSwitchCase().typeList() is null)
            {
                m_typeSwitchDefaultCase.Peek().Append($"{PopBlock(false)}{Spacing(1)}break;{Environment.NewLine}{Spacing()}}}");
            }
            else
            {
                string caseBlock = $"{PopBlock(false)}{Spacing(1)}break;";
                m_targetFile.Append(caseBlock);

                GoParser.TypeSwitchStmtContext parent = context.Parent as GoParser.TypeSwitchStmtContext;
                string identifier = parent == null ? "_" : SanitizedIdentifier(parent.typeSwitchGuard().IDENTIFIER().GetText());

                GoParser.TypeListContext typeList = context.typeSwitchCase().typeList();
                StringBuilder caseTypeExpressions = new StringBuilder();
                HashSet<string> typeNames = new HashSet<string>();

                for (int i = 0; i < typeList.type_().Length; i++)
                {
                    if (Types.TryGetValue(typeList.type_(i), out TypeInfo typeInfo))
                    {
                        string typeName = typeInfo.TypeName;

                        if (typeNames.Add(typeName))
                        {
                            string caseExpression = i > 0 ? $"{Environment.NewLine}{caseBlock}{Environment.NewLine}{Spacing()}case " : "";

                            if (typeName.Equals("nil", StringComparison.Ordinal))
                                caseTypeExpressions.Append($"{caseExpression}null:");
                            else
                                caseTypeExpressions.Append($"{caseExpression}{typeName} {identifier}:");
                        }
                        else
                        {
                            AddWarning(typeList, $"Skipped duplicate type info (from C# perspective) for type switch case statement: {typeList.GetText()}");
                        }
                    }
                    else
                    {
                        AddWarning(typeList, $"Failed to find type info for type switch case statement: {typeList.GetText()}");
                    }
                }

                // Replace type switch case type marker
                m_targetFile.Replace(string.Format(TypeSwitchCaseTypeMarker, m_typeSwitchExpressionLevel), caseTypeExpressions.ToString());
            }
        }

        public override void ExitTypeSwitchStmt(GoParser.TypeSwitchStmtContext context)
        {
            // typeSwitchStmt
            //     : 'switch'(simpleStmt ';') ? typeSwitchGuard '{' typeCaseClause * '}'

            // typeSwitchGuard
            //     : ( IDENTIFIER ':=' )? primaryExpr '.' '(' 'type' ')'

            // Default case always needs to be last case clause in SwitchExpression - Go allows its declaration anywhere
            m_targetFile.Append($"{m_typeSwitchDefaultCase.Pop()}{CheckForCommentsRight(context)}");

            if (PrimaryExpressions.TryGetValue(context.typeSwitchGuard().primaryExpr(), out ExpressionInfo expression))
            {
                // Replace type switch expression marker
                m_targetFile.Replace(string.Format(TypeSwitchExpressionMarker, m_typeSwitchExpressionLevel), expression.Text);
            }
            else
            {
                AddWarning(context, $"Failed to find primary expression for type switch statement: {context.typeSwitchGuard().GetText()}");
            }

            if (!(context.simpleStmt() is null) && context.simpleStmt().emptyStmt() is null)
            {
                // TODO: Handle case where short val declaration re-declares a variable already defined in current scope - C# does not allow this. One option: cache current variable value and restore below
                if (m_simpleStatements.TryGetValue(context.simpleStmt(), out string statement))
                    m_targetFile.Replace(string.Format(TypeSwitchStatementMarker, m_typeSwitchExpressionLevel), statement + Environment.NewLine);
                else
                    AddWarning(context, $"Failed to find simple statement for type switch statement: {context.simpleStmt().GetText()}");

                // Close any locally scoped declared variable sub-block
                if (!(context.simpleStmt().shortVarDecl() is null))
                {
                    IndentLevel--;
                    m_targetFile.AppendLine();
                    m_targetFile.Append($"{Spacing()}}}");
                }
            }

            IndentLevel--;
            m_targetFile.Append($"{Spacing()}}}");
            m_typeSwitchExpressionLevel--;
        }
    }
}

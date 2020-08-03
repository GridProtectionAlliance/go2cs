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
using System.Linq;

namespace go2cs
{
    public partial class Converter
    {
        private class ExprCaseStatement
        {
            public ExpressionInfo[] expressionList;
            public string leftComments;
            public string block;
            public bool hasFallthrough;

            public bool allConst => expressionList?.All(expr => expr?.Type?.IsConst ?? false) ?? false;
        }

        private class ExprSwitchStatement
        {
            public readonly ParseTreeValues<ExprCaseStatement> caseStatements = new ParseTreeValues<ExprCaseStatement>();
            public ExprCaseStatement defaultCase;

            public bool allConst => caseStatements?.Select(stmt => stmt.Value).All(stmt => stmt.allConst) ?? false;
            public bool anyFallthroughs => caseStatements?.Select(stmt => stmt.Value).Any(stmt => stmt.hasFallthrough) ?? false;
        }

        public const string ExprSwitchStatementMarker = ">>MARKER:EXPRSWITCHSTATEMENT_LEVEL_{0}<<";

        private readonly Stack<ExprSwitchStatement> m_exprSwitchStatements = new Stack<ExprSwitchStatement>();
        private int m_exprSwitchExpressionLevel;
        private int m_exprSwitchBreakCounter;

        public override void EnterExprSwitchStmt(GoParser.ExprSwitchStmtContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            m_exprSwitchExpressionLevel++;

            if (!(context.simpleStmt() is null) && context.simpleStmt().emptyStmt() is null)
            {
                if (!(context.simpleStmt().shortVarDecl() is null))
                {
                    // Any declared variable will be scoped to switch statement, so create a sub-block for it
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    IndentLevel++;
                    
                    // Handle storing of current values of any redeclared variables
                    m_targetFile.Append(OpenRedeclaredVariableBlock(context.simpleStmt().shortVarDecl(), m_exprSwitchExpressionLevel));
                }

                m_targetFile.Append(string.Format(ExprSwitchStatementMarker, m_exprSwitchExpressionLevel));
            }

            m_exprSwitchStatements.Push(new ExprSwitchStatement());
        }

        public override void EnterExprCaseClause(GoParser.ExprCaseClauseContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            // exprCaseClause
            //     : exprSwitchCase ':' statementList

            // exprSwitchCase
            //     : 'case' expressionList | 'default'

            ExprSwitchStatement exprSwitchStatement = m_exprSwitchStatements.Peek();

            if (context.exprSwitchCase().expressionList() is null)
            {
                // Handle default case
                exprSwitchStatement.defaultCase = new ExprCaseStatement
                {
                    leftComments = CheckForCommentsLeft(context.statementList(), 1)
                };
            }
            else
            {
                // Handle new case
                exprSwitchStatement.caseStatements.Add(context, new ExprCaseStatement
                {
                    leftComments = CheckForCommentsLeft(context.statementList(), 1)
                });
            }

            IndentLevel++;

            PushBlock();
        }

        public override void ExitExprCaseClause(GoParser.ExprCaseClauseContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            // exprCaseClause
            //     : exprSwitchCase ':' statementList

            // exprSwitchCase
            //     : 'case' expressionList | 'default'

            IndentLevel--;
            
            ExprSwitchStatement exprSwitchStatement = m_exprSwitchStatements.Peek();
            GoParser.ExpressionListContext expressionList = context.exprSwitchCase().expressionList();

            if (expressionList is null)
            {
                exprSwitchStatement.defaultCase.block = PopBlock(false);
            }
            else
            {
                if (!ExpressionLists.TryGetValue(expressionList, out ExpressionInfo[] expressions))
                    AddWarning(expressionList, $"Failed to find expression list for switch case statement: {context.exprSwitchCase()?.GetText()}");

                ExprCaseStatement caseStatement = exprSwitchStatement.caseStatements[context];

                caseStatement.expressionList = expressions;
                caseStatement.hasFallthrough = m_fallThrough;
                caseStatement.block = PopBlock(false);
            }

            // Reset fallthrough flag at the end of each case clause
            //m_exprSwitchPriorCaseFallThrough = m_fallThrough;
            m_fallThrough = false;
        }

        public override void ExitExprSwitchStmt(GoParser.ExprSwitchStmtContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            ExprSwitchStatement exprSwitchStatement = m_exprSwitchStatements.Pop();
            ExpressionInfo expression = null;

            if (!(context.expression() is null) && !Expressions.TryGetValue(context.expression(), out expression))
                AddWarning(context, $"Failed to find expression for switch statement: {context.expression().GetText()}");

            if (exprSwitchStatement.anyFallthroughs)
            {
                // Most complex scenario with standalone if's, fallthrough tests and goto case break-outs
                string breakLabel = $"__switch_break{m_exprSwitchBreakCounter++}";

                string expressionText = "";

                if (!string.IsNullOrEmpty(expression?.Text))
                    expressionText = $"{expression!.Text} == ";

                ExprCaseStatement[] caseStatements = exprSwitchStatement.caseStatements.Values.ToArray();

                for (int i = 0; i < caseStatements.Length; i++)
                {
                    ExprCaseStatement caseStatement = caseStatements[i];
                    
                    if (i > 0)
                        m_targetFile.Append(caseStatement.leftComments);
                    else if (!string.IsNullOrWhiteSpace(caseStatement.leftComments))
                        m_targetFile.Append(caseStatement.leftComments);

                    m_targetFile.Append($"{Spacing()}if (");

                    if (i > 0)
                    {
                        if (caseStatements[i - 1].hasFallthrough)
                            m_targetFile.Append("fallthrough || ");
                    }

                    for (int j = 0; j < caseStatement.expressionList.Length; j++)
                    {
                        ExpressionInfo caseExpr = caseStatement.expressionList[j];

                        if (j > 0)
                            m_targetFile.Append(" || ");

                        m_targetFile.Append($"{expressionText}{caseExpr.Text}");
                    }

                    m_targetFile.Append($"){Environment.NewLine}{Spacing()}{{{Environment.NewLine}{FixForwardSpacing(caseStatement.block, 1)}");

                    if (caseStatement.hasFallthrough)
                    {
                        if (i < caseStatements.Length - 1)
                            m_targetFile.Append($"{Spacing(1)}fallthrough = true;{Environment.NewLine}");
                    }
                    else
                    {
                        m_targetFile.Append($"{Spacing(1)}goto {breakLabel};{Environment.NewLine}");
                    }

                    m_targetFile.Append($"{Spacing()}}}");
                }

                if (!(exprSwitchStatement.defaultCase is null))
                {
                    m_targetFile.Append(exprSwitchStatement.defaultCase.leftComments);
                    m_targetFile.Append($"{Spacing()}// default:{Environment.NewLine}{RemoveLastLineFeed(FixForwardSpacing(exprSwitchStatement.defaultCase.block))}");
                }

                m_targetFile.Append($"{Environment.NewLine}{Spacing()}{breakLabel}:;{CheckForCommentsRight(context)}");
            }
            else if (exprSwitchStatement.allConst && !(expression is null))
            {
                // Most simple scenario when all case values are constant, a common C# switch will suffice
                m_targetFile.Append($"{Spacing()}switch ({expression.Text}){Environment.NewLine}{Spacing()}{{");

                foreach (ExprCaseStatement caseStatement in exprSwitchStatement.caseStatements.Values)
                {
                    m_targetFile.Append(caseStatement.leftComments);

                    if (!EndsWithLineFeed(caseStatement.leftComments))
                        m_targetFile.AppendLine();

                    foreach (ExpressionInfo caseExpr in caseStatement.expressionList)
                        m_targetFile.AppendLine($"{Spacing(1)}case {caseExpr.Text}:");

                    m_targetFile.Append(FixForwardSpacing(caseStatement.block, 2));
                    m_targetFile.Append($"{Spacing(2)}break;");
                }

                if (!(exprSwitchStatement.defaultCase is null))
                {
                    m_targetFile.Append(exprSwitchStatement.defaultCase.leftComments);
                    m_targetFile.AppendLine($"{Spacing(1)}default:");
                    m_targetFile.Append(FixForwardSpacing(exprSwitchStatement.defaultCase.block, 2));
                    m_targetFile.Append($"{Spacing(2)}break;");
                }

                m_targetFile.Append($"{Environment.NewLine}{Spacing()}}}{CheckForCommentsRight(context)}");
            }
            else
            {
                // Most common scenario where expression switch becomes "if / else if / else" statements
                string expressionText = "";

                if (!string.IsNullOrEmpty(expression?.Text))
                    expressionText = $"{expression!.Text} == ";

                ExprCaseStatement[] caseStatements = exprSwitchStatement.caseStatements.Values.ToArray();

                for (int i = 0; i < caseStatements.Length; i++)
                {
                    ExprCaseStatement caseStatement = caseStatements[i];
                    
                    if (i > 0)
                        m_targetFile.Append(caseStatement.leftComments);
                    else if (!string.IsNullOrWhiteSpace(caseStatement.leftComments))
                        m_targetFile.Append(caseStatement.leftComments);

                    m_targetFile.Append($"{Spacing()}{(i > 0 ? "else " : "")}if (");

                    for (int j = 0; j < caseStatement.expressionList.Length; j++)
                    {
                        ExpressionInfo caseExpr = caseStatement.expressionList[j];

                        if (j > 0)
                            m_targetFile.Append(" || ");

                        m_targetFile.Append($"{expressionText}{caseExpr.Text}");
                    }

                    m_targetFile.Append($"){Environment.NewLine}{RemoveLastLineFeed(caseStatement.block)}");
                }

                if (!(exprSwitchStatement.defaultCase is null))
                {
                    m_targetFile.Append(exprSwitchStatement.defaultCase.leftComments);
                    m_targetFile.Append($"{Spacing()}else{Environment.NewLine}{RemoveLastLineFeed(exprSwitchStatement.defaultCase.block)}");
                }

                m_targetFile.Append($"{CheckForCommentsRight(context)}");
            }

            if (!(context.simpleStmt() is null) && context.simpleStmt().emptyStmt() is null)
            {
                if (m_simpleStatements.TryGetValue(context.simpleStmt(), out string statement))
                    m_targetFile.Replace(string.Format(ExprSwitchStatementMarker, m_exprSwitchExpressionLevel), $"{statement}{Environment.NewLine}");
                else
                    AddWarning(context, $"Failed to find simple statement for expression based switch statement: {context.simpleStmt().GetText()}");

                // Close any locally scoped declared variable sub-block
                if (!(context.simpleStmt().shortVarDecl() is null))
                {
                    // Handle restoration of previous values of any redeclared variables
                    m_targetFile.Append(CloseRedeclaredVariableBlock(context.simpleStmt().shortVarDecl(), m_exprSwitchExpressionLevel));

                    IndentLevel--;
                    m_targetFile.Append($"{Spacing()}}}");
                }
            }

            m_exprSwitchExpressionLevel--;
        }
    }
}

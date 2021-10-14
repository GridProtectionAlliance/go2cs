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
using Antlr4.Runtime.Tree;

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
            public string fallthroughComment;

            public bool allConst => expressionList?.All(expr => expr?.Type?.IsConst ?? false) ?? false;
        }

        private class ExprSwitchStatement
        {
            public readonly ParseTreeValues<ExprCaseStatement> caseStatements = new ParseTreeValues<ExprCaseStatement>();
            public ExprCaseStatement defaultCase;
            public string intraSwitchComments;

            public bool allConst => caseStatements?.Select(stmt => stmt.Value).All(stmt => stmt.allConst) ?? false;
            public bool anyFallthroughs => caseStatements?.Select(stmt => stmt.Value).Any(stmt => stmt.hasFallthrough) ?? false;
        }

        public const string ExprSwitchStatementMarker = ">>MARKER:EXPRSWITCHSTATEMENT_LEVEL_{0}<<";

        private readonly Stack<ExprSwitchStatement> m_exprSwitchStatements = new Stack<ExprSwitchStatement>();
        private int m_exprSwitchExpressionLevel;
        private int m_exprSwitchBreakCounter;
        
        private bool m_fallThrough;
        private string m_fallThroughComment;

        public override void ExitFallthroughStmt(GoParser.FallthroughStmtContext context)
        {
            // fallthroughStmt
            //     : 'fallthrough'

            m_fallThrough = true;
            m_fallThroughComment = CheckForCommentsRight(context);
        }

        public override void EnterExprSwitchStmt(GoParser.ExprSwitchStmtContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            m_exprSwitchExpressionLevel++;

            if (context.simpleStmt() is not null && context.simpleStmt().emptyStmt() is null)
            {
                if (context.simpleStmt().shortVarDecl() is not null)
                {
                    // Any declared variable will be scoped to switch statement, so create a sub-block for it
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    IndentLevel++;
                    
                    // Handle storing of current values of any redeclared variables
                    m_targetFile.Append(OpenRedeclaredVariableBlock(context.simpleStmt().shortVarDecl().identifierList(), m_exprSwitchExpressionLevel));
                }

                m_targetFile.Append(string.Format(ExprSwitchStatementMarker, m_exprSwitchExpressionLevel));
            }

            ExprSwitchStatement exprSwitchStatement = new ExprSwitchStatement();

            foreach (IParseTree child in context.children)
            {
                if (child.GetText() == "{")
                {
                    exprSwitchStatement.intraSwitchComments = CheckForCommentsRight(child);
                    break;
                }
            }

            m_exprSwitchStatements.Push(exprSwitchStatement);
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
                caseStatement.fallthroughComment = m_fallThroughComment;
                caseStatement.block = PopBlock(false);
            }

            // Reset fallthrough flag at the end of each case clause
            //m_exprSwitchPriorCaseFallThrough = m_fallThrough;
            m_fallThrough = false;
            m_fallThroughComment = "";
        }

        public override void ExitExprSwitchStmt(GoParser.ExprSwitchStmtContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            ExprSwitchStatement exprSwitchStatement = m_exprSwitchStatements.Pop();
            ExpressionInfo expression = null;

            if (context.expression() is not null && !Expressions.TryGetValue(context.expression(), out expression))
                AddWarning(context, $"Failed to find expression for switch statement: {context.expression().GetText()}");

            bool hasSwitchStatement = context.simpleStmt() is not null && context.simpleStmt().emptyStmt() is null;

            if (exprSwitchStatement.anyFallthroughs)
            {
                // Most complex scenario with standalone if's, fallthrough tests and goto case break-outs
                string breakLabel = $"__switch_break{m_exprSwitchBreakCounter++}";

                string expressionText = "";

                if (!string.IsNullOrEmpty(expression?.Text))
                    expressionText = $"{expression!.Text} == ";

                ExprCaseStatement[] caseStatements = exprSwitchStatement.caseStatements.Values.ToArray();

                if (!string.IsNullOrEmpty(exprSwitchStatement.intraSwitchComments))
                    m_targetFile.Append(FixForwardSpacing(exprSwitchStatement.intraSwitchComments));

                for (int i = 0; i < caseStatements.Length; i++)
                {
                    ExprCaseStatement caseStatement = caseStatements[i];
                    
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

                    (string eolComment, string comments) = SplitEOLComment(caseStatement.leftComments);
                    m_targetFile.Append($"){eolComment}{Environment.NewLine}{Spacing()}{{{Environment.NewLine}{comments}{caseStatement.block}");
                    if (caseStatement.hasFallthrough)
                    {
                        if (i < caseStatements.Length - 1)
                            m_targetFile.Append($"{Spacing(1)}fallthrough = true;{caseStatement.fallthroughComment}");
                    }
                    else
                    {
                        m_targetFile.Append($"{Spacing(1)}goto {breakLabel};{Environment.NewLine}");
                    }

                    m_targetFile.Append($"{Spacing()}}}{Environment.NewLine}");
                }

                if (exprSwitchStatement.defaultCase is not null)
                    m_targetFile.Append($"{Spacing()}// default:{FixForwardSpacing(exprSwitchStatement.defaultCase.leftComments, 1, firstIsEOLComment: true)}{exprSwitchStatement.defaultCase.block}");

                m_targetFile.Append($"{Environment.NewLine}{Spacing()}{breakLabel}:;{(hasSwitchStatement ? Environment.NewLine : CheckForCommentsRight(context))}");
            }
            else if (exprSwitchStatement.allConst && expression is not null)
            {
                // Most simple scenario when all case values are constant, a common C# switch will suffice
                m_targetFile.Append($"{Spacing()}switch ({expression.Text}){Environment.NewLine}{Spacing()}{{{RemoveLastLineFeed(exprSwitchStatement.intraSwitchComments)}");

                foreach (ExprCaseStatement caseStatement in exprSwitchStatement.caseStatements.Values)
                {
                    foreach (ExpressionInfo caseExpr in caseStatement.expressionList)
                        m_targetFile.AppendLine($"{Environment.NewLine}{Spacing(1)}case {caseExpr.Text}:{RemoveLastLineFeed(FixForwardSpacing(caseStatement.leftComments, 2, firstIsEOLComment: true))}");

                    m_targetFile.Append(FixForwardSpacing(caseStatement.block, 2));
                    m_targetFile.Append($"{Spacing(2)}break;");
                }

                if (exprSwitchStatement.defaultCase is not null)
                {
                    m_targetFile.AppendLine($"{Environment.NewLine}{Spacing(1)}default:{RemoveLastLineFeed(FixForwardSpacing(exprSwitchStatement.defaultCase.leftComments, 2, firstIsEOLComment: true))}");
                    m_targetFile.Append(FixForwardSpacing(exprSwitchStatement.defaultCase.block, 2));
                    m_targetFile.Append($"{Spacing(2)}break;");
                }

                m_targetFile.Append($"{Environment.NewLine}{Spacing()}}}{(hasSwitchStatement ? Environment.NewLine : CheckForCommentsRight(context))}");
            }
            else
            {
                // Most common scenario where expression switch becomes "if / else if / else" statements
                string expressionText = "";

                if (!string.IsNullOrEmpty(expression?.Text))
                    expressionText = $"{expression!.Text} == ";

                ExprCaseStatement[] caseStatements = exprSwitchStatement.caseStatements.Values.ToArray();

                if (!string.IsNullOrEmpty(exprSwitchStatement.intraSwitchComments))
                    m_targetFile.Append(FixForwardSpacing(exprSwitchStatement.intraSwitchComments));

                for (int i = 0; i < caseStatements.Length; i++)
                {
                    ExprCaseStatement caseStatement = caseStatements[i];
                    
                    m_targetFile.Append($"{Spacing()}{(i > 0 ? "else " : "")}if (");

                    for (int j = 0; j < caseStatement.expressionList.Length; j++)
                    {
                        ExpressionInfo caseExpr = caseStatement.expressionList[j];

                        if (j > 0)
                            m_targetFile.Append(" || ");

                        m_targetFile.Append($"{expressionText}{caseExpr.Text}");
                    }

                    m_targetFile.Append($"){FixForwardSpacing(caseStatement.leftComments, 1, firstIsEOLComment: true)}{caseStatement.block}");
                }

                if (exprSwitchStatement.defaultCase is not null)
                {
                    m_targetFile.Append($"{Spacing()}else{FixForwardSpacing(exprSwitchStatement.defaultCase.leftComments, 1, firstIsEOLComment: true)}{exprSwitchStatement.defaultCase.block}");
                }

                if (hasSwitchStatement)
                    m_targetFile.AppendLine();
                else
                    m_targetFile.Append($"{Spacing()}{CheckForCommentsRight(context).TrimStart()}");
            }

            if (hasSwitchStatement)
            {
                if (m_simpleStatements.TryGetValue(context.simpleStmt(), out string statement))
                    m_targetFile.Replace(string.Format(ExprSwitchStatementMarker, m_exprSwitchExpressionLevel), $"{statement}{Environment.NewLine}");
                else
                    AddWarning(context, $"Failed to find simple statement for expression based switch statement: {context.simpleStmt().GetText()}");

                // Close any locally scoped declared variable sub-block
                if (context.simpleStmt().shortVarDecl() is not null)
                {
                    // Handle restoration of previous values of any redeclared variables
                    m_targetFile.Append(CloseRedeclaredVariableBlock(context.simpleStmt().shortVarDecl().identifierList(), m_exprSwitchExpressionLevel));

                    IndentLevel--;
                    m_targetFile.Append($"{Spacing()}}}{CheckForCommentsRight(context)}");
                }
            }

            m_exprSwitchExpressionLevel--;
        }
    }
}

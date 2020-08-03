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

namespace go2cs
{
    public partial class Converter
    {
        public const string ForExpressionMarker = ">>MARKER:FOREXPRESSION_LEVEL_{0}<<";
        public const string ForInitStatementMarker = ">>MARKER:FORINITSTATEMENT_LEVEL_{0}<<";
        public const string ForPostStatementMarker = ">>MARKER:FORPOSTSTATEMENT_LEVEL_{0}<<";
        //public const string ForRangeExpressionsMarker = ">>MARKER:FORRANGEEXPRESSIONS_LEVEL_{0}<<";

        private int m_forExpressionLevel;

        private bool ForHasInitStatement(GoParser.ForClauseContext forClause, out GoParser.SimpleStmtContext simpleStatement, out bool isRedeclared)
        {
            simpleStatement = null;
            isRedeclared = false;

            if (forClause.simpleStmt()?.Length == 0)
                return false;

            simpleStatement = forClause.simpleStmt(0);

            // Check if simple statement is short variable declaration, if so, check if it's redeclared
            if (simpleStatement?.shortVarDecl() != null)
            {
                GoParser.ShortVarDeclContext shortVarDecl = simpleStatement.shortVarDecl();
                GoParser.IdentifierListContext identifierList = shortVarDecl.identifierList();

                if (!Identifiers.TryGetValue(identifierList, out string[] identifiers))
                {
                    // Pre-visit identifiers if they are not defined yet
                    EnterIdentifierList(identifierList);

                    if (!Identifiers.TryGetValue(identifierList, out identifiers))
                    {
                        AddWarning(shortVarDecl, $"Failed to find identifier lists for short var declaration statements: {shortVarDecl.GetText()}");
                        identifiers = Array.Empty<string>();
                    }
                }

                foreach (string identifier in identifiers)
                {
                    if (TryGetFunctionVariable(identifier, out VariableInfo variable) && variable.Redeclared)
                    {
                        isRedeclared = true;
                        break;
                    }
                }
            }

            return
                !(simpleStatement is null) &&
                simpleStatement.emptyStmt() is null &&
                forClause.children[^1] != simpleStatement;
        }

        private bool ForHasPostStatement(GoParser.ForClauseContext forClause, out GoParser.SimpleStmtContext simpleStatement)
        {
            simpleStatement = null;

            if (forClause.simpleStmt()?.Length == 0)
                return false;

            simpleStatement = forClause.simpleStmt(0);

            if (forClause.simpleStmt().Length == 1)
                return
                    !(simpleStatement is null) &&
                    simpleStatement.emptyStmt() is null &&
                    forClause.children[^1] == simpleStatement;

            simpleStatement = forClause.simpleStmt(1);

            return
                !(simpleStatement is null) &&
                simpleStatement.emptyStmt() is null &&
                forClause.children[^1] == simpleStatement;
        }

        public override void EnterForStmt(GoParser.ForStmtContext context)
        {
            // forStmt
            //     : 'for' (expression | forClause | rangeClause)? block

            m_forExpressionLevel++;

            GoParser.ForClauseContext forClause = context.forClause();

            if (!(context.expression() is null) || !(forClause is null) && forClause.simpleStmt()?.Length == 0 || context.children.Count < 3)
            {
                // Handle while-style statement
                m_targetFile.AppendLine($"{Spacing()}while ({string.Format(ForExpressionMarker, m_forExpressionLevel)})");
            }
            else if (!(forClause is null))
            {
                // forClause
                //     : simpleStmt? ';' expression? ';' simpleStmt?

                bool hasInitStatement = ForHasInitStatement(forClause, out GoParser.SimpleStmtContext simpleInitStatement, out bool isRedeclared);
                bool hasPostStatement = ForHasPostStatement(forClause, out GoParser.SimpleStmtContext simplePostStatement);
                bool useForStyleStatement =
                    hasInitStatement && (!(simpleInitStatement.shortVarDecl() is null) || !(simpleInitStatement.assignment() is null)) &&
                    hasPostStatement && (!(simplePostStatement.incDecStmt() is null) || !(simplePostStatement.expressionStmt() is null));

                if (hasInitStatement)
                {
                    // Any declared variable will be scoped to for statement, so create a sub-block for it
                    if (isRedeclared || !useForStyleStatement && !(simpleInitStatement.shortVarDecl() is null))
                    {
                        m_targetFile.AppendLine($"{Spacing()}{{");
                        IndentLevel++;

                        // Handle storing of current values of any redeclared variables
                        m_targetFile.Append(OpenRedeclaredVariableBlock(simpleInitStatement.shortVarDecl(), m_forExpressionLevel));
                    }
                }

                if (hasInitStatement && (!(simpleInitStatement.shortVarDecl() is null) || !(simpleInitStatement.assignment() is null)) &&
                    hasPostStatement && (!(simplePostStatement.incDecStmt() is null) || !(simplePostStatement.expressionStmt() is null)))
                {
                    // Use standard for-style statement for simple constructs
                    m_targetFile.AppendLine($"{Spacing()}for ({string.Format(ForInitStatementMarker, m_forExpressionLevel)}; {string.Format(ForExpressionMarker, m_forExpressionLevel)}; {string.Format(ForPostStatementMarker, m_forExpressionLevel)})");
                    PushInnerBlockSuffix(null);
                }
                else
                {
                    if (hasInitStatement)
                        m_targetFile.Append(string.Format(ForInitStatementMarker, m_forExpressionLevel));

                    if (hasPostStatement)
                        PushInnerBlockSuffix($"{Spacing(indentLevel: 1)}{string.Format(ForPostStatementMarker, m_forExpressionLevel)}");
                    else
                        PushInnerBlockSuffix(null);

                    // Use loop-style statement for more free-form for constructs
                    m_targetFile.AppendLine($"{Spacing()}while ({string.Format(ForExpressionMarker, m_forExpressionLevel)})");
                }
            }
            else
            {
                // rangeClause
                //     : ( expressionList '=' | identifierList ':=' )? 'range' expression

                if (!(context.rangeClause().identifierList() is null))
                {
                    // Any declared variable will be scoped to for statement, so create a sub-block for it
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    IndentLevel++;
                }

                // Handle item iteration style statement - since Go's iteration variables are mutable and
                // can be pre-declared, a standard for loop is used instead of a foreach. The exception
                // is for an index and rune iteration over a string which in C# a foreach works fine because
                // the inferred tuple instance can be readonly since it would not be referenced by Go code.
            }
        }

        public override void ExitForStmt(GoParser.ForStmtContext context)
        {
            // forStmt
            //     : 'for' (expression | forClause | rangeClause)? block

            GoParser.ForClauseContext forClause = context.forClause();

            if (!(context.expression() is null) || !(forClause is null) && forClause.simpleStmt()?.Length == 0 || context.children.Count < 3)
            {
                Expressions.TryGetValue(context.expression() ?? forClause?.expression(), out ExpressionInfo expression);
                m_targetFile.Replace(string.Format(ForExpressionMarker, m_forExpressionLevel), expression?.Text ?? "true");
            }
            else if (!(forClause is null))
            {
                // forClause
                //     : simpleStmt? ';' expression? ';' simpleStmt?

                if (Expressions.TryGetValue(forClause.expression(), out ExpressionInfo expression))
                    m_targetFile.Replace(string.Format(ForExpressionMarker, m_forExpressionLevel), expression.Text);
                else
                    AddWarning(context, $"Failed to find expression in for statement: {context.GetText()}");

                bool hasInitStatement = ForHasInitStatement(forClause, out GoParser.SimpleStmtContext simpleInitStatement, out bool isRedeclared);
                bool hasPostStatement = ForHasPostStatement(forClause, out GoParser.SimpleStmtContext simplePostStatement);
                bool useForStyleStatement =
                    hasInitStatement && (!(simpleInitStatement.shortVarDecl() is null) || !(simpleInitStatement.assignment() is null)) &&
                    hasPostStatement && (!(simplePostStatement.incDecStmt() is null) || !(simplePostStatement.expressionStmt() is null));

                if (hasInitStatement)
                {
                    if (m_simpleStatements.TryGetValue(simpleInitStatement, out string statement))
                    {
                        if (useForStyleStatement)
                        {
                            statement = statement.Trim();

                            if (statement.EndsWith(";", StringComparison.Ordinal))
                                statement = statement.Substring(0, statement.Length - 1);
                        }
                        else
                        {
                            statement = $"{statement}{Environment.NewLine}";
                        }

                        m_targetFile.Replace(string.Format(ForInitStatementMarker, m_forExpressionLevel), statement);
                    }
                    else
                    {
                        AddWarning(context, $"Failed to find simple initial statement in for clause: {simpleInitStatement.GetText()}");
                    }

                    // Close any locally scoped declared variable sub-block
                    if (isRedeclared || !useForStyleStatement && !(simpleInitStatement.shortVarDecl() is null))
                    {
                        // Handle restoration of previous values of any redeclared variables
                        string closeRedeclarations = CloseRedeclaredVariableBlock(simpleInitStatement.shortVarDecl(), m_forExpressionLevel);

                        if (!string.IsNullOrEmpty(closeRedeclarations))
                            m_targetFile.Append($"{Environment.NewLine}{RemoveLastLineFeed(closeRedeclarations)}");

                        IndentLevel--;
                        m_targetFile.AppendLine();
                        m_targetFile.Append($"{Spacing()}}}");
                    }
                }

                if (hasPostStatement)
                {
                    if (m_simpleStatements.TryGetValue(simplePostStatement, out string statement))
                    {
                        if (useForStyleStatement)
                        {
                            statement = statement.Trim();

                            if (statement.EndsWith(";", StringComparison.Ordinal))
                                statement = statement.Substring(0, statement.Length - 1);
                        }
                        else
                        {
                            statement = statement.TrimEnd();
                        }

                        m_targetFile.Replace(string.Format(ForPostStatementMarker, m_forExpressionLevel), statement);
                    }
                    else
                    {
                        AddWarning(context, $"Failed to find simple post statement in for clause: {simplePostStatement.GetText()}");
                    }
                }
            }
            else
            {
                // rangeClause
                //     : (expressionList '=' | identifierList ':=' )? 'range' expression

                if (!(context.rangeClause().identifierList() is null))
                {
                    // Close any locally scoped declared variable sub-block
                    IndentLevel--;
                    m_targetFile.AppendLine();
                    m_targetFile.Append($"{Spacing()}}}");
                }
            }

            m_targetFile.Append(CheckForCommentsRight(context));

            m_forExpressionLevel--;
        }
    }
}

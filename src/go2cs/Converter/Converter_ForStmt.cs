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
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;

namespace go2cs;

public partial class Converter
{
    public const string ForExpressionMarker = ">>MARKER:FOREXPRESSION_LEVEL_{0}<<";
    public const string ForInitStatementMarker = ">>MARKER:FORINITSTATEMENT_LEVEL_{0}<<";
    public const string ForPostStatementMarker = ">>MARKER:FORPOSTSTATEMENT_LEVEL_{0}<<";
    public const string ForRangeExpressionsMarker = ">>MARKER:FORRANGEEXPRESSIONS_LEVEL_{0}<<";
    public const string ForRangeBlockMutableExpressionsMarker = ">>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_{0}<<";

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
            simpleStatement is not null &&
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
                simpleStatement is not null &&
                simpleStatement.emptyStmt() is null &&
                forClause.children[^1] == simpleStatement;

        simpleStatement = forClause.simpleStmt(1);

        return
            simpleStatement is not null &&
            simpleStatement.emptyStmt() is null &&
            forClause.children[^1] == simpleStatement;
    }

    public override void EnterForStmt(GoParser.ForStmtContext context)
    {
        // forStmt
        //     : 'for' (expression | forClause | rangeClause)? block

        m_forExpressionLevel++;

        GoParser.ForClauseContext forClause = context.forClause();

        if (context.expression() is not null || forClause is not null && forClause.simpleStmt()?.Length == 0 || context.children.Count < 3)
        {
            // Handle while-style statement
            m_targetFile.Append($"{Spacing()}while ({string.Format(ForExpressionMarker, m_forExpressionLevel)})");

            if (Options.UseAnsiBraceStyle)
                m_targetFile.AppendLine();
        }
        else if (forClause is not null)
        {
            // forClause
            //     : simpleStmt? ';' expression? ';' simpleStmt?

            bool hasInitStatement = ForHasInitStatement(forClause, out GoParser.SimpleStmtContext simpleInitStatement, out bool isRedeclared);
            bool hasPostStatement = ForHasPostStatement(forClause, out GoParser.SimpleStmtContext simplePostStatement);
            bool useForStyleStatement =
                hasInitStatement && (simpleInitStatement.shortVarDecl() is not null || simpleInitStatement.assignment() is not null) &&
                hasPostStatement && (simplePostStatement.incDecStmt() is not null || simplePostStatement.expressionStmt() is not null);

            if (hasInitStatement)
            {
                // Any declared variable will be scoped to for statement, so create a sub-block for it
                if (isRedeclared || !useForStyleStatement && simpleInitStatement.shortVarDecl() is not null)
                {
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    IndentLevel++;

                    // Handle storing of current values of any redeclared variables
                    m_targetFile.Append(OpenRedeclaredVariableBlock(simpleInitStatement.shortVarDecl().identifierList(), m_forExpressionLevel));
                }
            }

            if (hasInitStatement && (simpleInitStatement.shortVarDecl() is not null || simpleInitStatement.assignment() is not null) &&
                hasPostStatement && (simplePostStatement.incDecStmt() is not null || simplePostStatement.expressionStmt() is not null))
            {
                // Use standard for-style statement for simple constructs
                m_targetFile.Append($"{Spacing()}for ({string.Format(ForInitStatementMarker, m_forExpressionLevel)}; {string.Format(ForExpressionMarker, m_forExpressionLevel)}; {string.Format(ForPostStatementMarker, m_forExpressionLevel)})");

                if (Options.UseAnsiBraceStyle)
                    m_targetFile.AppendLine();

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
                m_targetFile.Append($"{Spacing()}while ({string.Format(ForExpressionMarker, m_forExpressionLevel)})");

                if (Options.UseAnsiBraceStyle)
                    m_targetFile.AppendLine();
            }
        }
        else
        {
            // rangeClause
            //     : ( expressionList '=' | identifierList ':=' )? 'range' expression

            GoParser.RangeClauseContext rangeClause = context.rangeClause();

            if (rangeClause is not null)
            {
                if (rangeClause.identifierList() is not null)
                {
                    GoParser.IdentifierListContext identifiers = rangeClause.identifierList();
                    bool isRedeclared = false;

                    for (int i = 0; i < identifiers.ChildCount; i++)
                    {
                        IParseTree identifer = identifiers.GetChild(i);

                        if (TryGetFunctionVariable(identifer.GetText(), out VariableInfo variable) && variable.Redeclared)
                        {
                            isRedeclared = true;
                            break;
                        }
                    }
                        
                    if (isRedeclared)
                    {
                        m_targetFile.AppendLine($"{Spacing()}{{");
                        IndentLevel++;

                        // Handle storing of current values of any redeclared variables
                        m_targetFile.Append(OpenRedeclaredVariableBlock(rangeClause.identifierList(), m_forExpressionLevel));
                    }
                }

                m_targetFile.Append($"{Spacing()}foreach ({string.Format(ForRangeExpressionsMarker, m_forExpressionLevel)} in {string.Format(ForExpressionMarker, m_forExpressionLevel)})");

                if (Options.UseAnsiBraceStyle)
                    m_targetFile.AppendLine();

                PushInnerBlockPrefix(string.Format(ForRangeBlockMutableExpressionsMarker, m_forExpressionLevel));
                PushInnerBlockSuffix(null);
            }
        }
    }

    public override void ExitForStmt(GoParser.ForStmtContext context)
    {
        // forStmt
        //     : 'for' (expression | forClause | rangeClause)? block

        GoParser.ForClauseContext forClause = context.forClause();

        if (context.expression() is not null || forClause is not null && forClause.simpleStmt()?.Length == 0 || context.children.Count < 3)
        {
            Expressions.TryGetValue(context.expression() ?? forClause?.expression(), out ExpressionInfo expression);
            m_targetFile.Replace(string.Format(ForExpressionMarker, m_forExpressionLevel), expression?.Text ?? "true");
            m_targetFile.Append(CheckForCommentsRight(context));
        }
        else if (forClause is not null)
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
                hasInitStatement && (simpleInitStatement.shortVarDecl() is not null || simpleInitStatement.assignment() is not null) &&
                hasPostStatement && (simplePostStatement.incDecStmt() is not null || simplePostStatement.expressionStmt() is not null);

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
                if (isRedeclared || !useForStyleStatement && simpleInitStatement.shortVarDecl() is not null)
                {
                    // Handle restoration of previous values of any redeclared variables
                    string closeRedeclarations = CloseRedeclaredVariableBlock(simpleInitStatement.shortVarDecl().identifierList(), m_forExpressionLevel);

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

            m_targetFile.Append(CheckForCommentsRight(context));
        }
        else
        {
            // rangeClause
            //     : (expressionList '=' | identifierList ':=' )? 'range' expression

            GoParser.RangeClauseContext rangeClause = context.rangeClause();

            ExpressionInfo[] expressions = null;
            string[] identifiers = null;

            if (rangeClause is not null)
            {
                if (rangeClause.identifierList() is not null && !Identifiers.TryGetValue(rangeClause.identifierList(), out identifiers))
                    AddWarning(context, $"Failed to find identifier list in range expression: {rangeClause.identifierList().GetText()}");

                if (rangeClause.expressionList() is not null && !ExpressionLists.TryGetValue(rangeClause.expressionList(), out expressions))
                    AddWarning(context, $"Failed to find expression list in range expression: {rangeClause.expressionList().GetText()}");

                if (expressions is not null)
                {
                    string[] expressionTexts = expressions.Select(expr => expr.Text).ToArray();
                    m_targetFile.AppendLine();

                    StringBuilder mutable = new StringBuilder();

                    for (int i = 0; i < expressionTexts.Length; i++)
                    {
                        if (!expressionTexts[i].Equals("_"))
                            mutable.Append($"{Environment.NewLine}{Spacing(1)}{expressionTexts[i]} = __{expressionTexts[i]};");
                    }

                    string immutable = string.Join(", ", expressionTexts.Select(expr => expr == "_" ? "_" : $"__{expr}"));

                    m_targetFile.Replace(string.Format(ForRangeExpressionsMarker, m_forExpressionLevel), $"var ({string.Join(", ", immutable)})");
                    m_targetFile.Replace(string.Format(ForRangeBlockMutableExpressionsMarker, m_forExpressionLevel), mutable.ToString());
                }

                if (identifiers is not null)
                {
                    bool isRedeclared = false;

                    foreach (string identifier in identifiers)
                    {
                        if (TryGetFunctionVariable(identifier, out VariableInfo variable) && variable.Redeclared)
                        {
                            isRedeclared = true;
                            break;
                        }
                    }

                    if (isRedeclared)
                    {
                        // Handle restoration of previous values of any redeclared variables
                        string closeRedeclarations = CloseRedeclaredVariableBlock(rangeClause.identifierList(), m_forExpressionLevel);

                        if (!string.IsNullOrEmpty(closeRedeclarations))
                            m_targetFile.Append(RemoveLastLineFeed(closeRedeclarations));

                        IndentLevel--;
                        m_targetFile.AppendLine();
                        m_targetFile.AppendLine($"{Spacing()}}}{Environment.NewLine}");

                        StringBuilder mutable = new StringBuilder();

                        for (int i = 0; i < identifiers.Length; i++)
                        {
                            if (!identifiers[i].Equals("_"))
                                mutable.Append($"{Environment.NewLine}{Spacing(2)}{identifiers[i]} = __{identifiers[i]};");
                        }

                        string immutable = string.Join(", ", identifiers.Select(expr => expr == "_" ? "_" : $"__{expr}"));

                        m_targetFile.Replace(string.Format(ForRangeExpressionsMarker, m_forExpressionLevel), $"var ({string.Join(", ", immutable)})");
                        m_targetFile.Replace(string.Format(ForRangeBlockMutableExpressionsMarker, m_forExpressionLevel), mutable.ToString());
                    }
                    else
                    {
                        m_targetFile.Replace(string.Format(ForRangeExpressionsMarker, m_forExpressionLevel), $"var ({string.Join(", ", identifiers)})");
                        m_targetFile.Replace(string.Format(ForRangeBlockMutableExpressionsMarker, m_forExpressionLevel), string.Empty);
                    }
                }

                Expressions.TryGetValue(rangeClause.expression(), out ExpressionInfo expression);
                m_targetFile.Replace(string.Format(ForExpressionMarker, m_forExpressionLevel), expression?.Text ?? "true");
            }
        }

        m_forExpressionLevel--;
    }
}

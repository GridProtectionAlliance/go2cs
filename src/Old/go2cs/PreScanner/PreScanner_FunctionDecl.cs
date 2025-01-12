//******************************************************************************************************
//  PreScanner_FunctionDecl.cs - Gbtc
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
//  06/21/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using go2cs.Metadata;
using System;
using System.Collections.Generic;

namespace go2cs;

public partial class PreScanner
{
    private readonly Dictionary<string, VariableInfo> m_variables = new(StringComparer.Ordinal);
    private bool m_hasDefer;
    private bool m_hasPanic;
    private bool m_hasRecover;

    public override void ExitUnaryExpr(GoParser.UnaryExprContext context)
    {
        base.ExitUnaryExpr(context);

        ExpressionInfo expression = null;

        if (context.primaryExpr() is not null && !PrimaryExpressions.ContainsKey(context.primaryExpr()) ||
            context.expression() is not null && !Expressions.TryGetValue(context.expression(), out expression) ||
            expression is null)
            return;

        // Taking the address of a variable means making a stack variable heap allocated
        if (context.children[0].GetText().Equals("&", StringComparison.Ordinal) && m_variables.TryGetValue(expression.Text, out VariableInfo variable))
            variable.HeapAllocated = true;
    }

    private void EnterMethod()
    {
        m_variables.Clear();
        m_hasDefer = false;
        m_hasPanic = false;
        m_hasRecover = false;
    }

    private FunctionInfo ExitMethod(GoParser.IFunctionContext context)
    {
        if (context.IDENTIFIER() is null)
            return null;

        string identifer = context.IDENTIFIER().GetText();
        GoParser.SignatureContext signatureContext = context.signature();

        if (!Signatures.TryGetValue(signatureContext, out Signature signature))
        {
            signatureContext = context.signature();
            Signatures.TryGetValue(signatureContext, out signature);
        }

        return new FunctionInfo
        {
            Signature = new FunctionSignature
            {
                Name = identifer,
                Signature = signature,
                Comments = CheckForCommentsRight(signatureContext),
                IsPromoted = false
            },
            Variables = new Dictionary<string, VariableInfo>(m_variables),
            HasDefer = m_hasDefer,
            HasPanic = m_hasPanic,
            HasRecover = m_hasRecover
        };
    }

    public override void EnterFunctionDecl(GoParser.FunctionDeclContext context)
    {
        base.EnterFunctionDecl(context);

        EnterMethod();
    }

    public override void EnterMethodDecl(GoParser.MethodDeclContext context)
    {
        base.EnterMethodDecl(context);

        EnterMethod();
    }

    public override void ExitFunctionDecl(GoParser.FunctionDeclContext context)
    {
        FunctionInfo functionInfo = ExitMethod(context);

        if (functionInfo is null)
            return;

        m_functions.Add(GetUniqueIdentifier(m_functions, functionInfo.Signature.GenerateLookup()), functionInfo);

        base.ExitFunctionDecl(context);
    }

    public override void ExitMethodDecl(GoParser.MethodDeclContext context)
    {
        FunctionInfo functionInfo = ExitMethod(context);

        if (functionInfo is null)
            return;

        if (Parameters.TryGetValue(context.receiver().parameters(), out List<ParameterInfo> parameters))
        {
            functionInfo.Signature = new MethodSignature(functionInfo.Signature)
            {
                ReceiverParameters = parameters.ToArray()
            };
        }

        m_functions.Add(GetUniqueIdentifier(m_functions, functionInfo.Signature.GenerateLookup()), functionInfo);

        base.ExitMethodDecl(context);
    }

    public override void ExitVarSpec(GoParser.VarSpecContext context)
    {
        if (!Identifiers.TryGetValue(context.identifierList(), out string[] identifiers))
            return;

        if (!Types.TryGetValue(context.type_(), out TypeInfo typeInfo))
            typeInfo = TypeInfo.VarType;

        ExpressionLists.TryGetValue(context.expressionList(), out ExpressionInfo[] expressions);

        for (int i = 0; i < identifiers.Length; i++)
        {
            string identifier = identifiers[i];

            if (identifier.Equals("_"))
                continue;

            if (expressions is not null && expressions.Length <= i)
                continue;

            string uniqueIdentifer = GetUniqueIdentifier(m_variables, identifiers[i]);

            // TODO: This is not enough to detect redeclaraion use cases where say only two for loops use the same variable
            bool redeclared = uniqueIdentifer.Contains("@@");

            m_variables.Add(uniqueIdentifer, new VariableInfo
            {
                Name = identifier,
                Type = typeInfo,
                HeapAllocated = expressions?[i]?.Text.StartsWith("&") ?? false,
                Redeclared = redeclared
            });

            if (redeclared && m_variables.TryGetValue(identifier, out VariableInfo rootVariable))
                rootVariable.Redeclared = true;
        }
    }

    public override void ExitShortVarDecl(GoParser.ShortVarDeclContext context)
    {
        if (!Identifiers.TryGetValue(context.identifierList(), out string[] identifiers))
            return;
            
        if (!ExpressionLists.TryGetValue(context.expressionList(), out ExpressionInfo[] expressions))
            return;

        for (int i = 0; i < identifiers.Length; i++)
        {
            string identifier = identifiers[i];

            if (identifier.Equals("_"))
                continue;

            if (expressions.Length <= i)
                continue;

            string uniqueIdentifer = GetUniqueIdentifier(m_variables, identifiers[i]);

            // TODO: This is not enough to detect redeclaraion use cases where say only two for loops use the same variable
            bool redeclared = uniqueIdentifer.Contains("@@");

            m_variables.Add(uniqueIdentifer, new VariableInfo
            {
                Name = identifier,
                Type = expressions[i]?.Type ?? TypeInfo.VarType,
                HeapAllocated = expressions[i]?.Text.StartsWith("&") ?? false,
                Redeclared = redeclared
            });

            if (redeclared && m_variables.TryGetValue(identifier, out VariableInfo rootVariable))
                rootVariable.Redeclared = true;
        }
    }

    public override void ExitRecvStmt(GoParser.RecvStmtContext context)
    {
        CheckIdentifiers(context.identifierList(), context.expression());
    }

    public override void ExitRangeClause(GoParser.RangeClauseContext context)
    {
        CheckIdentifiers(context.identifierList(), context.expression());
    }

    private void CheckIdentifiers(GoParser.IdentifierListContext identifierList, GoParser.ExpressionContext expression)
    {
        if (!Identifiers.TryGetValue(identifierList, out string[] identifiers))
            return;

        Expressions.TryGetValue(expression, out ExpressionInfo expressionInfo);

        for (int i = 0; i < identifiers.Length; i++)
        {
            string identifier = identifiers[i];

            if (identifier.Equals("_"))
                continue;

            string uniqueIdentifer = GetUniqueIdentifier(m_variables, identifiers[i]);

            // TODO: This is not enough to detect redeclaraion use cases where say only two for loops use the same variable
            bool redeclared = uniqueIdentifer.Contains("@@");

            m_variables.Add(uniqueIdentifer, new VariableInfo
            {
                Name = identifier,
                Type = expressionInfo?.Type ?? TypeInfo.VarType,
                HeapAllocated = expressionInfo?.Text.StartsWith("&") ?? false,
                Redeclared = redeclared
            });

            if (redeclared && m_variables.TryGetValue(identifier, out VariableInfo rootVariable))
                rootVariable.Redeclared = true;
        }
    }

    public override void ExitTypeSwitchGuard(GoParser.TypeSwitchGuardContext context)
    {
        if (context.IDENTIFIER() is null)
            return;

        string identifier = context.IDENTIFIER().GetText();

        if (identifier.Equals("_"))
            return;

        Expressions.TryGetValue(context.primaryExpr(), out ExpressionInfo expression);

        string uniqueIdentifer = GetUniqueIdentifier(m_variables, identifier);

        // TODO: This is not enough to detect redeclaraion use cases where say only two for loops use the same variable
        bool redeclared = uniqueIdentifer.Contains("@@");

        m_variables.Add(uniqueIdentifer, new VariableInfo
        {
            Name = identifier,
            Type = expression?.Type ?? TypeInfo.VarType,
            HeapAllocated = expression?.Text.StartsWith("&") ?? false,
            Redeclared = redeclared
        });

        if (redeclared && m_variables.TryGetValue(identifier, out VariableInfo rootVariable))
            rootVariable.Redeclared = true;
    }

    // TODO: Look for cases where a pointer is assigned to an address of a variable,
    //       in these cases the variable should be marked as HeapAllocated

    public override void ExitAssignment(GoParser.AssignmentContext context)
    {
        if (context.assign_op().GetText() == "=")
        {
            // Check for pointer reference
        }
    }

    public override void ExitDeferStmt(GoParser.DeferStmtContext context)
    {
        m_hasDefer = true;
    }

    public override void ExitExpressionStmt(GoParser.ExpressionStmtContext context)
    {
        // TODO: Better to directly find tokens in PrimaryExpressions tree values
        string expression = context.expression().GetText();

        if (expression.StartsWith("panic("))
            m_hasPanic = true;

        if (expression.StartsWith("recover("))
            m_hasRecover = true;
    }
}

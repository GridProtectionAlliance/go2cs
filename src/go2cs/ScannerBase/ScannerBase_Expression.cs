//******************************************************************************************************
//  ScannerBase_Expression.cs - Gbtc
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

namespace go2cs;

public partial class ScannerBase
{
    // Stack handlers:
    //  expressionList (required)
    //  expressionStmt (required)
    //  sendStmt (required)
    //  incDecStmt (required)
    //  defer (required)
    //  ifStmt (required)
    //  exprSwitchStmt (optional)
    //  recvStmt (required)
    //  forClause (optional)
    //  rangeClause (required)
    //  goStmt (required)
    //  arrayLength (required)
    //  operand (optional)
    //  key (optional)
    //  element (optional)
    //  index (required)
    //  slice (optional)
    //  expression (optional)
    //  conversion (required)
    protected readonly ParseTreeValues<ExpressionInfo> Expressions = new();
    protected readonly ParseTreeValues<ExpressionInfo> UnaryExpressions = new();
    protected readonly ParseTreeValues<ExpressionInfo> PrimaryExpressions = new();
    protected readonly ParseTreeValues<ExpressionInfo> Operands = new();

    private static readonly HashSet<string> s_comparisionOperands = new()
    {
        "==", "!=", "<", "<=", ">=", "&&", "||"
    };

    public override void ExitExpression(GoParser.ExpressionContext context)
    {
        // expression
        //     : primaryExpr
        //     | unaryExpr
        //     | expression('*' | '/' | '%' | '<<' | '>>' | '&' | '&^') expression
        //     | expression('+' | '-' | '|' | '^') expression
        //     | expression('==' | '!=' | '<' | '<=' | '>' | '>=') expression
        //     | expression '&&' expression
        //     | expression '||' expression

        if (context.expression()?.Length == 2)
        {
            string leftOperand = Expressions.TryGetValue(context.expression(0), out ExpressionInfo leftOperandExpression) ? leftOperandExpression.Text : context.expression(0).GetText();
            string rightOperand = Expressions.TryGetValue(context.expression(1), out ExpressionInfo rightOperandExpression) ? rightOperandExpression.Text : context.expression(1).GetText();
            string binaryOP = context.children[1].GetText();

            if (binaryOP.Equals("<<") || binaryOP.Equals(">>"))
            {
                if (!int.TryParse(rightOperand, out int _))
                    rightOperand = $"(int)({rightOperand})";
            }

            binaryOP = binaryOP.Equals("&^") ? " & ~" : $" {binaryOP} ";

            string expression = $"{leftOperand}{binaryOP}{rightOperand}";

            if (s_comparisionOperands.Contains(binaryOP))
            {
                Expressions[context] = new()
                {
                    Text = expression,
                    Type = new()
                    {
                        Name = "bool",
                        TypeName = "bool",
                        FullTypeName = "System.Boolean",
                        TypeClass = TypeClass.Simple,
                        IsConst = true
                    }
                };
            }
            else
            {
                // TODO: If both operands are integer, expression should be treated as arbitrary-precision numbers until assigned to a variable
                Expressions[context] = new()
                {
                    Text = expression,
                    Type = leftOperandExpression.Type ?? TypeInfo.VarType
                };
            }
        }
        else
        {
            if (context.primaryExpr() is not null)
            {
                if (PrimaryExpressions.TryGetValue(context.primaryExpr(), out ExpressionInfo primaryExpression))
                    Expressions[context] = primaryExpression;
                else
                    AddWarning(context, $"Failed to find primary expression \"{context.primaryExpr().GetText()}\" in the expression \"{context.GetText()}\"");
            }
            else if (context.unaryExpr() is not null)
            {
                if (UnaryExpressions.TryGetValue(context.unaryExpr(), out ExpressionInfo unaryExpression))
                    Expressions[context] = unaryExpression;
                else
                    AddWarning(context, $"Failed to find unary expression \"{context.unaryExpr().GetText()}\" in the expression \"{context.GetText()}\"");
            }
            else
            {
                AddWarning(context, $"Unexpected expression \"{context.GetText()}\"");
            }
        }
    }

    public override void ExitUnaryExpr(GoParser.UnaryExprContext context)
    {
        // unaryExpr
        //     : primaryExpr
        //     | ('+' | '-' | '!' | '^' | '*' | '&' | '<-') expression

        if (PrimaryExpressions.TryGetValue(context.primaryExpr(), out ExpressionInfo primaryExpression))
        {
            UnaryExpressions[context] = primaryExpression;
        }
        else if (context.expression() is not null)
        {
            if (Expressions.TryGetValue(context.expression(), out ExpressionInfo expression))
            {
                string unaryOP = context.children[0].GetText();
                string unaryExpression = string.Empty;
                TypeInfo expressionType = expression.Type;

                if (unaryOP.Equals("^", StringComparison.Ordinal))
                {
                    unaryOP = "~";
                }
                else if (unaryOP.Equals("<-", StringComparison.Ordinal))
                {
                    // TODO: Handle channel value access (update when channel class is created):
                    unaryOP = null;
                    unaryExpression = $"{expression}.Receive()";
                }
                else if (unaryOP.Equals("&", StringComparison.Ordinal))
                {
                    unaryOP = null;

                    if (expression.Text.StartsWith("new ", StringComparison.Ordinal))
                    {
                        unaryExpression = $"addr({expression})";
                        expressionType = new PointerTypeInfo
                        {
                            Name = $"ptr<{expressionType.Name}>",
                            TypeName = $"ptr<{expressionType.Name}>",
                            FullTypeName = $"go.ptr<{expressionType.FullTypeName}>",
                            TypeClass = expressionType.TypeClass,
                            IsDerefPointer = expressionType.IsDerefPointer,
                            IsByRefPointer = expressionType.IsByRefPointer,
                            IsConst = expressionType.IsConst,
                            TargetTypeInfo = expressionType
                        };
                    }
                    else
                    {
                        unaryExpression = $"{AddressPrefix}{expression}";
                    }
                }
                else if (unaryOP.Equals("*", StringComparison.Ordinal))
                {
                    unaryOP = null;

                    if (!expression.Text.EndsWith(".val"))
                        unaryExpression = $"{expression}.val";
                    else
                        unaryExpression = expression.Text;
                }

                if (unaryOP is not null)
                    unaryExpression = $"{unaryOP}{expression}";

                UnaryExpressions[context] = new()
                {
                    Text = unaryExpression,
                    Type = expressionType
                };
            }
            else
            {
                AddWarning(context, $"Unexpected unary expression \"{context.expression().GetText()}\"");
            }
        }
        else if (!UnaryExpressions.ContainsKey(context))
        {
            AddWarning(context, $"Unexpected unary expression \"{context.GetText()}\"");
        }
    }

    public override void ExitPrimaryExpr(GoParser.PrimaryExprContext context)
    {
        string packageImport = $"{PackageImport.Replace('/', '.')}";

        // primaryExpr
        //     : operand
        //     | conversion
        //     | primaryExpr selector
        //     | primaryExpr index
        //     | primaryExpr slice
        //     | primaryExpr typeAssertion
        //     | primaryExpr arguments

        PrimaryExpressions.TryGetValue(context.primaryExpr(), out ExpressionInfo primaryExpression);

        //if (!(primaryExpression is null) && !string.IsNullOrEmpty(primaryExpression.Text))
        //    primaryExpression.Text = SanitizedIdentifier(primaryExpression.Text);

        if (Operands.TryGetValue(context.operand(), out ExpressionInfo operand))
        {
            PrimaryExpressions[context] = new()
            {
                Text = operand.Text,
                Type = operand.Type
            };
        }
        else if (context.conversion() is not null)
        {
            // conversion
            //     : type '(' expression ',' ? ')'

            if (Types.TryGetValue(context.conversion().type_(), out TypeInfo typeInfo) && Expressions.TryGetValue(context.conversion().expression(), out ExpressionInfo expression))
            {
                // TODO: Complex pointer expression may need special handling - could opt for unsafe implementation
                //if (typeInfo.TypeName.StartsWith("*(*"))
                //{
                //    PrimaryExpressions[context] = new ExpressionInfo
                //    {
                //        Text = $"{expression}.Value",
                //        Type = typeInfo
                //    };
                //}
                //else
                    
                if (typeInfo is PointerTypeInfo)
                {
                    PrimaryExpressions[context] = new()
                    {
                        Text = $"new ptr<{typeInfo.TypeName}>({expression})",
                        Type = typeInfo
                    };
                }
                else if (typeInfo.IsDerefPointer)
                {
                    string functionName = typeInfo.TypeName;
                    FunctionInfo functionInfo = null;
                    Metadata?.Functions.TryGetValue($"{functionName}()", out functionInfo);

                    if (functionInfo is not null)
                    {
                        typeInfo = functionInfo.Signature.Signature.Result[0].Type;

                        if (typeInfo is PointerTypeInfo pointerTypeInfo)
                            typeInfo = pointerTypeInfo.TargetTypeInfo;
                    }
                    else
                    {
                        typeInfo = TypeInfo.ObjectType;
                    }

                    PrimaryExpressions[context] = new()
                    {
                        Text = $"{functionName}({expression}).val",
                        Type = typeInfo
                    };
                }
                else if (typeInfo.TypeClass == TypeClass.Struct)
                {
                    PrimaryExpressions[context] = new()
                    {
                        Text = $"{typeInfo.TypeName}_cast({expression})",
                        Type = typeInfo
                    };
                }
                else
                {
                    PrimaryExpressions[context] = new()
                    {
                        Text = $"({typeInfo.TypeName}){expression}",
                        Type = typeInfo
                    };
                }
            }
            else
            {
                AddWarning(context, $"Failed to find type or sub-expression for the conversion expression in \"{context.GetText()}\"");
            }
        }
        else if (context.DOT() is not null)
        {
            // selector
            //     : '.' IDENTIFIER

            string selectionExpression = $"{SanitizedIdentifier(primaryExpression?.Text ??string.Empty)}.{SanitizedIdentifier(context.IDENTIFIER().GetText())}";
            TypeInfo typeInfo = null;

            // TODO: Will need to lookup IDENTIFIER type in metadata to determine type
            if (primaryExpression is null || primaryExpression.Type.FullTypeName == "System.Object")
                typeInfo = TypeInfo.VarType;

            PrimaryExpressions[context] = new()
            {
                Text = selectionExpression,
                Type = typeInfo ?? primaryExpression?.Type
            };
        }
        else if (context.index() is not null)
        {
            // index
            //     : '[' expression ']'
            // TODO: Will need to lookup IDENTIFIER type in metadata to determine type
            if (Expressions.TryGetValue(context.index().expression(), out ExpressionInfo expression))
            {
                PrimaryExpressions[context] = new()
                {
                    Text = $"{primaryExpression}[{expression}]",
                    Type = primaryExpression.Type
                };
            }
            else
            {
                AddWarning(context, $"Failed to find index expression for \"{context.GetText()}\"");
            }
        }
        else if (context.slice_() is not null)
        {
            // slice
            //     : '['((expression ? ':' expression ? ) | (expression ? ':' expression ':' expression)) ']'

            GoParser.Slice_Context sliceContext = context.slice_();

            if (sliceContext.children.Count == 3)
            {
                // primaryExpr[:]
                PrimaryExpressions[context] = new()
                {
                    Text = $"{primaryExpression}[..]",
                    Type = primaryExpression.Type
                };
            }
            else if (sliceContext.children.Count == 4)
            {
                bool expressionIsLeft = sliceContext.children[1] is GoParser.ExpressionContext;

                // primaryExpr[low:] or primaryExpr[:high]
                if (Expressions.TryGetValue(sliceContext.expression(0), out ExpressionInfo expression))
                {
                    PrimaryExpressions[context] = new()
                    {
                        Text = $"{primaryExpression}[{(expressionIsLeft ? $"{(expression.Type?.TypeName == "int" ? string.Empty : "(int)")}{expression}..": $"..{(expression.Type?.TypeName == "int" ? string.Empty : "(int)")}{expression}")}]",
                        Type = primaryExpression.Type
                    };
                }
                else
                {
                    AddWarning(context, $"Failed to find slice expression for \"{context.GetText()}\"");
                }
            }
            else if (sliceContext.children.Count == 5)
            {
                if (sliceContext.children[1] is GoParser.ExpressionContext && sliceContext.children[3] is GoParser.ExpressionContext)
                {
                    // primaryExpr[low:high]
                    if (Expressions.TryGetValue(sliceContext.expression(0), out ExpressionInfo lowExpression) && Expressions.TryGetValue(sliceContext.expression(1), out ExpressionInfo highExpression))
                    {
                        PrimaryExpressions[context] = new()
                        {
                            Text = $"{primaryExpression}[{(lowExpression.Type?.TypeName == "int" ? string.Empty : "(int)")}{lowExpression}..{(highExpression.Type?.TypeName == "int" ? string.Empty : "(int)")}{highExpression}]",
                            Type = primaryExpression.Type
                        };
                    }
                    else
                    {
                        AddWarning(context, $"Failed to find one of the slice expressions for \"{context.GetText()}\"");
                    }
                }
                else
                {
                    AddWarning(context, $"Failed to find slice expression for \"{context.GetText()}\"");
                }
            }
            else if (sliceContext.children.Count == 6)
            {
                // primaryExpr[:high:max]
                if (Expressions.TryGetValue(sliceContext.expression(0), out ExpressionInfo highExpression) && Expressions.TryGetValue(sliceContext.expression(1), out ExpressionInfo maxExpression))
                {
                    PrimaryExpressions[context] = new()
                    {
                        Text = $"{primaryExpression}.slice(-1, {highExpression}, {maxExpression})",
                        Type = primaryExpression.Type
                    };
                }
                else
                {
                    AddWarning(context, $"Failed to find one of the slice expressions for \"{context.GetText()}\"");
                }
            }
            else if (sliceContext.children.Count == 7)
            {
                // primaryExpr[low:high:max]
                if (Expressions.TryGetValue(sliceContext.expression(0), out ExpressionInfo lowExpression) && Expressions.TryGetValue(sliceContext.expression(1), out ExpressionInfo highExpression) && Expressions.TryGetValue(sliceContext.expression(2), out ExpressionInfo maxExpression))
                {
                    PrimaryExpressions[context] = new()
                    {
                        Text = $"{primaryExpression}.slice({lowExpression}, {highExpression}, {maxExpression})",
                        Type = primaryExpression.Type
                    };
                }
                else
                {
                    AddWarning(context, $"Failed to find one of the slice expressions for \"{context.GetText()}\"");
                }
            }
        }
        else if (context.typeAssertion() is not null)
        {
            // typeAssertion
            //     : '.' '(' type ')'

            if (Types.TryGetValue(context.typeAssertion().type_(), out TypeInfo typeInfo))
            {
                PrimaryExpressions[context] = new()
                {
                    Text = $"{primaryExpression}._<{typeInfo.TypeName}>()",
                    Type = typeInfo
                };
            }
            else
            {
                AddWarning(context, $"Failed to find type for the type assertion expression in \"{context.GetText()}\"");
            }
        }
        else if (context.arguments() is not null)
        {
            // arguments
            //     : '('((expressionList | type(',' expressionList) ? ) '...' ? ',' ? ) ? ')'

            GoParser.ArgumentsContext argumentsContext = context.arguments();
            List<string> arguments = new();

            Types.TryGetValue(argumentsContext.type_(), out TypeInfo typeInfo);

            // Attempt to lookup expression with arguments as a "function"
            ParameterInfo[] parameters = null;
            string functionName = primaryExpression.Text;
            FunctionInfo functionInfo = null;

            // TODO: Need to lookup functions from imported libraries as well
            Metadata?.Functions.TryGetValue($"{functionName}()", out functionInfo);

            if (functionInfo is not null)
                parameters = functionInfo.Signature.Signature.Parameters;

            if (ExpressionLists.TryGetValue(argumentsContext.expressionList(), out ExpressionInfo[] expressions))
            {
                if (InFunction && CurrentFunction is not null)
                {
                    for (int i = 0; i < expressions.Length; i++)
                    {
                        ExpressionInfo expression = expressions[i];
                        ParameterInfo parameter = parameters?.Length > i ? parameters[i] : null;
                        CurrentFunction.Variables.TryGetValue(expression.Text, out VariableInfo variable);

                        if (parameter?.Type is PointerTypeInfo && expression.Type is not PointerTypeInfo && variable?.Type is not PointerTypeInfo && !expression.Text.StartsWith(AddressPrefix, StringComparison.Ordinal))
                            arguments.Add($"{AddressPrefix}{expression}");
                        else
                            arguments.Add(expression.Text);
                    }
                }
                else
                {
                    arguments.AddRange(expressions.Select(expression => expression.Text));
                }
            }

            string argumentList = string.Join(", ", arguments);

            if (primaryExpression.Text == "new")
            {
                if (typeInfo is null)
                {
                    string typeName = expressions?[0].Text;
                    TypeInfo argType = null;

                    foreach (TypeInfo typeInfoValue in Types.Values)
                    {
                        if (typeInfoValue.TypeName.Equals(typeName))
                        {
                            argType = typeInfoValue.Clone();
                            break;
                        }

                        if (typeInfoValue.TypeName.Equals($"{packageImport}.{typeName}"))
                        {
                            argType = typeInfoValue.Clone();
                            break;
                        }

                        foreach (string import in Imports)
                        {
                            if (typeInfoValue.TypeName.Equals($"{import}.{typeName}"))
                            {
                                argType = typeInfoValue.Clone();
                                break;
                            }
                        }

                        if (argType is not null)
                            break;
                            
                        if (typeInfoValue.Name.Equals(typeName))
                        {
                            argType = typeInfoValue.Clone();
                            break;
                        }
                    }

                    if (argType == null)
                        argType = TypeInfo.ObjectType.Clone();

                    argType = new PointerTypeInfo
                    {
                        Name = $"ptr<{argType.Name}>",
                        TypeName = $"ptr<{argType.Name}>",
                        FullTypeName = $"go.ptr<{argType.FullTypeName}>",
                        TypeClass = argType.TypeClass,
                        IsDerefPointer = argType.IsDerefPointer,
                        IsByRefPointer = argType.IsByRefPointer,
                        IsConst = argType.IsConst,
                        TargetTypeInfo = argType
                    };

                    PrimaryExpressions[context] = new() { Text = $"@new<{typeName}>()", Type = argType };
                }
                else
                {
                    TypeInfo argType = expressions?[0].Type;

                    if (argType is not PointerTypeInfo)
                    {
                        argType = expressions?[0].Type.Clone() ?? TypeInfo.VarType.Clone();

                        argType = new PointerTypeInfo
                        {
                            Name = $"ptr<{argType.Name}>",
                            TypeName = $"ptr<{argType.Name}>",
                            FullTypeName = $"go.ptr<{argType.FullTypeName}>",
                            TypeClass = argType.TypeClass,
                            IsDerefPointer = argType.IsDerefPointer,
                            IsByRefPointer = argType.IsByRefPointer,
                            IsConst = argType.IsConst,
                            TargetTypeInfo = argType
                        };
                    }

                    PrimaryExpressions[context] = new() { Text = $"@new<{typeInfo.Name}>({argumentList})", Type = argType };
                }
            }
            else if (primaryExpression.Text == "make" && typeInfo is not null)
            {
                PrimaryExpressions[context] = typeInfo.TypeClass switch
                {
                    TypeClass.Slice => new()
                    {
                        Text = $"make_slice<{RemoveSurrounding(typeInfo.TypeName, "slice<", ">")}>({argumentList})",
                        Type = primaryExpression.Type
                    },
                    TypeClass.Map => new()
                    {
                        Text = $"make_map<{RemoveSurrounding(typeInfo.TypeName, "map<", ">")}>({argumentList})",
                        Type = primaryExpression.Type
                    },
                    TypeClass.Channel => new()
                    {
                        Text = $"make_channel<{RemoveSurrounding(typeInfo.TypeName, "channel<", ">")}>({argumentList})",
                        Type = primaryExpression.Type
                    },
                    _ => new()
                    {
                        Text = $"{primaryExpression}<{typeInfo.TypeName}>({argumentList})",
                        Type = primaryExpression.Type
                    },
                };
            }
            else
            {
                if (typeInfo is not null)
                    argumentList = $"typeof({typeInfo.TypeName}){(string.IsNullOrEmpty(argumentList) ? string.Empty : $", {argumentList}")}";

                PrimaryExpressions[context] = new()
                {
                    Text = $"{primaryExpression}({argumentList})",
                    Type = primaryExpression.Type
                };
            }
        }
        else
        {
            AddWarning(context, $"Unexpected primary expression \"{context.GetText()}\"");
        }
    }

    public override void ExitOperand(GoParser.OperandContext context)
    {
        // operand
        //     : literal
        //     | operandName
        //     | methodExpr
        //     | '(' expression ')'

        if (Expressions.TryGetValue(context.expression(), out ExpressionInfo expression))
        {
            Operands[context] = new()
            {
                Text = $"({expression})",
                Type = expression.Type
            };
        }

        // Remaining operands contexts handled below...
    }

    public override void ExitBasicLit(GoParser.BasicLitContext context)
    {
        // operand
        //     : literal
        //     | operandName
        //     | methodExpr
        //     | '(' expression ')'

        // literal
        //     : basicLit
        //     | compositeLit
        //     | functionLit

        if (context.Parent.Parent is not GoParser.OperandContext operandContext)
        {
            AddWarning(context, $"Could not derive parent operand context from basic literal: \"{context.GetText()}\"");
            return;
        }

        string basicLiteral;
        TypeInfo typeInfo;

        // basicLit
        //     : INT_LIT
        //     | FLOAT_LIT
        //     | IMAGINARY_LIT
        //     | RUNE_LIT
        //     | STRING_LIT

        if (context.IMAGINARY_LIT() is not null)
        {
            string value = context.IMAGINARY_LIT().GetText();
            bool endsWith_i = value.EndsWith("i");
            value = endsWith_i ? value[..^1] : value;

            if (float.TryParse(value, out _))
            {
                basicLiteral = endsWith_i ? $"i({value}F)" : $"{value}F";

                typeInfo = new()
                {
                    Name = "complex64",
                    TypeName = "complex64",
                    FullTypeName = "go.complex64",
                    TypeClass = TypeClass.Simple
                };
            }
            else
            {
                basicLiteral = endsWith_i ? $"i({value}D)" : $"{value}D";

                typeInfo = new()
                {
                    Name = "Complex",
                    TypeName = "Complex",
                    FullTypeName = "System.Numerics.Complex",
                    TypeClass = TypeClass.Simple
                };
            }
        }
        else if (context.FLOAT_LIT() is not null)
        {
            basicLiteral = context.GetText();

            if (float.TryParse(basicLiteral, out _))
            {
                basicLiteral += "F";

                typeInfo = new()
                {
                    Name = "float",
                    TypeName = "float",
                    FullTypeName = "System.Single",
                    TypeClass = TypeClass.Simple,
                    IsConst = true
                };
            }
            else
            {
                basicLiteral += "D";

                typeInfo = new()
                {
                    Name = "double",
                    TypeName = "double",
                    FullTypeName = "System.Double",
                    TypeClass = TypeClass.Simple,
                    IsConst = true
                };
            }
        }
        else if (context.integer() is not null)
        {
            basicLiteral = ReplaceOctalBytes(context.integer().GetText());

            if (context.integer().RUNE_LIT() is not null)
            {
                typeInfo = new()
                {
                    Name = "char",
                    TypeName = "char",
                    FullTypeName = "System.Char",
                    TypeClass = TypeClass.Simple,
                    IsConst = true
                };
            }
            else
            {
                if (nint.TryParse(basicLiteral, out nint val))
                {
                    if (val > int.MaxValue)
                        basicLiteral = $"(nint){basicLiteral}L";

                    typeInfo = new()
                    {
                        Name = "nint",
                        TypeName = "nint",
                        FullTypeName = "nint",
                        TypeClass = TypeClass.Simple,
                        IsConst = true
                    };
                }
                else
                {
                    if (nuint.TryParse(basicLiteral, out nuint uval) && uval > uint.MaxValue)
                        basicLiteral = $"(nuint){basicLiteral}UL";

                    typeInfo = new()
                    {
                        Name = "nuint",
                        TypeName = "nuint",
                        FullTypeName = "nuint",
                        TypeClass = TypeClass.Simple,
                        IsConst = true
                    };
                }
            }
        }
        else if (context.RUNE_LIT() is not null)
        {
            basicLiteral = ReplaceOctalBytes(context.RUNE_LIT().GetText());

            typeInfo = new()
            {
                Name = "char",
                TypeName = "char",
                FullTypeName = "System.Char",
                TypeClass = TypeClass.Simple,
                IsConst = true
            };
        }
        else if (context.string_() is not null)
        {
            basicLiteral = ToStringLiteral(ReplaceOctalBytes(context.string_().GetText()));

            typeInfo = new()
            {
                Name = "@string",
                TypeName = "@string",
                FullTypeName = "go.@string",
                TypeClass = TypeClass.Simple,
                IsConst = true
            };
        }
        else if (context.NIL_LIT() is not null)
        {
            basicLiteral = "null";
            typeInfo = TypeInfo.ObjectType;
        }
        else
        {
            AddWarning(context, $"Unexpected basic literal: \"{context.GetText()}\"");
            return;
        }

        Operands[operandContext] = new()
        {
            Text = basicLiteral,
            Type = typeInfo
        };
    }

    public override void ExitCompositeLit(GoParser.CompositeLitContext context)
    {
        // literal
        //     : basicLit
        //     | compositeLit
        //     | functionLit

        // compositeLit
        //    : literalType literalValue

        if (context.Parent.Parent is not GoParser.OperandContext operandContext)
        {
            AddWarning(context, $"Could not derive parent operand context from composite literal: \"{context.GetText()}\"");
            return;
        }

        GoParser.LiteralTypeContext literalType = context.literalType();
        GoParser.LiteralValueContext literalValue = context.literalValue();
        GoParser.KeyedElementContext[] keyedElements = literalValue.elementList()?.keyedElement();
        bool isDynamicSizedArray = literalType.elementType() is not null;
        List<(string key, string element)> elements = new();
        bool hasKeyedElement = false;

        if (keyedElements is not null)
        {
            foreach (GoParser.KeyedElementContext keyedElement in keyedElements)
            {
                string key = keyedElement.key()?.GetText();
                string element = keyedElement.element().GetText();

                elements.Add((key, element));

                if (key is not null && !hasKeyedElement)
                    hasKeyedElement = true;
            }
        }

        string expressionText;
        TypeInfo typeInfo;

        if (literalType.structType() is not null)
        {
            // TODO: Need to properly handle in-line struct, see "src\Examples\Manual Tour of Go Conversions\moretypes\slice-literals"
            expressionText = $"/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ {context.GetText()}";
            typeInfo = new()
            {
                Name = literalType.GetText(),
                TypeName = literalType.GetText(),
                FullTypeName = literalType.GetText(),
                TypeClass = TypeClass.Struct
            };
        }
        else if (literalType.arrayType() is not null || isDynamicSizedArray)
        {
            if (Types.TryGetValue(literalType.arrayType()?.elementType() ?? literalType.elementType(), out typeInfo))
            {
                if (typeInfo?.TypeClass == TypeClass.Interface)
                {
                    for (int i = 0; i < elements.Count; i++)
                    {
                        (string key, string element) = elements[i];
                        elements[i] = (key, $"{typeInfo.TypeName}.As({element})!");
                    }
                }

                string typeName = typeInfo?.TypeName ?? "object";
                string arrayLength = isDynamicSizedArray ? "-1" : literalType.arrayType().arrayLength().GetText();

                expressionText = hasKeyedElement ? 
                    $"new array<{typeName}>(InitKeyedValues<{typeName}>({(isDynamicSizedArray ? string.Empty: $"{arrayLength}, ")}{string.Join(", ", elements.Select(kvp => kvp.key is null ? kvp.element : $"({kvp.key}, {kvp.element})"))}))" :
                    $"new array<{typeName}>(new {typeName}[] {{ {string.Join(", ", elements.Select(kvp => kvp.element))} }})";

                typeInfo = new ArrayTypeInfo
                {
                    Name = typeName,
                    TypeName = $"array<{typeName}>",
                    FullTypeName = $"go.array<{typeInfo.FullTypeName}>",
                    TargetTypeInfo = typeInfo,
                    Length = new()
                    {
                        Text = arrayLength,
                        Type = new()
                        {
                            Name = "nint",
                            TypeName = "nint",
                            FullTypeName = "nint",
                            TypeClass = TypeClass.Simple,
                            IsConst = true
                        }
                    },
                    TypeClass = TypeClass.Array
                };
            }
            else
            {
                AddWarning(context, $"Failed to find element type for the array type expression in \"{context.GetText()}\"");
                return;
            }
        }
        else if (literalType.sliceType() is not null)
        {
            if (Types.TryGetValue(literalType.sliceType().elementType(), out typeInfo))
            {
                string typeName = typeInfo.TypeName;

                if (typeInfo.TypeClass == TypeClass.Interface)
                {
                    for (int i = 0; i < elements.Count; i++)
                    {
                        (string key, string element) = elements[i];
                        elements[i] = (key, $"{typeInfo.TypeName}.As({element})!");
                    }
                }

                expressionText = hasKeyedElement ?
                    $"new slice<{typeName}>(InitKeyedValues<{typeName}>({string.Join(", ", elements.Select(kvp => kvp.key is null ? kvp.element : $"({kvp.key}, {kvp.element})"))}))" :
                    $"new slice<{typeName}>(new {typeName}[] {{ {string.Join(", ", elements.Select(kvp => kvp.element))} }})";

                typeInfo = new()
                {
                    Name = typeName,
                    TypeName = typeName,
                    FullTypeName = typeInfo.FullTypeName,
                    TypeClass = TypeClass.Slice
                };
            }
            else
            {
                AddWarning(context, $"Failed to find element type for the slice type expression in \"{context.GetText()}\"");
                return;
            }
        }
        else if (literalType.mapType() is not null)
        {
            // TODO: Need to properly handle map literals, see "src\Examples\Manual Tour of Go Conversions\moretypes\map-literals-continued"
            if (Types.TryGetValue(literalType.mapType().type_(), out typeInfo) && Types.TryGetValue(literalType.mapType().elementType(), out TypeInfo elementTypeInfo))
            {
                expressionText = $"/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<{typeInfo.TypeName}, {elementTypeInfo.TypeName}>{literalValue.GetText()}";
                    
                typeInfo = new MapTypeInfo
                {
                    Name = "map",
                    TypeName = "map",
                    FullTypeName = "go.map",
                    KeyTypeInfo = typeInfo,
                    ElementTypeInfo = elementTypeInfo,
                    TypeClass = TypeClass.Map
                };
            }
            else
            {
                AddWarning(context, $"Failed to find key and/or value type for the map type expression in \"{context.GetText()}\"");
                return;
            }
        }
        else if (literalType.typeName() is not null)
        {
            // TODO: Need to determine how to properly employ keyed elements here - guess is type aliases to array/slice/map would need to map back to original implementations
            expressionText = $"new {literalType.GetText()}({RemoveSurrounding(literalValue.GetText(), "{", "}")})";
                
            typeInfo = new()
            {
                Name = literalType.GetText(),
                TypeName = literalType.GetText(),
                FullTypeName = $"go.{literalType.GetText()}",
                TypeClass = TypeClass.Simple
            };
        }
        else
        {
            AddWarning(context, $"Unexpected literal type \"{context.GetText()}\"");
            return;
        }

        Operands[operandContext] = new()
        {
            Text = expressionText,
            Type = typeInfo
        };
    }

    public override void ExitFunctionLit(GoParser.FunctionLitContext context)
    {
        // operand
        //     : literal
        //     | operandName
        //     | methodExpr
        //     | '(' expression ')'

        // literal
        //     : basicLit
        //     | compositeLit
        //     | functionLit

        if (context.Parent.Parent is not GoParser.OperandContext operandContext)
        {
            AddWarning(context, $"Could not derive parent operand context from function literal: \"{context.GetText()}\"");
            return;
        }

        // functionLit
        //     : 'func' function

        // This is a place-holder for base class - derived classes, e.g., Converter, have to properly handle function content
        Operands[operandContext] = new()
        {
            Text = SanitizedIdentifier(context.GetText()),
            Type = new()
            {
                Name = "Action",
                TypeName = "Action",
                FullTypeName = "System.Action",
                TypeClass = TypeClass.Function
            }
        };
    }

    public override void ExitOperandName(GoParser.OperandNameContext context)
    {
        // operand
        //     : literal
        //     | operandName
        //     | methodExpr
        //     | '(' expression ')'

        if (context.Parent is not GoParser.OperandContext operandContext)
        {
            AddWarning(context, $"Could not derive parent operand context from operand name: \"{context.GetText()}\"");
            return;
        }

        // operandName
        //     : IDENTIFIER
        //     | qualifiedIdent

        // TODO: var assignment is temporary, to resolve actual type, converter would override to load identifier metadata and recursively resolve identifier based components
        Operands[operandContext] = new()
        {
            Text = context.GetText(),
            Type = TypeInfo.VarType
        };
    }

    public override void ExitMethodExpr([NotNull] GoParser.MethodExprContext context)
    {
        // operand
        //     : literal
        //     | operandName
        //     | methodExpr
        //     | '(' expression ')'

        if (context.Parent is not GoParser.OperandContext operandContext)
        {
            AddWarning(context, $"Could not derive parent operand context from method expression: \"{context.GetText()}\"");
            return;
        }

        // methodExpr
        //     : receiverType '.' IDENTIFIER

        // receiverType
        //     : typeName
        //     | '(' '*' typeName ')'
        //     | '(' receiverType ')'

        GoParser.ReceiverTypeContext receiverType = context.receiverType();

        // TODO: should this be a delegate to an extension function? Need a use case...
        string receiver;

        if (receiverType?.children.Count == 4)
            receiver = $"ptr<{receiverType.typeName().GetText()}>";
        else
            receiver = context.GetText();

        Operands[operandContext] = new()
        {
            Text = receiver,
            Type = TypeInfo.ObjectType
        };
    }
}

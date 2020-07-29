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

namespace go2cs
{
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
        protected readonly ParseTreeValues<ExpressionInfo> Expressions = new ParseTreeValues<ExpressionInfo>();
        protected readonly ParseTreeValues<ExpressionInfo> UnaryExpressions = new ParseTreeValues<ExpressionInfo>();
        protected readonly ParseTreeValues<ExpressionInfo> PrimaryExpressions = new ParseTreeValues<ExpressionInfo>();
        protected readonly ParseTreeValues<ExpressionInfo> Operands = new ParseTreeValues<ExpressionInfo>();

        private static readonly HashSet<string> s_comparisionOperands = new HashSet<string>
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
                ExpressionInfo leftOperand = Expressions[context.expression(0)];
                ExpressionInfo rightOperand = Expressions[context.expression(1)];
                string binaryOP = context.children[1].GetText();

                if (binaryOP.Equals("<<") || binaryOP.Equals(">>"))
                {
                    if (!int.TryParse(rightOperand.Text, out int _))
                        rightOperand.Text = $"(int)({rightOperand.Text})";
                }

                binaryOP = binaryOP.Equals("&^") ? " & ~" : $" {binaryOP} ";

                string expression = $"{leftOperand}{binaryOP}{rightOperand}";

                if (s_comparisionOperands.Contains(binaryOP))
                {
                    Expressions[context] = new ExpressionInfo
                    {
                        Text = expression,
                        Type = new TypeInfo
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
                    Expressions[context] = new ExpressionInfo
                    {
                        Text = expression,
                        Type = leftOperand.Type
                    };
                }
            }
            else
            {
                if (!(context.primaryExpr() is null))
                {
                    if (PrimaryExpressions.TryGetValue(context.primaryExpr(), out ExpressionInfo primaryExpression))
                        Expressions[context] = primaryExpression;
                    else
                        AddWarning(context, $"Failed to find primary expression \"{context.primaryExpr().GetText()}\" in the expression \"{context.GetText()}\"");
                }
                else if (!(context.unaryExpr() is null))
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
            else if (!(context.expression() is null))
            {
                if (Expressions.TryGetValue(context.expression(), out ExpressionInfo expression))
                {
                    string unaryOP = context.children[0].GetText();
                    string unaryExpression = "";

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
                        // TODO: Handle pointer acquisition context - may need to augment pre-scan metadata for heap allocation notation
                        unaryOP = null;
                        unaryExpression = $"ref {expression}";
                    }
                    else if (unaryOP.Equals("*", StringComparison.Ordinal))
                    {
                        // TODO: Handle pointer dereference context - if this is a ref variable, Value call is unnecessary
                        unaryOP = null;
                        unaryExpression = $"{expression}.Value";
                    }

                    if (!(unaryOP is null))
                        unaryExpression = $"{unaryOP}{expression}";

                    UnaryExpressions[context] = new ExpressionInfo
                    {
                        Text = unaryExpression,
                        Type = expression.Type
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
                PrimaryExpressions[context] = new ExpressionInfo
                {
                    Text = operand.Text,
                    Type = operand.Type
                };
            }
            else if (!(context.conversion() is null))
            {
                // conversion
                //     : type '(' expression ',' ? ')'

                if (Types.TryGetValue(context.conversion().type_(), out TypeInfo typeInfo) && Expressions.TryGetValue(context.conversion().expression(), out ExpressionInfo expression))
                {
                    if (typeInfo.TypeName.StartsWith("*(*"))
                    {
                        // TODO: Complex pointer expression needs special handling consideration - could opt for unsafe implementation
                        PrimaryExpressions[context] = new ExpressionInfo
                        {
                            Text = $"{expression}.Value",
                            Type = typeInfo
                        };
                    }
                    else
                    {
                        if (typeInfo.IsPointer)
                        {
                            PrimaryExpressions[context] = new ExpressionInfo
                            {
                                Text = $"new ptr<{typeInfo.TypeName}>({expression})",
                                Type = typeInfo
                            };
                        }
                        else if (typeInfo.TypeClass == TypeClass.Struct)
                        {
                            PrimaryExpressions[context] = new ExpressionInfo
                            {
                                Text = $"{typeInfo.TypeName}_cast({expression})",
                                Type = typeInfo
                            };
                        }
                        else
                        {
                            PrimaryExpressions[context] = new ExpressionInfo
                            {
                                Text = $"({typeInfo.TypeName}){expression}",
                                Type = typeInfo
                            };
                        }
                    }
                }
                else
                {
                    AddWarning(context, $"Failed to find type or sub-expression for the conversion expression in \"{context.GetText()}\"");
                }
            }
            else if (!(context.DOT() is null))
            {
                // selector
                //     : '.' IDENTIFIER

                string selectionExpression = $"{primaryExpression.Text}.{SanitizedIdentifier(context.IDENTIFIER().GetText())}";
                TypeInfo typeInfo = null;

                // TODO: Will need to lookup IDENTIFIER type in metadata to determine type
                if (primaryExpression.Type.FullTypeName == "System.Object")
                    typeInfo = TypeInfo.VarType;

                PrimaryExpressions[context] = new ExpressionInfo
                {
                    Text = selectionExpression,
                    Type = typeInfo ?? primaryExpression.Type
                };
            }
            else if (!(context.index() is null))
            {
                // index
                //     : '[' expression ']'
                // TODO: Will need to lookup IDENTIFIER type in metadata to determine type
                if (Expressions.TryGetValue(context.index().expression(), out ExpressionInfo expression))
                {
                    PrimaryExpressions[context] = new ExpressionInfo
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
            else if (!(context.slice() is null))
            {
                // slice
                //     : '['((expression ? ':' expression ? ) | (expression ? ':' expression ':' expression)) ']'

                GoParser.SliceContext sliceContext = context.slice();

                if (sliceContext.children.Count == 3)
                {
                    // primaryExpr[:]
                    PrimaryExpressions[context] = new ExpressionInfo
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
                        PrimaryExpressions[context] = new ExpressionInfo
                        {
                            Text = $"{primaryExpression}[{(expressionIsLeft ? $"{expression}..": $"..{expression}")}]",
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
                            PrimaryExpressions[context] = new ExpressionInfo
                            {
                                Text = $"{primaryExpression}[{lowExpression}..{highExpression}]",
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
                        PrimaryExpressions[context] = new ExpressionInfo
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
                        PrimaryExpressions[context] = new ExpressionInfo
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
            else if (!(context.typeAssertion() is null))
            {
                // typeAssertion
                //     : '.' '(' type ')'

                if (Types.TryGetValue(context.typeAssertion().type_(), out TypeInfo typeInfo))
                {
                    PrimaryExpressions[context] = new ExpressionInfo
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
            else if (!(context.arguments() is null))
            {
                // arguments
                //     : '('((expressionList | type(',' expressionList) ? ) '...' ? ',' ? ) ? ')'

                GoParser.ArgumentsContext argumentsContext = context.arguments();
                List<string> arguments = new List<string>();

                if (Types.TryGetValue(argumentsContext.type_(), out TypeInfo typeInfo))
                    arguments.Add($"typeof({typeInfo.TypeName})");

                if (ExpressionLists.TryGetValue(argumentsContext.expressionList(), out ExpressionInfo[] expressions))
                    arguments.AddRange(expressions.Select(expr => expr.Text));

                PrimaryExpressions[context] = new ExpressionInfo
                {
                    Text = $"{primaryExpression}({string.Join(", ", arguments)})",
                    Type = primaryExpression.Type
                };
            }
            else
            {
                AddWarning(context, $"Unexpected primary expression \"{context.GetText()}\"");
            }
        }

        public override void ExitOperand( GoParser.OperandContext context)
        {
            // operand
            //     : literal
            //     | operandName
            //     | methodExpr
            //     | '(' expression ')'

            if (Expressions.TryGetValue(context.expression(), out ExpressionInfo expression))
            {
                Operands[context] = new ExpressionInfo
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

            if (!(context.Parent.Parent is GoParser.OperandContext operandContext))
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

            if (!(context.IMAGINARY_LIT() is null))
            {
                string value = context.IMAGINARY_LIT().GetText();
                bool endsWith_i = value.EndsWith("i");
                value = endsWith_i ? value.Substring(0, value.Length - 1) : value;

                if (float.TryParse(value, out _))
                {
                    basicLiteral = endsWith_i ? $"i({value}F)" : $"{value}F";

                    typeInfo = new TypeInfo
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

                    typeInfo = new TypeInfo
                    {
                        Name = "Complex",
                        TypeName = "Complex",
                        FullTypeName = "System.Numerics.Complex",
                        TypeClass = TypeClass.Simple
                    };
                }
            }
            else if (!(context.FLOAT_LIT() is null))
            {
                basicLiteral = context.GetText();

                if (float.TryParse(basicLiteral, out _))
                {
                    basicLiteral += "F";

                    typeInfo = new TypeInfo
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

                    typeInfo = new TypeInfo
                    {
                        Name = "double",
                        TypeName = "double",
                        FullTypeName = "System.Double",
                        TypeClass = TypeClass.Simple,
                        IsConst = true
                    };
                }
            }
            else if (!(context.integer() is null))
            {
                basicLiteral = ReplaceOctalBytes(context.integer().GetText());

                if (long.TryParse(basicLiteral, out _))
                {
                    basicLiteral += "L";

                    typeInfo = new TypeInfo
                    {
                        Name = "long",
                        TypeName = "long",
                        FullTypeName = "System.Int64",
                        TypeClass = TypeClass.Simple,
                        IsConst = true
                    };
                }
                else
                {
                    basicLiteral += "UL";

                    typeInfo = new TypeInfo
                    {
                        Name = "ulong",
                        TypeName = "ulong",
                        FullTypeName = "System.UInt64",
                        TypeClass = TypeClass.Simple,
                        IsConst = true
                    };
                }
            }
            else if (!(context.RUNE_LIT() is null))
            {
                basicLiteral = ReplaceOctalBytes(context.RUNE_LIT().GetText());

                typeInfo = new TypeInfo
                {
                    Name = "char",
                    TypeName = "char",
                    FullTypeName = "System.Char",
                    TypeClass = TypeClass.Simple,
                    IsConst = true
                };
            }
            else if (!(context.string_() is null))
            {
                basicLiteral = ToStringLiteral(ReplaceOctalBytes(context.string_().GetText()));

                typeInfo = new TypeInfo
                {
                    Name = "@string",
                    TypeName = "@string",
                    FullTypeName = "go.@string",
                    TypeClass = TypeClass.Simple,
                    IsConst = true
                };
            }
            else if (!(context.NIL_LIT() is null))
            {
                basicLiteral = "null";
                typeInfo = TypeInfo.ObjectType;
            }
            else
            {
                AddWarning(context, $"Unexpected basic literal: \"{context.GetText()}\"");
                return;
            }

            Operands[operandContext] = new ExpressionInfo
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

            if (!(context.Parent.Parent is GoParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from composite literal: \"{context.GetText()}\"");
                return;
            }

            GoParser.LiteralTypeContext literalType = context.literalType();
            GoParser.LiteralValueContext literalValue = context.literalValue();

            string expressionText;
            TypeInfo typeInfo;

            if (!(literalType.structType() is null))
            {
                // TODO: Need to properly handle in-line struct, see "src\Examples\Manual Tour of Go Conversions\moretypes\slice-literals"
                expressionText = $"/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ {context.GetText()}";
                typeInfo = new TypeInfo
                {
                    Name = literalType.GetText(),
                    TypeName = literalType.GetText(),
                    FullTypeName = literalType.GetText(),
                    TypeClass = TypeClass.Struct
                };
            }
            else if (!(literalType.arrayType() is null))
            {
                if (Types.TryGetValue(literalType.arrayType().elementType(), out typeInfo))
                {
                    string typeName = typeInfo.TypeName;
                    expressionText = $"new {typeName}[]{literalValue.GetText()}";
                    typeInfo = new ArrayTypeInfo
                    {
                        Name = typeName,
                        TypeName = typeName,
                        FullTypeName = typeInfo.FullTypeName,
                        Length = new ExpressionInfo
                        {
                            Text = literalType.arrayType().arrayLength().GetText(),
                            Type = new TypeInfo
                            {
                                Name = "long",
                                TypeName = "long",
                                FullTypeName = "System.Int64",
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
            else if(!(literalType.elementType() is null))
            {
                if (Types.TryGetValue(literalType.elementType(), out typeInfo))
                {
                    string typeName = typeInfo.TypeName;
                    expressionText = $"new {typeName}[]{literalValue.GetText()}";
                    typeInfo = new TypeInfo
                    {
                        Name = typeName,
                        TypeName = typeName,
                        FullTypeName = typeInfo.FullTypeName,
                        TypeClass = TypeClass.Array
                    };
                }
                else
                {
                    AddWarning(context, $"Failed to find element type for the array type expression in \"{context.GetText()}\"");
                    return;
                }
            }
            else if (!(literalType.sliceType() is null))
            {
                if (Types.TryGetValue(literalType.sliceType().elementType(), out typeInfo))
                {
                    string typeName = typeInfo.TypeName;
                    expressionText = $"slice(new {typeName}[]{literalValue.GetText()})";
                    typeInfo = new TypeInfo
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
            else if (!(literalType.mapType() is null))
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
            else if (!(literalType.typeName() is null))
            {
                // TODO: Converter will need to override and look at identifier metadata to specify proper expression type
                expressionText = context.GetText();
                typeInfo = TypeInfo.ObjectType;
            }
            else
            {
                AddWarning(context, $"Unexpected literal type \"{context.GetText()}\"");
                return;
            }

            Operands[operandContext] = new ExpressionInfo
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

            if (!(context.Parent.Parent is GoParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from function literal: \"{context.GetText()}\"");
                return;
            }

            // functionLit
            //     : 'func' function

            // This is a place-holder for base class - derived classes, e.g., Converter, have to properly handle function content
            Operands[operandContext] = new ExpressionInfo
            {
                Text = SanitizedIdentifier(context.GetText()),
                Type = new TypeInfo
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

            if (!(context.Parent is GoParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from operand name: \"{context.GetText()}\"");
                return;
            }

            // operandName
            //     : IDENTIFIER
            //     | qualifiedIdent

            // TODO: Converter will need to override and look at identifier metadata to specify proper expression type
            Operands[operandContext] = new ExpressionInfo
            {
                Text = context.GetText(),
                Type = TypeInfo.ObjectType
            };
        }

        public override void ExitMethodExpr([NotNull] GoParser.MethodExprContext context)
        {
            // operand
            //     : literal
            //     | operandName
            //     | methodExpr
            //     | '(' expression ')'

            if (!(context.Parent is GoParser.OperandContext operandContext))
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
                receiver = $"ref {receiverType.typeName().GetText()}";
            else
                receiver = context.GetText();

            Operands[operandContext] = new ExpressionInfo
            {
                Text = receiver,
                Type = TypeInfo.ObjectType
            };
        }
    }
}

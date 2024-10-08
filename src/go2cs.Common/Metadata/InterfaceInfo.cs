﻿//******************************************************************************************************
//  InterfaceInfo.cs - Gbtc
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
//  06/06/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable InconsistentNaming
namespace go2cs.Metadata
{
    [Serializable]
    public class InterfaceInfo
    {
        public string Name;
        public FunctionSignature[] Methods;

        public IEnumerable<FunctionSignature> GetLocalMethods() => 
            Methods.Where(method => !method.IsPromoted);

        public IEnumerable<FunctionSignature> GetInheritedInterfaces() => 
            Methods.Where(method => method.IsPromoted);

        public IEnumerable<string> GetInheritedInterfaceNames() => 
            GetInheritedInterfaces().Select(method => method.Signature.Result[0].Type.TypeName);

        public string GenerateInheritedInterfaceList() => 
            string.Join(", ", GetInheritedInterfaceNames());

        // Built-in error interface is a special case, this is currently the only built-in interface
        public static InterfaceInfo error() => new()
        {
            Name = "error",
            Methods = new[]
            {
                new FunctionSignature
                {
                    Name = "Error",
                    Signature = new()
                    {
                        Parameters = [],
                        Result = new[]
                        {
                            new ParameterInfo
                            {
                                Name = string.Empty,
                                Type = new()
                                {
                                    Name = "string",
                                    TypeName = "@string",
                                    FullTypeName = "go.@string",
                                    TypeClass = TypeClass.Simple
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}

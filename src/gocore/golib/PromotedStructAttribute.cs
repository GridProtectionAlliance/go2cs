//******************************************************************************************************
//  PromotedStructAttribute.cs - Gbtc
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
// ReSharper disable CheckNamespace

using System;

namespace go
{
    /// <summary>
    /// Marks a declared type as having an anonymous promoted structure element.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class PromotedStructAttribute : Attribute
    {
        /// <summary>
        /// Promoted declaration type.
        /// </summary>
        public Type PromotedType { get; }

        /// <summary>
        /// Creates a new <see cref="PromotedStructAttribute"/>.
        /// </summary>
        /// <param name="promotedType">Type of promoted structure.</param>
        public PromotedStructAttribute(Type promotedType) => PromotedType = promotedType;
    }
}
//******************************************************************************************************
//  DictionaryExtensions.cs - Gbtc
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
//  05/25/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace go2cs
{
    /// <summary>
    /// Defines dictionary related helper functions.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Attempts to get the value for the given key and returns the default value instead if the key does not exist in the <see cref="IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to check for the given key.</param>
        /// <param name="key">The key to be checked for the existence of a value.</param>
        /// <returns>The value of the key in the dictionary or the default value if no such value exists.</returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.GetOrDefault(key, k => default);
        }

        /// <summary>
        /// Attempts to get the value for the given key and returns the default value instead if the key does not exist in the <see cref="IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to check for the given key.</param>
        /// <param name="key">The key to be checked for the existence of a value.</param>
        /// <param name="defaultValueFactory">The function used to generate the default value if no value exists for the given key.</param>
        /// <returns>The value of the key in the dictionary or the default value if no such value exists.</returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> defaultValueFactory)
        {
            if (!dictionary.TryGetValue(key, out TValue value))
                value = defaultValueFactory(key);

            return value;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="IDictionary{TKey, TValue}"/> if the key does not already exist.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to add the key/value pair to if the key does not already exist.</param>
        /// <param name="key">The key to be added to the dictionary if it does not already exist.</param>
        /// <param name="valueFactory">The function used to generate a value for the key.</param>
        /// <returns>The value of the key in the dictionary.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!dictionary.TryGetValue(key, out TValue value))
            {
                value = valueFactory(key);
                dictionary.Add(key, value);
            }

            return value;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="IDictionary{TKey, TValue}"/> if the key does not already exist.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to add the key/value pair to if the key does not already exist.</param>
        /// <param name="key">The key to be added to the dictionary if it does not already exist.</param>
        /// <param name="value">The value to assign to the key if the key does not already exist.</param>
        /// <returns>The value of the key in the dictionary.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.TryGetValue(key, out TValue tryGetValue))
            {
                tryGetValue = value;
                dictionary.Add(key, tryGetValue);
            }

            return tryGetValue;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="IDictionary{TKey, TValue}"/> if the key does not already exist,
        /// or updates a key/value pair in the <see cref="IDictionary{TKey, TValue}"/> if the key already exists.
        /// </summary>
        /// <param name="dictionary">The dictionary to add the key/value pair to if the key does not already exist.</param>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (dictionary.TryGetValue(key, out TValue value))
                value = updateValueFactory(key, value);
            else
                value = addValueFactory(key);

            dictionary[key] = value;

            return value;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="IDictionary{TKey, TValue}"/> if the key does not already exist,
        /// or updates a key/value pair in the <see cref="IDictionary{TKey, TValue}"/> if the key already exists.
        /// </summary>
        /// <param name="dictionary">The dictionary to add the key/value pair to if the key does not already exist.</param>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (dictionary.TryGetValue(key, out TValue value))
                value = updateValueFactory(key, value);
            else
                value = addValue;

            dictionary[key] = value;

            return value;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="IDictionary{TKey, TValue}"/> if the key does not already exist,
        /// or updates a key/value pair in the <see cref="IDictionary{TKey, TValue}"/> if the key already exists.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to add the key/value pair to if the key does not already exist.</param>
        /// <param name="key">The key to be added or updated.</param>
        /// <param name="valueFactory">The function used to generate a value for the key.</param>
        /// <returns>The value of the key in the dictionary after updating.</returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue value = valueFactory(key);
            dictionary[key] = value;
            return value;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="IDictionary{TKey, TValue}"/> if the key does not already exist,
        /// or updates a key/value pair in the <see cref="IDictionary{TKey, TValue}"/> if the key already exists.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to add the key/value pair to if the key does not already exist.</param>
        /// <param name="key">The key to be added or updated.</param>
        /// <param name="value">The value to be assigned to the key.</param>
        /// <returns>The value of the key in the dictionary after updating.</returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary[key] = value;
            return value;
        }
    }
}

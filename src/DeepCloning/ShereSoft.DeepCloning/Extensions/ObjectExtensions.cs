﻿using System;
using System.Runtime.CompilerServices;

namespace ShereSoft.Extensions
{
    /// <summary>
    /// Provides a set of convenient methods for deep cloning.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Creates a deep-copied instance of the specified object
        /// </summary>
        /// <typeparam name="T">The type of object to clone.</typeparam>
        /// <param name="value">Any object</param>
        /// <returns>A deep-copied instance of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DeepClone<T>(this T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return DeepCloning<T>.Copy(value, DeepCloningOptions.None);
        }

        /// <summary>
        /// Creates a deep-copied instance of the specified object with options
        /// </summary>
        /// <typeparam name="T">The type of object to clone.</typeparam>
        /// <param name="value">Any object</param>
        /// <param name="options">Options to control the cloning behavior</param>
        /// <returns>A deep-copied instance of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DeepClone<T>(this T value, DeepCloningOptions options)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return DeepCloning<T>.Copy(value, options);
        }
    }
}

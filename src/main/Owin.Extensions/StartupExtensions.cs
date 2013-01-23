// Licensed to Monkey Square, Inc. under one or more contributor 
// license agreements.  See the NOTICE file distributed with 
// this work or additional information regarding copyright 
// ownership.  Monkey Square, Inc. licenses this file to you 
// under the Apache License, Version 2.0 (the "License"); you 
// may not use this file except in compliance with the License.
// You may obtain a copy of the License at 
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Owin.Types;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Owin
{
    /// <summary>
    /// Extension methods for IAppBuilder that provide syntax for commonly supported patterns.
    /// </summary>
    public static partial class StartupExtensions
    {
        /// <summary>
        /// Attach the given application to the pipeline.  Nothing attached after this point will be executed.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="app"></param>
        public static void Run(this IAppBuilder builder, object app)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            builder.Use(new Func<object, object>(ignored => app));
        }

        /// <summary>
        /// The Build is called at the point when all of the middleware should be chained
        /// together. May be called to build pipeline branches.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>The request processing entry point for this section of the pipeline.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static AppFunc Build(this IAppBuilder builder)
        {
            return builder.Build<AppFunc>();
        }

        /// <summary>
        /// The Build is called at the point when all of the middleware should be chained
        /// together. May be called to build pipeline branches.
        /// </summary>
        /// <typeparam name="TApp">The application signature.</typeparam>
        /// <param name="builder"></param>
        /// <returns>The request processing entry point for this section of the pipeline.</returns>
        public static TApp Build<TApp>(this IAppBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return (TApp)builder.Build(typeof(TApp));
        }

        /// <summary>
        /// Creates a new IAppBuilder instance from the current one and then invokes the configuration callback.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">The callback for configuration.</param>
        /// <returns>The request processing entry point for this section of the pipeline.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "By design")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static AppFunc BuildNew(this IAppBuilder builder, Action<IAppBuilder> configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            return builder.BuildNew<AppFunc>(configuration);
        }

        /// <summary>
        /// Creates a new IAppBuilder instance from the current one and then invokes the configuration callback.
        /// </summary>
        /// <typeparam name="TApp">The application signature.</typeparam>
        /// <param name="builder"></param>
        /// <param name="configuration">The callback for configuration.</param>
        /// <returns>The request processing entry point for this section of the pipeline.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "By design")]
        public static TApp BuildNew<TApp>(this IAppBuilder builder, Action<IAppBuilder> configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            IAppBuilder nested = builder.New();
            configuration(nested);
            return nested.Build<TApp>();
        }

        /// <summary>
        /// Adds converters for adapting between disparate application signatures.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="conversion"></param>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "False positive")]
        public static void AddSignatureConversion(
            this IAppBuilder builder,
            Delegate conversion)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            object value;
            if (builder.Properties.TryGetValue("builder.AddSignatureConversion", out value) &&
                value is Action<Delegate>)
            {
                ((Action<Delegate>)value).Invoke(conversion);
            }
            else
            {
                throw new MissingMethodException(builder.GetType().FullName, "AddSignatureConversion");
            }
        }
    }
}

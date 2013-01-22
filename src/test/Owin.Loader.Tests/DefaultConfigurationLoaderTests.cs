﻿// Licensed to Monkey Square, Inc. under one or more contributor 
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DifferentNamespace;
using Owin;
using Owin.Builder;
using Xunit;

namespace Owin.Loader.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class DefaultConfigurationLoaderTests
    {
        [Fact]
        public void Strings_are_split_based_on_dots()
        {
            var strings = DefaultLoader.DotByDot("this.is.a.test").ToArray();
            Assert.Equal(4, strings.Length);
            Assert.Equal("this.is.a.test", strings[0]);
            Assert.Equal("this.is.a", strings[1]);
            Assert.Equal("this.is", strings[2]);
            Assert.Equal("this", strings[3]);
        }

        [Fact]
        public void Leading_and_trailing_dot_and_empty_strings_are_safe_and_ignored()
        {
            var string1 = DefaultLoader.DotByDot(".a.test").ToArray();
            var string2 = DefaultLoader.DotByDot("a.test.").ToArray();
            var string3 = DefaultLoader.DotByDot(".a.test.").ToArray();
            var string4 = DefaultLoader.DotByDot(".").ToArray();
            var string5 = DefaultLoader.DotByDot(string.Empty).ToArray();
            var string6 = DefaultLoader.DotByDot(null).ToArray();

            AssertArrayEqual(new[] { "a.test", "a" }, string1);
            AssertArrayEqual(new[] { "a.test", "a" }, string2);
            AssertArrayEqual(new[] { "a.test", "a" }, string3);
            AssertArrayEqual(new string[0], string4);
            AssertArrayEqual(new string[0], string5);
            AssertArrayEqual(new string[0], string6);
        }

        void AssertArrayEqual(string[] expected, string[] actual)
        {
            Assert.Equal(expected.Length, actual.Length);
            foreach (var index in Enumerable.Range(0, actual.Length))
            {
                Assert.Equal(expected[index], actual[index]);
            }
        }

        static int _helloCalls;

        public static void Hello(IAppBuilder builder)
        {
            _helloCalls += 1;
        }

        [Fact]
        public void Load_will_find_assembly_and_type_and_static_method()
        {
            var loader = new DefaultLoader();
            var configuration = loader.Load("Owin.Loader.Tests.DefaultConfigurationLoaderTests.Hello");

            _helloCalls = 0;
            configuration(new AppBuilder());
            Assert.Equal(1, _helloCalls);
        }

        [Fact]
        public void An_extra_segment_will_cause_the_match_to_fail()
        {
            var loader = new DefaultLoader();
            var configuration = loader.Load("Gate.Tests.StartupTests.Loader.DefaultConfigurationLoaderTests.Hello.Bar");

            Assert.Null(configuration);
        }

        [Fact]
        public void Calling_a_class_with_multiple_configs_is_okay()
        {
            var loader = new DefaultLoader();
            var foo = loader.Load("Owin.Loader.Tests.MultiConfigs.Foo");
            var bar = loader.Load("Owin.Loader.Tests.MultiConfigs.Bar");

            MultiConfigs.FooCalls = 0;
            MultiConfigs.BarCalls = 0;

            foo(new AppBuilder());

            Assert.Equal(1, MultiConfigs.FooCalls);
            Assert.Equal(0, MultiConfigs.BarCalls);

            bar(new AppBuilder());

            Assert.Equal(1, MultiConfigs.FooCalls);
            Assert.Equal(1, MultiConfigs.BarCalls);
        }

        [Fact]
        public void Configuration_method_defaults_to_Configuration_if_only_type_name_is_provided()
        {
            var loader = new DefaultLoader();
            var configuration = loader.Load("Owin.Loader.Tests.MultiConfigs");

            MultiConfigs.FooCalls = 0;
            MultiConfigs.BarCalls = 0;
            MultiConfigs.ConfigurationCalls = 0;

            Assert.Equal(0, MultiConfigs.FooCalls);
            Assert.Equal(0, MultiConfigs.BarCalls);
            Assert.Equal(0, MultiConfigs.ConfigurationCalls);

            configuration(new AppBuilder());

            Assert.Equal(0, MultiConfigs.FooCalls);
            Assert.Equal(0, MultiConfigs.BarCalls);
            Assert.Equal(1, MultiConfigs.ConfigurationCalls);
        }

        [Fact]
        public void Comma_may_be_used_if_assembly_name_doesnt_match_namespace()
        {
            var loader = new DefaultLoader();
            var configuration = loader.Load("DifferentNamespace.DoesNotFollowConvention, Owin.Loader.Tests");

            DoesNotFollowConvention.ConfigurationCalls = 0;

            configuration(new AppBuilder());

            Assert.Equal(1, DoesNotFollowConvention.ConfigurationCalls);
        }

        static int _alphaCalls;

        public static AppFunc Alpha(IDictionary<string, object> properties)
        {
            return env => 
            {
                ++_alphaCalls;
                return GetCompletedTask();
            };
        }

        private static Task GetCompletedTask()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetResult(null);
            return tcs.Task;
        }

        [Fact]
        public void Method_that_returns_app_action_may_also_be_called()
        {
            var loader = new DefaultLoader();
            var configuration = loader.Load("Owin.Loader.Tests.DefaultConfigurationLoaderTests.Alpha");

            var builder = new AppBuilder();
            configuration(builder);
            var app = builder.Build<AppFunc>();

            _alphaCalls = 0;
            app(new Dictionary<string, object>());
            Assert.Equal(1, _alphaCalls);
        }

        [Fact]
        public void Startup_Configuration_in_assembly_namespace_will_be_discovered_by_default()
        {
            var loader = new DefaultLoader();
            var configuration = loader.Load(string.Empty);
            Startup.ConfigurationCalls = 0;
            configuration(new AppBuilder());
            Assert.Equal(1, Startup.ConfigurationCalls);

            configuration = loader.Load(null);
            Startup.ConfigurationCalls = 0;
            configuration(new AppBuilder());
            Assert.Equal(1, Startup.ConfigurationCalls);
        }
    }

    public class Startup
    {
        public static int ConfigurationCalls { get; set; }

        public void Configuration(IAppBuilder builder)
        {
            ConfigurationCalls++;
        }
    }

    public class MultiConfigs
    {
        public static int FooCalls;

        public static void Foo(IAppBuilder builder)
        {
            FooCalls += 1;
        }

        public static int BarCalls;

        public static void Bar(IAppBuilder builder)
        {
            BarCalls += 1;
        }

        public static int ConfigurationCalls;

        public static void Configuration(IAppBuilder builder)
        {
            ConfigurationCalls += 1;
        }
    }
}

namespace DifferentNamespace
{
    public class DoesNotFollowConvention
    {
        public static int ConfigurationCalls;

        public static void Configuration(IAppBuilder builder)
        {
            ConfigurationCalls += 1;
        }
    }
}
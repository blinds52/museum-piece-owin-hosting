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
using System.Threading.Tasks;
using MiddlewareConvention1;
using MiddlewareConvention2;
using MiddlewareConvention3;

namespace StartupConvention2
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Startup
    {
        public AppFunc Configuration(IDictionary<string, object> properties)
        {
            AppFunc app = Main;

            app = Alpha.Invoke(app, "One", "Two");

            app = Beta.Invoke("Three", "Four").Invoke(app);

            app = new Gamma(app, "Five", "Six").Invoke;

            app = new Delta(app, "Seven", "Eight").Invoke;

            Theta theta = new Theta(new object());
            theta.Initialize(app, "Nine", "Ten");
            app = theta.Invoke;

            return app;
        }

        public Task Main(IDictionary<string, object> env)
        {
            throw new NotImplementedException();
        }
    }
}
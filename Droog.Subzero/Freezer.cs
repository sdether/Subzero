/*
 * Subzero 
 * Copyright (C) 2010 Arne F. Claassen
 * http://www.claassen.net/geek/blog geekblog [at] claassen [dot] net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using Droog.Subzero.Util;

namespace Droog.Subzero {
    public class Freezer {

        public interface IFreezableWrapper { }

        private static readonly ProxyGenerator _generator = new ProxyGenerator();

        // TODO: need to check the type for virtuals and supported collection types
        public static T AsFreezable<T>(T instance) where T : class {
            if(instance.IsA<IFreezableWrapper>()) {
                return instance;
            }
            return instance.Wrap(_generator, Frozen.False);
        }

        public static bool IsFreezable<T>(T instance) where T : class {
            return CheckInstance(instance);
        }

        public static bool IsFrozen<T>(T instance) where T : class {
            var freezable = CastToFreezableOrThrow(instance);
            return freezable.IsFrozen;
        }

        public static void Freeze<T>(T instance) where T : class {
            var freezable = CastToFreezableOrThrow(instance);
            freezable.Freeze();
        }

        public static T Thaw<T>(T instance) where T : class {
            var freezable = CastToFreezableOrThrow(instance);
            return freezable.Thaw();
        }

        public static T FreezeDry<T>(T instance) where T : class {
            var freezable = instance as IFreezable<T>;
            return freezable == null ? ((IFreezable<T>)AsFreezable(instance)).FreezeDry() : freezable.FreezeDry();
        }


        private static IFreezable<T> CastToFreezableOrThrow<T>(T instance) where T : class {
            if(!CheckInstance(instance)) {
                throw new NonFreezableException(string.Format("Instance '{0}' is not an IFreezable or wrapped by a freezable proxy", instance.GetType()));
            }
            return instance as IFreezable<T>;
        }

        private static bool CheckInstance(object instance) {
            return instance.IsA<IFreezableWrapper>();
        }
    }
}

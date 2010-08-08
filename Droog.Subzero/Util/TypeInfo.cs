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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Droog.Subzero.Util {
    internal class TypeInfo {

        // getter and setter from http://jachman.wordpress.com/2006/08/22/2000-faster-using-dynamic-method-calls/
        internal class Property {
            public readonly string Name;
            public readonly Type Type;
            private readonly Action<object, object> _setter;
            private readonly Func<object, object> _getter;

            public Property(PropertyInfo property) {
                Name = property.Name;
                Type = property.PropertyType;
                _getter = CreateGetMethod(property);
                _setter = CreateSetMethod(property);
            }

            public object Get(object instance) {
                return _getter(instance);
            }

            public void Set(object instance, object value) {
                _setter(instance, value);
            }

            private static Action<object, object> CreateSetMethod(PropertyInfo propertyInfo) {
                MethodInfo setMethod = propertyInfo.GetSetMethod();
                if(setMethod == null) {
                    return null;
                }
                var arguments = new[] { typeof(object), typeof(object) };
                var setter = new DynamicMethod(String.Concat("_Set", propertyInfo.Name, "_"), typeof(void), arguments, propertyInfo.DeclaringType);
                var generator = setter.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
                generator.Emit(OpCodes.Ldarg_1);
                if(propertyInfo.PropertyType.IsClass) {
                    generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                } else {
                    generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                }
                generator.EmitCall(OpCodes.Callvirt, setMethod, null);
                generator.Emit(OpCodes.Ret);
                return (Action<object, object>)setter.CreateDelegate(typeof(Action<object, object>));
            }

            private static Func<object, object> CreateGetMethod(PropertyInfo propertyInfo) {
                var getMethod = propertyInfo.GetGetMethod();
                if(getMethod == null) {
                    return null;
                }
                var arguments = new[] { typeof(object) };
                var getter = new DynamicMethod(String.Concat("_Get", propertyInfo.Name, "_"), typeof(object), arguments, propertyInfo.DeclaringType);
                var generator = getter.GetILGenerator();
                generator.DeclareLocal(typeof(object));
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
                generator.EmitCall(OpCodes.Callvirt, getMethod, null);
                if(!propertyInfo.PropertyType.IsClass) {
                    generator.Emit(OpCodes.Box, propertyInfo.PropertyType);
                }
                generator.Emit(OpCodes.Ret);
                return (Func<object, object>)getter.CreateDelegate(typeof(Func<object, object>));
            }
        }

        private static readonly Dictionary<Type, TypeInfo> _types = new Dictionary<Type, TypeInfo>();

        public static TypeInfo GetTypeInfo(Type type) {
            lock(_types) {
                TypeInfo info;
                if(!_types.TryGetValue(type, out info)) {
                    info = new TypeInfo(type);
                    _types[type] = info;
                }
                return info;
            }
        }

        private readonly List<Property> _properties = new List<Property>();
        private readonly Type _type;
        private readonly Func<object> _createHandler;

        private TypeInfo(Type type) {
            _type = type;
            _createHandler = BuildCreateHandler();
            foreach(var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)) {
                _properties.Add(new Property(property));
            }
        }

        private Func<object> BuildCreateHandler() {
            var constructorInfo = _type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if(constructorInfo == null) {
                return () => Activator.CreateInstance(_type, true);
            }
            return constructorInfo.CreateDelegate(typeof(Func<object>)) as Func<object>;
        }

        public object Create() {
            return _createHandler();
        }

        public IEnumerable<Property> Properties { get { return _properties; } }
    }
}
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Droog.Subzero.Util;
using NUnit.Framework;

namespace Droog.Subzero.Test {

    [TestFixture]
    public class IncubatorTests {

        [Test]
        public void Can_clone_simple_dto() {
            var expected = new Simple() { Id = 42 };
            var clone = Incubator.Clone(expected);

            Assert.AreNotSame(expected, clone);
            Assert.AreEqual(expected.Id, clone.Id);
        }

        [Test]
        public void Can_clone_complex_dto() {
            var expected = new Complex() { Simple = new Simple() { Id = 42 } };
            var clone = Incubator.Clone(expected);

            Assert.AreNotSame(expected, clone);
            Assert.AreNotSame(expected.Simple, clone.Simple);
            Assert.AreEqual(expected.Simple.Id, clone.Simple.Id);
        }

        [Test]
        public void Can_clone_DtoWithArrayList() {
            var expected = new DtoWithArrayList() { Values = new ArrayList() };
            expected.Values.Add("foo");
            var clone = Incubator.Clone(expected);

            Assert.AreNotSame(expected, clone);
            Assert.AreNotSame(expected.Values, clone.Values);
            Assert.AreEqual(expected.Values.ToArray(), clone.Values.ToArray());
        }

        [Test]
        public void Can_clone_DtoWithValueArray() {
            var expected = new DtoWithValueArray() { Values = new[] { 1, 2, 3 } };
            var clone = Incubator.Clone(expected);

            Assert.AreNotSame(expected, clone);
            Assert.AreNotSame(expected.Values, clone.Values);
            Assert.AreEqual(expected.Values, clone.Values);
        }

        [Test]
        public void Can_clone_DtoWithObjectArray() {
            var expected = new DtoWithObjectArray() {
                Id = 1,
                Children = new[] {new DtoWithObjectArray() {
                    Id = 2, Children = new[] {new DtoWithObjectArray()}
                }}
            };
            var clone = Incubator.Clone(expected);

            Assert.AreNotSame(expected, clone);
            Assert.AreNotSame(expected.Children, clone.Children);
            Assert.AreEqual(expected.Children[0].Id, clone.Children[0].Id);
            Assert.AreEqual(expected.Children[0].Children[0].Id, clone.Children[0].Children[0].Id);
        }

        [Test]
        public void Can_clone_DtoWithValueCollection() {
            var expected = new DtoWithValueCollection() { Values = new[] { 1, 2, 3 } };
            var clone = Incubator.Clone(expected);

            Assert.AreNotSame(expected, clone);
            Assert.AreNotSame(expected.Values, clone.Values);
            Assert.AreEqual(expected.Values.ToArray(), clone.Values.ToArray());
        }

        [Test]
        public void Can_clone_DtoWithValueCollection2() {
            var expected = new DtoWithValueCollection() { Values = new List<int>() };
            expected.Values.Add(1);
            expected.Values.Add(2);
            expected.Values.Add(3);
            var clone = Incubator.Clone(expected);

            Assert.AreNotSame(expected, clone);
            Assert.AreNotSame(expected.Values, clone.Values);
            Assert.AreEqual(expected.Values.ToArray(), clone.Values.ToArray());
        }

        [Test]
        public void Can_clone_DtoWithObjectCollection() {
            var expected = new DtoWithObjectCollection() {
                Id = 1,
                Children = new[] {new DtoWithObjectCollection() {
                    Id = 2, Children = new[] {new DtoWithObjectCollection()}
                }}
            };
            var clone = Incubator.Clone(expected);

            Assert.AreNotSame(expected, clone);
            Assert.AreNotSame(expected.Children, clone.Children);
            Assert.AreEqual(expected.Children.First().Id, clone.Children.First().Id);
            Assert.AreEqual(expected.Children.First().Children.First().Id, clone.Children.First().Children.First().Id);
        }

        [Test]
        public void Can_clone_DtoWithValueList() {
            var expected = new DtoWithValueList() { Values = new[] { 1, 2, 3 } };
            var clone = Incubator.Clone(expected);

            Assert.AreNotSame(expected, clone);
            Assert.AreNotSame(expected.Values, clone.Values);
            Assert.AreEqual(expected.Values.ToArray(), clone.Values.ToArray());
        }

        [Test]
        public void Can_clone_DtoWithObjectList() {
            var expected = new DtoWithObjectList() {
                Id = 1,
                Children = new[] {new DtoWithObjectList() {
                    Id = 2, Children = new[] {new DtoWithObjectList()}
                }}
            };
            var clone = Incubator.Clone(expected);

            Assert.AreNotSame(expected, clone);
            Assert.AreNotSame(expected.Children, clone.Children);
            Assert.AreEqual(expected.Children.First().Id, clone.Children.First().Id);
            Assert.AreEqual(expected.Children.First().Children.First().Id, clone.Children.First().Children.First().Id);
        }

        [Test]
        public void Can_clone_DtoWithValueEnumerable() {
            var expected = new DtoWithValueList() { Values = new[] { 1, 2, 3 } };
            var clone = Incubator.Clone(expected);

            Assert.AreNotSame(expected, clone);
            Assert.AreNotSame(expected.Values, clone.Values);
            Assert.AreEqual(expected.Values.ToArray(), clone.Values.ToArray());
        }

        [Test]
        public void Can_clone_DtoWithObjectEnumerable() {
            var expected = new DtoWithObjectEnumerable() {
                Id = 1,
                Children = new[] {new DtoWithObjectEnumerable() {
                    Id = 2, Children = new[] {new DtoWithObjectEnumerable()}
                }}
            };
            var clone = Incubator.Clone(expected);

            Assert.AreNotSame(expected, clone);
            Assert.AreNotSame(expected.Children, clone.Children);
            Assert.AreEqual(expected.Children.First().Id, clone.Children.First().Id);
            Assert.AreEqual(expected.Children.First().Children.First().Id, clone.Children.First().Children.First().Id);
        }

        [Test]
        public void Can_clone_DtoWithDictionary() {
            var expected = new DtoWithDictionary() {
                Lookup = new Dictionary<string, Simple>() {
                    {"foo",new Simple(){Id = 1}},
                    {"bar",new Simple(){Id = 2}},
                }
            };
            var clone = Incubator.Clone(expected);

            Assert.AreNotSame(expected, clone);
            Assert.AreNotSame(expected.Lookup, clone.Lookup);
            Assert.AreEqual(expected.Lookup.Values.OrderBy(x => x).ToArray(), clone.Lookup.Values.OrderBy(x => x).ToArray());
            Assert.AreNotSame(expected.Lookup["foo"], clone.Lookup["foo"]);
            Assert.AreEqual(expected.Lookup["foo"].Id, clone.Lookup["foo"].Id);

        }
    }
}

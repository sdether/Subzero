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
using System.Linq;
using NUnit.Framework;

namespace Droog.Subzero.Test {

    [Ignore("don't deal with collections in graphs yet")]
    [TestFixture]
    public class PocoContainerTests {

        [Test]
        public void Can_freeze_and_access_DtoWithValueCollection() {
            var dto = Freezer.AsFreezable(new DtoWithValueCollection() { Values = new[] { 1, 2, 3 } });
            Freezer.Freeze(dto);
            Assert.AreEqual(1, dto.Values.First());
        }

        [Test]
        [ExpectedException(typeof(FrozenAccessException))]
        public void Frozen_DtoWithValueCollection_throws_on_add() {
            var dto = Freezer.AsFreezable(new DtoWithValueCollection() { Values = new[] { 1, 2, 3 } });
            Freezer.Freeze(dto);
            dto.Values.Add(5);
        }

        [Test]
        [ExpectedException(typeof(FrozenAccessException))]
        public void Frozen_DtoWithValueCollection_throws_on_remove() {
            var dto = Freezer.AsFreezable(new DtoWithValueCollection() { Values = new[] { 1, 2, 3 } });
            Freezer.Freeze(dto);
            dto.Values.Remove(2);
        }

        [Test]
        public void Can_freeze_and_access_DtoWithObjectCollection() {
            var dto = Freezer.AsFreezable(new DtoWithObjectCollection() {
                Id = 1,
                Children = new[] {new DtoWithObjectCollection() {
                    Id = 2, Children = new[] {new DtoWithObjectCollection()}
                }}
            });
            Freezer.Freeze(dto);
            Assert.AreEqual(2, dto.Children.First().Id);
        }

        [Test]
        [ExpectedException(typeof(FrozenAccessException))]
        public void Frozen_DtoWithObjectCollection_throws_on_add() {
            var dto = Freezer.AsFreezable(new DtoWithObjectCollection() {
                Id = 1,
                Children = new[] {new DtoWithObjectCollection() {
                    Id = 2, Children = new[] {new DtoWithObjectCollection()}
                }}
            });
            Freezer.Freeze(dto);
            dto.Children.Add(new DtoWithObjectCollection() { Id = 10 });
        }

        [Test]
        [ExpectedException(typeof(FrozenAccessException))]
        public void Frozen_DtoWithObjectCollection_throws_on_remove() {
            var dto = Freezer.AsFreezable(new DtoWithObjectCollection() {
                Id = 1,
                Children = new[] {new DtoWithObjectCollection() {
                    Id = 2, Children = new[] {new DtoWithObjectCollection()}
                }}
            });
            Freezer.Freeze(dto);
            var c = dto.Children.First();
            dto.Children.Remove(c);
        }

        [Test]
        [ExpectedException(typeof(FrozenAccessException))]
        public void Frozen_DtoWithObjectCollection_throws_on_mod_item() {
            var dto = Freezer.AsFreezable(new DtoWithObjectCollection() {
                Id = 1,
                Children = new[] {new DtoWithObjectCollection() {
                    Id = 2, Children = new[] {new DtoWithObjectCollection()}
                }}
            });
            Freezer.Freeze(dto);
            var c = dto.Children.First();
            c.Id = 5;
        }

        [Test]
        public void Can_freeze_and_access_DtoWithValueEnumerable() {
            var dto = Freezer.AsFreezable(new DtoWithValueEnumerable() { Values = new[] { 1, 2, 3 } });
            Freezer.Freeze(dto);
            Assert.AreEqual(1, dto.Values.First());
        }

        [Test]
        public void Can_freeze_and_access_DtoWithObjectEnumerable() {
            var dto = Freezer.AsFreezable(new DtoWithObjectEnumerable() {
                Id = 1,
                Children = new[] {new DtoWithObjectEnumerable() {
                    Id = 2, Children = new[] {new DtoWithObjectEnumerable()}
                }}
            });
            Freezer.Freeze(dto);
            Assert.AreEqual(2, dto.Children.First().Id);
        }

        [Test]
        [ExpectedException(typeof(FrozenAccessException))]
        public void Frozen_DtoWithObjectEnumerable_throws_on_mod_item() {
            var dto = Freezer.AsFreezable(new DtoWithObjectEnumerable() {
                Id = 1,
                Children = new[] {new DtoWithObjectEnumerable() {
                    Id = 2, Children = new[] {new DtoWithObjectEnumerable()}
                }}
            });
            Freezer.Freeze(dto);
            var c = dto.Children.First();
            c.Id = 5;
        }


    }
}

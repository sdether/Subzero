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
using NUnit.Framework;

namespace Droog.Subzero.Test {

    [TestFixture]
    public class PocoGraphTests {

        private void AssertSameValue(DataGraph first, DataGraph next) {
            Assert.AreEqual(first.Readonly.Id, next.Readonly.Id);
            Assert.AreEqual(first.Readonly.Name, next.Readonly.Name);
            if(first.Settable == null) {
                Assert.IsNull(next.Settable);
            } 
            Assert.AreEqual(first.Settable.Id, next.Settable.Id);
            Assert.AreEqual(first.Settable.Name, next.Settable.Name);
        }

        private DataGraph CreateData() {
            return Freezer.AsFreezable(new DataGraph { Readonly = { Id = 42, Name = "Everything" } });
        }

        public class DataGraph {
            public DataGraph() {
                Readonly = new Data();
            }
            public virtual Data Settable { get; set; }
            public virtual Data Readonly { get; private set; }
        }

        public class Data {
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
        }
    }
}

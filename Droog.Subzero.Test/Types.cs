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

namespace Droog.Subzero.Test {
    public class DtoWithArrayList {
        public virtual ArrayList Values { get; set; }
    }

    public class DtoWithValueArray {
        public virtual int[] Values { get; set; }
    }

    public class DtoWithObjectArray {
        public virtual int Id { get; set; }
        public virtual DtoWithObjectArray[] Children { get; set; }
    }

    public class DtoWithValueCollection {
        public virtual ICollection<int> Values { get; set; }
    }

    public class DtoWithObjectCollection {
        public virtual int Id { get; set; }
        public virtual ICollection<DtoWithObjectCollection> Children { get; set; }
    }

    public class DtoWithValueList {
        public virtual IList<int> Values { get; set; }
    }

    public class DtoWithObjectList {
        public virtual int Id { get; set; }
        public virtual IList<DtoWithObjectList> Children { get; set; }
    }

    public class DtoWithValueEnumerable {
        public virtual IEnumerable<int> Values { get; set; }
    }

    public class DtoWithObjectEnumerable {
        public virtual int Id { get; set; }
        public virtual IEnumerable<DtoWithObjectEnumerable> Children { get; set; }
    }

    public class DtoWithDictionary {
        public virtual IDictionary<string, Simple> Lookup { get; set; }
    }

    public class Complex {
        public virtual Simple Simple { get; set; }
    }

    public class Simple {
        public virtual int Id { get; set; }
    }
}

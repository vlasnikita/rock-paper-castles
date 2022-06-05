// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    public class Attribute
    {
        public string Name;
    }

    public class BoolAttribute : Attribute
    {
        public bool Value;
    }

    public class IntAttribute : Attribute
    {
        public int Value;
    }

    public class StringAttribute : Attribute
    {
        public string Value;
    }
}

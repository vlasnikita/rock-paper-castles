// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// Base class for integer values in the visual editor. They are implicitly convertible to int
    /// for convenience.
    /// </summary>
    public class IntValue
    {
        public virtual int Value { get; set; }

        public static implicit operator int(IntValue instance)
        {
            return instance.Value;
        }
    }

    /// <summary>
    /// Constant integer value.
    /// </summary>
    public class ConstantIntValue : IntValue
    {
        public int Constant;

        public override int Value
        {
            get { return Constant; }
        }

        public ConstantIntValue(int constant)
        {
            Constant = constant;
        }
    }

    /// <summary>
    /// Random integer value in the [Min, Max] range.
    /// </summary>
    public class RandomIntValue : IntValue
    {
        public int Min;
        public int Max;

        public override int Value
        {
            get { return Random.Range(Min, Max + 1); }
        }

        public RandomIntValue(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }
}

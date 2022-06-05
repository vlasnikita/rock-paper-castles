// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine.Assertions;

namespace CCGKit
{
    /// <summary>
    /// Convenience base class for types that need to have a list of attributes.
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// List of attributes for this entity.
        /// </summary>
        public List<Attribute> Attributes = new List<Attribute>();

        /// <summary>
        /// Returns the value of the boolean attribute with the specified name.
        /// </summary>
        /// <param name="name">Name of the boolean attribute whose value to return.</param>
        /// <returns>The value of the boolean attribute with the specified name.</returns>
        public bool GetBoolAttribute(string name)
        {
            var attribute = Attributes.Find(x => x.Name == name);
            Assert.IsTrue(attribute != null && attribute is BoolAttribute);
            return (attribute as BoolAttribute).Value;
        }

        /// <summary>
        /// Returns the value of the integer attribute with the specified name.
        /// </summary>
        /// <param name="name">Name of the integer attribute whose value to return.</param>
        /// <returns>The value of the integer attribute with the specified name.</returns>
        public int GetIntegerAttribute(string name)
        {
            var attribute = Attributes.Find(x => x.Name == name);
            Assert.IsTrue(attribute != null && attribute is IntAttribute);
            return (attribute as IntAttribute).Value;
        }

        /// <summary>
        /// Returns the value of the string attribute with the specified name.
        /// </summary>
        /// <param name="name">Name of the string attribute whose value to return.</param>
        /// <returns>The value of the string attribute with the specified name.</returns>
        public string GetStringAttribute(string name)
        {
            var attribute = Attributes.Find(x => x.Name == name);
            Assert.IsTrue(attribute != null && attribute is StringAttribute);
            return (attribute as StringAttribute).Value;
        }

        /// <summary>
        /// Sets the value of the boolean attribute with the specified name to the specified value.
        /// </summary>
        /// <param name="name">Name of the boolean attribute whose value to set.</param>
        /// <param name="value">New value for the specified boolean attribute.</param>
        public void SetBoolAttribute(string name, bool value)
        {
            var attribute = Attributes.Find(x => x.Name == name);
            if (attribute != null && attribute is BoolAttribute)
            {
                var existingAttribute = attribute as BoolAttribute;
                existingAttribute.Value = value;
            }
            else
            {
                var newAttribute = new BoolAttribute();
                newAttribute.Name = name;
                newAttribute.Value = value;
                Attributes.Add(newAttribute);
            }
        }

        /// <summary>
        /// Sets the value of the integer attribute with the specified name to the specified value.
        /// </summary>
        /// <param name="name">Name of the integer attribute whose value to set.</param>
        /// <param name="value">New value for the specified integer attribute.</param>
        public void SetIntegerAttribute(string name, int value)
        {
            var attribute = Attributes.Find(x => x.Name == name);
            if (attribute != null && attribute is IntAttribute)
            {
                var existingAttribute = attribute as IntAttribute;
                existingAttribute.Value = value;
            }
            else
            {
                var newAttribute = new IntAttribute();
                newAttribute.Name = name;
                newAttribute.Value = value;
                Attributes.Add(newAttribute);
            }
        }

        /// <summary>
        /// Sets the value of the string attribute with the specified name to the specified value.
        /// </summary>
        /// <param name="name">Name of the string attribute whose value to set.</param>
        /// <param name="value">New value for the specified string attribute.</param>
        public void SetStringAttribute(string name, string value)
        {
            var attribute = Attributes.Find(x => x.Name == name);
            if (attribute != null && attribute is StringAttribute)
            {
                var existingAttribute = attribute as StringAttribute;
                existingAttribute.Value = value;
            }
            else
            {
                var newAttribute = new StringAttribute();
                newAttribute.Name = name;
                newAttribute.Value = value;
                Attributes.Add(newAttribute);
            }
        }
    }
}

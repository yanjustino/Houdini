using System;

namespace Houdini.Oracle
{
    public class PropertyConfig
    {
        public Type Type { get; private set; }
        public string Name { get; private set; }
        public string Column { get; private set; }

        public PropertyConfig(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        internal void SetColumn(string name)
        {
            Column = name;
        }
    }
}

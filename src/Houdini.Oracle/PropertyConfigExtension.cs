namespace Houdini.Oracle
{
    public static class PropertyConfigExtension
    {
        public static PropertyConfig Column(this PropertyConfig property, string column)
        {
            property.SetColumn(column);
            return property;
        }
    }
}

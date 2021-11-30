namespace SberGames.Utils.Reflection
{
    using System;
    using System.Linq;

    public static class ReflectionExtensions
    {
        public static bool HasInterfaceOfOpenGenericType(this Type type, Type genericInterfaceType)
        {
            return type.GetInterfaces().Any(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == genericInterfaceType);
        }
        
        public static Type GetInterfaceOfOpenGenericType(this Type type, Type genericInterfaceType)
        {
            return type.GetInterfaces().First(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == genericInterfaceType);
        }
    }
}
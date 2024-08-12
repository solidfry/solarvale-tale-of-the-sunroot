﻿namespace Utilities
{
    public class EnumUtilities
    {
        public static T ParseEnum<T>(string value)
        {
            return (T) System.Enum.Parse(typeof(T), value, true);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Func<object, string>> CustomTypeSerialization { get; }
        Dictionary<Type, CultureInfo> NumbersCulture { get; }
        Dictionary<PropertyInfo, Func<object, string>> CustomPropertySerialization { get; }
        Dictionary<PropertyInfo, int> StringPropertyTrimmingCount { get; }
    }
}
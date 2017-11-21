using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> FinalTypes;
        private readonly HashSet<Type> ExcludingTypes;
        private readonly HashSet<PropertyInfo> ExcludingProperty;


        private readonly Dictionary<Type, Func<object, string>> customTypeSerialization;
        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.CustomTypeSerialization
            => customTypeSerialization;

        private readonly Dictionary<Type, CultureInfo> numbersCulture;
        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.NumbersCulture
            => numbersCulture;

        private readonly Dictionary<PropertyInfo, Func<object, string>> customPropertySerialization;
        Dictionary<PropertyInfo, Func<object, string>> IPrintingConfig<TOwner>.CustomPropertySerialization
            => customPropertySerialization;


        private readonly Dictionary<PropertyInfo, int> stringPropertyTrimmingCount;
        Dictionary<PropertyInfo, int> IPrintingConfig<TOwner>.StringPropertyTrimmingCount
            => stringPropertyTrimmingCount;

        public PrintingConfig()
        {
            customPropertySerialization = new Dictionary<PropertyInfo, Func<object, string>>();
            stringPropertyTrimmingCount = new Dictionary<PropertyInfo, int>();
            FinalTypes = new HashSet<Type>(new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            });
            ExcludingTypes = new HashSet<Type>();
            ExcludingProperty = new HashSet<PropertyInfo>();
            numbersCulture = new[] { typeof(double), typeof(long), typeof(int) }
            .ToDictionary(x => x, x => CultureInfo.CurrentCulture);
            customTypeSerialization = new Dictionary<Type, Func<object, string>>();
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private bool TryPrintingUsingCustomRules(object obj, out string printingStirng)
        {
            if (obj == null)
            {
                printingStirng = "null";
                return true;
            }
            var type = obj.GetType();
            if (customTypeSerialization.ContainsKey(type))
            {
                printingStirng = customTypeSerialization[obj.GetType()](obj);
                return true;
            }
            if (numbersCulture.ContainsKey(type))
            {
                printingStirng = type.GetMethods().Where(x => x.Name == "ToString")
                        .First(x => x.GetParameters().Length == 1 &&
                                    x.GetParameters()[0].ParameterType == typeof(IFormatProvider))
                        .Invoke(obj, new[] { numbersCulture[type] }).ToString();
                return true;
            }
            if (FinalTypes.Contains(type))
            {
                printingStirng = obj.ToString();
                return true;
            }
            printingStirng = null;
            return false;
        }

        private string PrintPropertyToString(PropertyInfo property, object obj, int nestingLevel)
        {
            if (customPropertySerialization.ContainsKey(property))
                return customPropertySerialization[property](property.GetValue(obj));
            return PrintToString(property.GetValue(obj), nestingLevel + 1);
        }

        private bool IsIgnoreProperty(PropertyInfo property)
        {
            var propType = property.PropertyType;

            if (ExcludingProperty.Contains(property))
                return true;
            if (ExcludingTypes.Contains(propType))
                return true;
            return false;

        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (TryPrintingUsingCustomRules(obj, out var printingStirng))
                return printingStirng;
            var type = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var allProperties = type.GetProperties();
            if (allProperties.Length == 0)
                sb.Append(type.Name);
            else sb.AppendLine(type.Name);
            foreach (var propertyInfo in allProperties)
            {
                if (IsIgnoreProperty(propertyInfo))
                    continue;
                var propertyInString = PrintPropertyToString(propertyInfo, obj, nestingLevel + 1);
                if (propertyInfo.PropertyType == typeof(string) &&
                    stringPropertyTrimmingCount.ContainsKey(propertyInfo))
                {
                    var trimmingCount = stringPropertyTrimmingCount[propertyInfo];
                    propertyInString = new string(propertyInString.Take(trimmingCount).ToArray());
                }

                sb.Append(identation + propertyInfo.Name + " = " + propertyInString + Environment.NewLine);
            }
            return sb.ToString();
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            ExcludingTypes.Add(typeof(T));
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>
            (Expression<Func<TOwner, TProperty>> propSelector)
        {
            var name = ((MemberExpression)propSelector.Body).Member.Name;
            var property = typeof(TOwner).GetProperty(name);
            return new PropertyPrintingConfig<TOwner, TProperty>(this, property);
        }

        public PrintingConfig<TOwner> Excluding<TProp>(Expression<Func<TOwner, TProp>> propSelector)
        {
            var name = ((MemberExpression)propSelector.Body).Member.Name;
            var property = typeof(TOwner).GetProperty(name);
            ExcludingProperty.Add(property);
            return this;
        }
    }
}
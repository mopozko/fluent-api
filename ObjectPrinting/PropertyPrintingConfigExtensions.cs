using System;
using System.Globalization;
using System.Linq;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        private static PrintingConfig<TOwner> SetCulture<TOwner,TPropType>
            (IPropertyPrintingConfig<TOwner, TPropType> config,CultureInfo culture)
        {
            var supptortTypes = new[] {typeof(int), typeof(double), typeof(long)};
            var type = typeof(TPropType);

            if (supptortTypes.All(t => t != type))
                throw new ArgumentException($"PropertyPrintingConfig with PropType == {type} not supported");

            IPrintingConfig<TOwner> parent = config.ParentConfig;
            parent.NumbersCulture[typeof(TPropType)] = culture;
            return (PrintingConfig<TOwner>) parent;
        }

        public static PrintingConfig<TOwner> Using<TOwner>
            (this PropertyPrintingConfig<TOwner, int> config, CultureInfo culture)
        {
            return SetCulture(config, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>
            (this PropertyPrintingConfig<TOwner, double> config, CultureInfo culture)
        {
            return SetCulture(config, culture);
        }
        public static PrintingConfig<TOwner> Using<TOwner>
            (this PropertyPrintingConfig<TOwner, long> config, CultureInfo culture)
        {
            return SetCulture(config, culture);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>
            (this IPropertyPrintingConfig<TOwner, string> config, int trimCount)
        {
            IPrintingConfig<TOwner> parent = config.ParentConfig;
            var property = config.Property;
            parent.StringPropertyTrimmingCount.Add(property,trimCount);
            return (PrintingConfig<TOwner>)parent;
        }
    }
}
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig 
            => printingConfig;

        private readonly PropertyInfo property;
        PropertyInfo IPropertyPrintingConfig<TOwner, TPropType>.Property
            => property;


        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo property)
        {
            this.printingConfig = printingConfig;
            this.property = property;
        }
        public PrintingConfig<TOwner> Using(Func<TPropType, string> printFunction)
        {
            var objectFunction = new Func<object, string>(obj => printFunction((TPropType)obj));
            var config = (IPrintingConfig<TOwner>) printingConfig;
            if (property == null)
                config.CustomTypeSerialization.Add(typeof(TPropType),objectFunction);
            else
                config.CustomPropertySerialization.Add(property,objectFunction);
            return printingConfig;
        }
    }
}
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Blueprint.Tests.Http.HttpMessagePopulation_Middleware;

[TypeConverter(typeof(LengthConverter))]
public class Length
{
    public override string ToString()
    {
        string value;
        string unit;

        value = this.Value.ToString(CultureInfo.InvariantCulture);
        unit = this.Unit.ToString();

        return string.Concat(value, unit);
    }

    public Unit Unit { get; set; }

    public float Value { get; set; }
}

public enum Unit
{
    None,
    cm,
    mm,
    pt,
    px
}

public class LengthConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        object result = null;
        var stringValue = value as string;

        if (!string.IsNullOrEmpty(stringValue))
        {
            var nonDigitIndex = stringValue.IndexOf(stringValue.FirstOrDefault(char.IsLetter));

            if (nonDigitIndex > 0)
            {
                result = new Length
                {
                    Value = Convert.ToSingle(stringValue.Substring(0, nonDigitIndex)),
                    Unit = (Unit)Enum.Parse(typeof(Unit), stringValue.Substring(nonDigitIndex), true)
                };
            }
        }

        return result ?? base.ConvertFrom(context, culture, value);
    }
}
using System.Reflection;

namespace MailApp.Helpers;

public class AttributesHelper
{
    public static object GetPropertyAttribute(Type objectType, string propertyName, Type attributeType)
    {
        var propertyInfo = objectType.GetProperty(propertyName);
        if (propertyInfo is null)
            throw new InvalidOperationException("Object doesnt contain specified property");
        return propertyInfo.GetCustomAttributes().First(attr => attr.GetType() == attributeType);
    }
}

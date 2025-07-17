using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Neptune.NsPay.Commons
{
    public static class EnumExtensions
    {
        public static T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString());

            if (memberInfo.Length > 0)
            {
                var displayAttribute = memberInfo[0].GetCustomAttribute<DisplayAttribute>(false);

                if (!string.IsNullOrEmpty(displayAttribute?.Name))
                {
                    return displayAttribute.Name;
                }
            }

            // Fallback to enum name if DisplayAttribute is not found
            return enumValue.ToString();
        }
    }
}
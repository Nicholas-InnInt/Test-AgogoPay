﻿using System.Globalization;
using Neptune.NsPay.ApiClient;
using Neptune.NsPay.Core;
using Neptune.NsPay.Mobile.MAUI.Extensions;
using Neptune.NsPay.Mobile.MAUI.Resources.Localization;

namespace Neptune.NsPay.Localization
{
    public static class L
    {
        public static string Localize(string text)
        {
            return LocalizeInternal(text);
        }

        public static string Localize(string text, params object[] args)
        {
            return string.Format(LocalizeInternal(text), args);
        }

        public static string LocalizeWithThreeDots(string text, params object[] args)
        {
            var localizedText = Localize(text, args);
            return CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? "..." + localizedText : localizedText + "...";
        }

        public static string LocalizeWithParantheses(string text, object valueWithinParentheses, params object[] args)
        {
            var localizedText = Localize(text);
            return CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                ? " (" + valueWithinParentheses + ")" + localizedText
                : localizedText + " (" + valueWithinParentheses + ")";
        }

        private static string LocalizeInternal(string text)
        {
            if (ApplicationBootstrapper.AbpBootstrapper == null || text == null)
            {
                return text;
            }

            var appContext = ApplicationBootstrapper.AbpBootstrapper.IocManager.IocContainer.Resolve<IApplicationContext>();
            if (appContext.Configuration != null)
            {
                return appContext.Configuration.Localization.Localize(text);
            }

            var localTranslation = AppResources.ResourceManager.GetString(text);

            if (localTranslation == null)
            {
                throw new Exception(string.Format("Localization not found for given key {0}!", text));
            }

            return localTranslation;
        }
    }
}
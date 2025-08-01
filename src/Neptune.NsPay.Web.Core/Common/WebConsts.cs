﻿using System.Collections.Generic;

namespace Neptune.NsPay.Web.Common
{
    public static class WebConsts
    {
        public const string SwaggerUiEndPoint = "/swagger";
        public const string HangfireDashboardEndPoint = "/hangfire";

        public static bool SwaggerUiEnabled = false;
        public static bool HangfireDashboardEnabled = true;

        public static List<string> ReCaptchaIgnoreWhiteList = new List<string>
        {
            NsPayConsts.AbpApiClientUserAgent
        };

        public static class GraphQL
        {
            public const string PlaygroundEndPoint = "/ui/playground";
            public const string EndPoint = "/graphql";

            public static bool PlaygroundEnabled = true;
            public static bool Enabled = true;
        }
    }
}

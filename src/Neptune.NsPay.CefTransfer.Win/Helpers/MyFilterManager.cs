using CefSharp;

namespace Neptune.NsPay.CefTransfer.Win.Helpers
{
    public class MyFilterManager
    {
        private static Dictionary<string, IResponseFilter> dataList = new Dictionary<string, IResponseFilter>();

        public static IResponseFilter CreateFilter(string guid)
        {
            lock (dataList)
            {
                var filter = new MyIResponseFilter();
                dataList.Add(guid, filter);

                return filter;
            }
        }

        public static IResponseFilter GetFilter(string guid)
        {
            lock (dataList)
            {
                return dataList[guid];
            }
        }

        public static void Dispose()
        {
            dataList = new Dictionary<string, IResponseFilter>();
        }
    }
}

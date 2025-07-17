using Abp.Dependency;

namespace Neptune.NsPay.Web.Xss
{
    public interface IHtmlSanitizer: ITransientDependency
    {
        string Sanitize(string html);
    }
}
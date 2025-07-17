using Neptune.NsPay.Auditing;
using Neptune.NsPay.Test.Base;
using Shouldly;
using Xunit;

namespace Neptune.NsPay.Tests.Auditing
{
    // ReSharper disable once InconsistentNaming
    public class NamespaceStripper_Tests: AppTestBase
    {
        private readonly INamespaceStripper _namespaceStripper;

        public NamespaceStripper_Tests()
        {
            _namespaceStripper = Resolve<INamespaceStripper>();
        }

        [Fact]
        public void Should_Stripe_Namespace()
        {
            var controllerName = _namespaceStripper.StripNameSpace("Neptune.NsPay.Web.Controllers.HomeController");
            controllerName.ShouldBe("HomeController");
        }

        [Theory]
        [InlineData("Neptune.NsPay.Auditing.GenericEntityService`1[[Neptune.NsPay.Storage.BinaryObject, Neptune.NsPay.Core, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null]]", "GenericEntityService<BinaryObject>")]
        [InlineData("CompanyName.ProductName.Services.Base.EntityService`6[[CompanyName.ProductName.Entity.Book, CompanyName.ProductName.Core, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null],[CompanyName.ProductName.Services.Dto.Book.CreateInput, N...", "EntityService<Book, CreateInput>")]
        [InlineData("Neptune.NsPay.Auditing.XEntityService`1[Neptune.NsPay.Auditing.AService`5[[Neptune.NsPay.Storage.BinaryObject, Neptune.NsPay.Core, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null],[Neptune.NsPay.Storage.TestObject, Neptune.NsPay.Core, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null],]]", "XEntityService<AService<BinaryObject, TestObject>>")]
        public void Should_Stripe_Generic_Namespace(string serviceName, string result)
        {
            var genericServiceName = _namespaceStripper.StripNameSpace(serviceName);
            genericServiceName.ShouldBe(result);
        }
    }
}

using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit;
using Xunit;

namespace Nelson.Architecture.Refactor
{
    public interface IAuthorize
    {
        bool IsAuthorized();
    }
    public class RefactoredVersion
    {
        public decimal GetDiscountPrice(Product p)
        {
            return 0;
        }
    }

    public class RefactoringTests
    {
        [Theory, Gen]
        public void AuthorizedUserPasses(
            Product testProduct,
            [Frozen] Mock<IAuthorize> authorizer,
            RefactoredVersion sut)
        {
            sut.GetDiscountPrice(testProduct);
            // does not throw
        }
    }

    public class Gen : AutoDataAttribute
    {
        public Gen() : base(new Fixture().Customize(new AutoMoqCustomization())) { }
    }
}

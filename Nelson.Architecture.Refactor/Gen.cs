using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;

namespace Nelson.Architecture.Refactor
{
    public class Gen : AutoDataAttribute
    {
        public Gen() : base(new Fixture().Customize(new AutoMoqCustomization())) { }
    }
}
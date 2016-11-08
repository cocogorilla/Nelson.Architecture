using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.DataAnnotations;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Nelson.Architecture.Refactor
{
    public class RefactoringTests
    {
        [Theory, Gen]
        public void AuthorizableDiscountCalculatorExecutesWhenAuthorized(
            Product testProduct,
            decimal expected,
            [Frozen] Mock<IAuthorize> authorizer,
            [Frozen] Mock<IDiscountQuery> baseQuery,
            AuthorizableDiscountCalculator sut)
        {
            authorizer.Setup(x => x.IsAuthorized()).Returns(true);
            baseQuery.Setup(x => x.GetDiscountPrice(testProduct)).Returns(expected);
            var actual = sut.GetDiscountPrice(testProduct);
            Assert.Equal(expected, actual);
        }

        [Theory, Gen]
        public void AuthorizableDiscountCalculatorThrowsWhenUnauthorized(
            Product testProduct,
            [Frozen] Mock<IAuthorize> authorizer,
            AuthorizableDiscountCalculator sut)
        {
            authorizer.Setup(x => x.IsAuthorized()).Returns(false);
            Assert.Throws<UnauthorizedException>(() => sut.GetDiscountPrice(testProduct));
        }

        [Theory, Gen]
        public void PercentageDiscountStrategyCalculationIsCorrect(
            Product p,
            PercentageDiscountStrategy sut)
        {
            var discount = (decimal)new Random().NextDouble();
            Assert.True(discount > 0);
            var actual = sut.DiscountProduct(p, discount);
            Assert.Equal(p.Price * discount, actual);
        }

        [Theory, Gen]
        public void PercentageDiscountStrategyThrowsOnHighDiscount(
            Product p,
            decimal discount,
            PercentageDiscountStrategy sut)
        {
            if (discount < 1) discount = Math.Abs(discount) + 1;
            Assert.Throws<InvalidDiscountException>(() => sut.DiscountProduct(p, discount));
        }

        [Theory, Gen]
        public void PercentageDiscountStrategyThrowsOnLowDiscount(
         Product p,
         decimal discount,
         PercentageDiscountStrategy sut)
        {
            if (Math.Abs(discount) != 0)
                discount = -Math.Abs(discount);
            Assert.Throws<InvalidDiscountException>(() => sut.DiscountProduct(p, discount));
        }

        [Theory, Gen]
        public void MoneyOffDiscountStrategyCalculationIsCorrect(
            Product p,
            decimal discount,
            MoneyOffDiscountStrategy sut)
        {
            if (discount > p.Price)
                discount = discount - (discount - 1);
            var actual = sut.DiscountProduct(p, discount);
            Assert.Equal(p.Price - discount, actual);
        }

        [Theory, Gen]
        public void MoneyOffDiscountStrategyThrowsOnLowDiscount(
            Product p,
            decimal discount,
            MoneyOffDiscountStrategy sut)
        {
            if (Math.Abs(discount) != 0)
                discount = -discount;
            Assert.Throws<InvalidDiscountException>(() => sut.DiscountProduct(p, discount));
        }

        [Theory, Gen]
        public void MoneyOffDiscountStrategyThrowsOnHighDiscount(
            Product p,
            decimal discount,
            MoneyOffDiscountStrategy sut)
        {
            while (discount < p.Price) discount = discount + Math.Abs(discount);
            Assert.Throws<InvalidDiscountException>(() => sut.DiscountProduct(p, discount));
        }

        [Theory, Gen]
        public void PercentageDiscountStrategyShouldRunIsCorrect(
            Product testProduct,
            PercentageDiscountStrategy sut)
        {
            testProduct.DiscountType = "percentage";
            Assert.True(sut.ShouldRunFor(testProduct));
        }

        [Theory, Gen]
        public void PercentageDiscountStrategyShouldNotRunIsCorrect(
            Product testProduct,
            string bogusdiscounttype,
            PercentageDiscountStrategy sut)
        {
            testProduct.DiscountType = bogusdiscounttype;
            Assert.False(sut.ShouldRunFor(testProduct));
        }

        [Theory, Gen]
        public void MoneyOffDiscountStrategyShouldRunIsCorrect(
                Product testProduct,
                MoneyOffDiscountStrategy sut)
        {
            testProduct.DiscountType = "moneyoff";
            Assert.True(sut.ShouldRunFor(testProduct));
        }

        [Theory, Gen]
        public void MoneyOffDiscountStrategyShouldNotRunIsCorrect(
            Product testProduct,
            string bogusdiscounttype,
            MoneyOffDiscountStrategy sut)
        {
            testProduct.DiscountType = bogusdiscounttype;
            Assert.False(sut.ShouldRunFor(testProduct));
        }

        [Theory, Gen]
        public void UnknownDiscountStrategyShouldAlwaysRun(
            Product testProduct,
            UnknownDiscountStrategy sut)
        {
            Assert.Throws<UnknownDiscountTypeException>(() => sut.ShouldRunFor(testProduct));
        }

        [Theory, Gen]
        public void UnknownDiscountStrategyThrowsIfRun(
            Product p,
            decimal discount,
            UnknownDiscountStrategy sut)
        {
            Assert.Throws<InvalidOperationException>(() => { sut.DiscountProduct(p, discount); });
        }

        [Theory, Gen]
        public void RefactoredShouldRunOrchestrationIsCorrect(
            Product testProduct,
            decimal repoDiscount,
            decimal expectedDiscount,
            [Frozen] Mock<IDiscountRepository> repository,
            [Frozen] Mock<IDiscountStrategy> discountStrategy,
            RefactoredProductClass sut)
        {
            repository.Setup(x => x.GetDiscountForType(testProduct.DiscountType, testProduct.ProductType))
                .Returns(repoDiscount);
            discountStrategy.Setup(x => x.DiscountProduct(testProduct, repoDiscount))
                .Returns(expectedDiscount);

            var actual = sut.GetDiscountPrice(testProduct);

            Assert.Equal(expectedDiscount, actual);
        }

        [Theory, Gen]
        public void CompositeStrategyShouldRun(
            Product testProduct,
            RunFirstCompositeStrategy sut)
        {
            Assert.True(sut.ShouldRunFor(testProduct));
        }

        [Theory, Gen]
        public void CompositeStrategyShouldRunRandom(
            decimal dummyDiscount,
            Product testproduct,
            IEnumerable<Mock<IDiscountStrategy>> strategies,
            IFixture fixture)
        {
            foreach (var s in strategies)
            {
                s.Setup(x => x.ShouldRunFor(It.IsAny<Product>()))
                    .Returns(false);
            }
            var runthisone = strategies.OrderBy(x => Guid.NewGuid()).ToList().First();
            runthisone.Setup(x => x.ShouldRunFor(testproduct))
                .Returns(true);
            fixture.Inject(strategies.Select(x => x.Object));
            var sut = fixture.Create<RunFirstCompositeStrategy>();

            sut.DiscountProduct(testproduct, dummyDiscount);

            runthisone.Verify(x => x.DiscountProduct(testproduct, dummyDiscount), Times.Once());
            foreach (var s in strategies.Except(Enumerable.Repeat(runthisone, 1)))
            {
                s.Verify(x => x.DiscountProduct(It.IsAny<Product>(), It.IsAny<decimal>()), Times.Never);
            }
        }

        [Theory, Gen]
        public void CompositeStrategyShouldRunOnlyFirst(
            decimal dummyDiscount,
            Product testproduct,
            IEnumerable<Mock<IDiscountStrategy>> strategies,
            IFixture fixture)
        {
            foreach (var s in strategies)
            {
                s.Setup(x => x.ShouldRunFor(It.IsAny<Product>())).Returns(true);
            }
            fixture.Inject(strategies.Select(x => x.Object));
            var sut = fixture.Create<RunFirstCompositeStrategy>();

            sut.DiscountProduct(testproduct, dummyDiscount);

            strategies.First().Verify(x => x.DiscountProduct(testproduct, dummyDiscount), Times.Once());
            foreach (var s in strategies.Skip(1))
            {
                s.Verify(x => x.DiscountProduct(It.IsAny<Product>(), It.IsAny<decimal>()), Times.Never);
            }
        }

        [Theory, Gen]
        public void CompositeStrategyReturnIsCorrect(
            Product product,
            decimal discount,
            decimal expectedDiscount,
            [Frozen] Mock<IDiscountStrategy> strategy,
            RunFirstCompositeStrategy sut)
        {
            strategy.Setup(x => x.ShouldRunFor(product)).Returns(true);
            strategy.Setup(x => x.DiscountProduct(product, discount)).Returns(expectedDiscount);
            var actual = sut.DiscountProduct(product, discount);
            Assert.Equal(expectedDiscount, actual);
        }

        [Theory, Gen]
        public void CharacterizeRefactoredVersion(
            Product dummyProduct,
            Mock<IDiscountRepository> dummyRepository,
            Mock<IAuthorize> dummyAuthorizer)
        {
            dummyProduct.DiscountType = "percentage";
            dummyRepository.Setup(x => x.GetDiscountForType(
                It.IsAny<string>(),
                It.IsAny<string>())).Returns(.5M);
            dummyAuthorizer.Setup(x => x.IsAuthorized()).Returns(true);

            // composition root
            var percentoff = new PercentageDiscountStrategy();
            var moneyoff = new MoneyOffDiscountStrategy();
            var unknown = new UnknownDiscountStrategy();

            var runfirst = new RunFirstCompositeStrategy(new IDiscountStrategy[]
            {
        percentoff,
        moneyoff,
        unknown
            });
            var discountCalculator = new RefactoredProductClass(
                dummyRepository.Object,
                runfirst);
            var authorizedSut = new AuthorizableDiscountCalculator(
                dummyAuthorizer.Object,
                discountCalculator);
            // end composition root

            var actual = authorizedSut.GetDiscountPrice(dummyProduct);

            Assert.Equal(dummyProduct.Price * .5M, actual);
        }
    }
}
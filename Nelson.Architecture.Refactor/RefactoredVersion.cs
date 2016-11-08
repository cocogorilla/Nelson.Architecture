using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nelson.Architecture.Refactor
{
    public interface IAuthorize
    {
        bool IsAuthorized();
    }

    public interface IDiscountQuery
    {
        decimal GetDiscountPrice(Product p);
    }

    public interface IDiscountRepository
    {
        decimal GetDiscountForType(string discountType, string productType);
    }

    public interface IDiscountStrategy
    {
        decimal DiscountProduct(Product p, decimal discount);
        bool ShouldRunFor(Product p);
    }

    public class RefactoredProductClass : IDiscountQuery
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IDiscountStrategy _discountStrategy;

        public RefactoredProductClass(
            IDiscountRepository discountRepository,
            IDiscountStrategy discountStrategy)
        {

            if (discountRepository == null) throw new
                ArgumentNullException(nameof(discountRepository));
            if (discountStrategy == null) throw new
                ArgumentNullException(nameof(discountStrategy));
            _discountRepository = discountRepository;
            _discountStrategy = discountStrategy;
        }
        public decimal GetDiscountPrice(Product p)
        {
            var discount = _discountRepository.GetDiscountForType(
                p.DiscountType, p.ProductType);
            return _discountStrategy.DiscountProduct(p, discount);
        }
    }

    public class AuthorizableDiscountCalculator : IDiscountQuery
    {
        private readonly IAuthorize _authorizer;
        private readonly IDiscountQuery _baseQuery;

        public AuthorizableDiscountCalculator(
            IAuthorize authorizer,
            IDiscountQuery baseQuery)
        {

            if (authorizer == null) throw new
                ArgumentNullException(nameof(authorizer));
            if (baseQuery == null) throw new
                ArgumentNullException(nameof(baseQuery));
            _authorizer = authorizer;
            _baseQuery = baseQuery;
        }
        public decimal GetDiscountPrice(Product p)
        {
            if (_authorizer.IsAuthorized())
                return _baseQuery.GetDiscountPrice(p);
            throw new UnauthorizedException("user not authorized to get discount");
        }
    }

    public class RunFirstCompositeStrategy : IDiscountStrategy
    {
        private readonly IEnumerable<IDiscountStrategy> _strategies;

        public RunFirstCompositeStrategy(IEnumerable<IDiscountStrategy> strategies)
        {
            if (strategies == null) throw new ArgumentNullException(nameof(strategies));
            _strategies = strategies;
        }
        public decimal DiscountProduct(Product p, decimal discount)
        {
            return _strategies.First(x => x.ShouldRunFor(p)).DiscountProduct(p, discount);
        }

        public bool ShouldRunFor(Product p)
        {
            return true;
        }
    }

    public class PercentageDiscountStrategy : IDiscountStrategy
    {
        public decimal DiscountProduct(Product p, decimal discount)
        {
            if (discount > 1)
                throw new InvalidDiscountException("discount exceeded one");
            if (discount <= 0)
                throw new InvalidDiscountException("discount was under zero");
            return p.Price * discount;
        }

        public bool ShouldRunFor(Product p)
        {
            return p.DiscountType == "percentage";
        }
    }

    public class MoneyOffDiscountStrategy : IDiscountStrategy
    {
        public decimal DiscountProduct(Product p, decimal discount)
        {
            if (discount > p.Price)
                throw new InvalidDiscountException("cannot reduce more than price");
            if (discount < 0)
                throw new InvalidDiscountException("cannot reduce by negative discount");
            return p.Price - discount;
        }

        public bool ShouldRunFor(Product p)
        {
            return p.DiscountType == "moneyoff";
        }
    }

    public class UnknownDiscountStrategy : IDiscountStrategy
    {
        public decimal DiscountProduct(Product p, decimal discount)
        {
            throw new InvalidOperationException();
        }

        public bool ShouldRunFor(Product p)
        {
            throw new UnknownDiscountTypeException("unknown discount type");
        }
    }
}
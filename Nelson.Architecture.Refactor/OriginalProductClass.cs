using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace Nelson.Architecture.Refactor
{
    public class OriginalProductClass
    {
        private string ConnectionString => ConfigurationManager.ConnectionStrings["ProductAppConnString"].ConnectionString;
        public decimal GetDiscountPrice(Product p)
        {
            if (AuthContext.IsAuthorized())
            {
                if (p.DiscountType == "percent")
                {
                    using (var conn = new SqlConnection(ConnectionString))
                    {
                        var pdiscount = conn.ExecuteScalar<decimal>("GetDiscount", new
                        {
                            DiscountType = "percent",
                            p.ProductType
                        }, commandType: CommandType.StoredProcedure);
                        if (pdiscount < 0)
                            throw new InvalidDiscountException("Discount was less than zero");
                        if (pdiscount > 1)
                            throw new InvalidDiscountException("Discount was greater than one");
                        return p.Price * pdiscount;
                    }
                }
                else if (p.DiscountType == "moneyoff")
                {
                    using (var conn = new SqlConnection(ConnectionString))
                    {
                        var mdiscount = conn.ExecuteScalar<decimal>("GetDiscount", new
                        {
                            DiscountType = "moneyoff",
                            p.ProductType
                        }, commandType: CommandType.StoredProcedure);
                        if (mdiscount < 0)
                            throw new InvalidDiscountException("Discount was less than zero");
                        if (mdiscount > p.Price)
                            throw new InvalidDiscountException("Discount exceeded product price");
                        return p.Price - mdiscount;
                    }
                }
                else
                {
                    throw new UnknownDiscountTypeException("Unexpected Discount");
                }
            }
            else
            {
                throw new UnauthorizedException("user not authorized for discount");
            }
        }
    }
}
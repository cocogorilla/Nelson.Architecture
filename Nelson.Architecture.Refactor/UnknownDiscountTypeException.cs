using System;

namespace Nelson.Architecture.Refactor
{
    public class UnknownDiscountTypeException : Exception
    {
        public UnknownDiscountTypeException(string message) : base(message) { }
    }
}
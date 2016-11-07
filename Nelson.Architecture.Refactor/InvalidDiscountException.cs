using System;

namespace Nelson.Architecture.Refactor
{
    public class InvalidDiscountException : Exception
    {
        public InvalidDiscountException(string message) : base(message) { }
    }
}
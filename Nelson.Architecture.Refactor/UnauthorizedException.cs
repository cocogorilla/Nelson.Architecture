using System;

namespace Nelson.Architecture.Refactor
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}

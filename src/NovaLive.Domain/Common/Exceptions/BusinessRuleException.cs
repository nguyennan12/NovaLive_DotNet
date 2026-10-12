namespace NovaLive.Domain.Common.Exceptions;

public class BusinessRuleException : ApplicationException
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}

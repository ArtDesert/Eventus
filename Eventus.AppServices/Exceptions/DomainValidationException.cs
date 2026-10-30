namespace Eventus.AppServices.Exceptions;

public class DomainValidationException : Exception
{
    public string ProblemFieldName { get; }

    public DomainValidationException(string message, string problemFieldName)
        : base(message)
    {
        ProblemFieldName = problemFieldName;
    }

    public DomainValidationException(string message, string problemFieldName, Exception innerException)
        : base(message, innerException)
    {
        ProblemFieldName = problemFieldName;
    }
}
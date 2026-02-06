using System;

namespace Trolle.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a concurrency conflict is detected.
/// </summary>
public class ConcurrencyException : Exception
{
    public ConcurrencyException() 
        : base("The record has been modified or deleted by another user. Please refresh and try again.")
    {
    }

    public ConcurrencyException(string message) 
        : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
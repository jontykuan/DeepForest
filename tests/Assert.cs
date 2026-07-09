using System;
using System.Collections.Generic;

namespace DeepForest.Tests;

public class AssertionException : Exception
{
    public AssertionException(string message) : base(message)
    {
    }
}

public static class Assert
{
    public static void IsTrue(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw new AssertionException(message ?? "Expected true, but was false.");
        }
    }

    public static void IsFalse(bool condition, string? message = null)
    {
        if (condition)
        {
            throw new AssertionException(message ?? "Expected false, but was true.");
        }
    }

    public static void AreEqual<T>(T expected, T actual, string? message = null)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new AssertionException(message ?? $"Expected: {expected}, but was: {actual}");
        }
    }

    public static void AreNotEqual<T>(T expected, T actual, string? message = null)
    {
        if (EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new AssertionException(message ?? $"Expected not equal to {expected}, but they were equal.");
        }
    }

    public static void IsNull(object? value, string? message = null)
    {
        if (value is not null)
        {
            throw new AssertionException(message ?? $"Expected null, but was: {value}");
        }
    }

    public static void IsNotNull(object? value, string? message = null)
    {
        if (value is null)
        {
            throw new AssertionException(message ?? "Expected not null, but was null.");
        }
    }

    public static void Throws<TException>(Action action, string? message = null) where TException : Exception
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        try
        {
            action();
        }
        catch (TException)
        {
            // Expected exception was thrown
            return;
        }
        catch (Exception ex)
        {
            throw new AssertionException(message ?? $"Expected exception of type {typeof(TException).Name}, but caught {ex.GetType().Name}: {ex.Message}");
        }

        throw new AssertionException(message ?? $"Expected exception of type {typeof(TException).Name}, but no exception was thrown.");
    }
}

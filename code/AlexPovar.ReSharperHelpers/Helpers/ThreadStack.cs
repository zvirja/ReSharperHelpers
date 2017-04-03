using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace AlexPovar.ReSharperHelpers.Helpers
{
  /// <summary>
  ///   Thread bound stack. Allows to access pushed data from the same stack.
  ///   Useful if it's needed to pass data between methods that don't have API for that.
  /// </summary>
  public static class ThreadStack<TElementType>
  {
    [CanBeNull, ThreadStatic] private static Stack<TElementType> _stack;

    [NotNull] private static readonly IDisposable DisposableCleaner = new StackCleaner();

    /// <summary>
    ///   Get the latest pushed data (context data) or default (null).
    /// </summary>
    public static TElementType Current => GetCurrentValue();

    /// <summary>
    ///   Gets the latest pushed data. Allows to specify custom default value.
    /// </summary>
    public static TElementType GetCurrentValue(TElementType defValue = default(TElementType))
    {
      if ((_stack == null) || (_stack.Count == 0)) return defValue;

      return _stack.Peek();
    }

    /// <summary>
    ///   Add a new value to the stack.
    ///   Consider usage of <see cref="EnterScope" /> method to ensure value is always deleted.
    /// </summary>
    public static void PushValue(TElementType value)
    {
      if (_stack == null) _stack = new Stack<TElementType>();

      _stack.Push(value);
    }

    /// <summary>
    ///   Returns the last pushed data from stack.
    ///   Consider usage of <see cref="EnterScope" /> method to ensure value is always deleted.
    /// </summary>
    public static void PopValue(bool assertStackContainValue = true)
    {
      if ((_stack == null) || (_stack.Count == 0))
      {
        if (assertStackContainValue) throw new InvalidOperationException("Thread stack is empty. Unable to pop value.");
        return;
      }

      _stack.Pop();
    }

    /// <summary>
    ///   Sets the context while the returned disposable is alive.
    ///   Wrap return value in using() context to ensure value is always popped.
    /// </summary>
    [NotNull]
    public static IDisposable EnterScope(TElementType value)
    {
      PushValue(value);
      return DisposableCleaner;
    }

    private class StackCleaner : IDisposable
    {
      public void Dispose() => PopValue();
    }
  }
}
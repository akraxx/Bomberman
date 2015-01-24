using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// A generic unary EventArgs.
/// </summary>
/// <typeparam name="T">Desired payload type.</typeparam>
public class EventArgs<T> : EventArgs
{
    /// <summary>
    /// Payload of the unary EventArgs.
    /// </summary>
    public T Value { get; private set; }

    public EventArgs(T value)
    {
        Value = value;
    }
}
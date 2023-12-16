using System.ComponentModel;

namespace TCPServer.Services;

public class PropertyChangedWithValueEventArgs: PropertyChangedEventArgs
{
    public object Value { get; }
    
    public PropertyChangedWithValueEventArgs(string? propertyName, object value) : base(propertyName) 
        => Value = value;
}
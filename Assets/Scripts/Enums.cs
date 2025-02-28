using System;
using Random = UnityEngine.Random;

public enum EFruitType
{
    Avocado,
    Klubnika,
    Limon
}

public enum EPlateState
{
    Base,
    Svet,
}

public static partial class Extensions
{
    public static T RandomEnum<T>() where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum) { throw new Exception("random enum variable is not an enum"); }

        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(Random.Range(0, values.Length));
    }

    public static T RandomEnum<T>(int limit) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum) { throw new Exception("random enum variable is not an enum"); }

        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(Random.Range(0, limit));
    }
}
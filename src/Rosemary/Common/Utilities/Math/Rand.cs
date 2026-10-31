using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Utilities;

namespace Rosemary.Common;

public static class Rand
{
    public static UnifiedRandom Instance => Main.rand;

    /// <summary>
    ///     Generates a random value between 0 (inclusive) and <paramref name="max"/> (exclusive.)<br/>
    ///     It will not return <paramref name="max"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte Next(sbyte max) => (sbyte)(Instance.Sample() * max);

    /// <summary>
    ///     Generates a random value between <paramref name="min"/> (inclusive) and <paramref name="max"/> (exclusive.)<br/>
    ///     It will not return <paramref name="max"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte Next(sbyte min, sbyte max) => (sbyte)(Instance.Sample() * (max - min) + min);

    /// <inheritdoc cref="Next(sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Next(byte max) => (byte)(Instance.Sample() * max);

    /// <inheritdoc cref="Next(sbyte, sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Next(byte min, byte max) => (byte)(Instance.Sample() * (max - min) + min);

    /// <inheritdoc cref="Next(sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short Next(short max) => (short)(Instance.Sample() * max);

    /// <inheritdoc cref="Next(sbyte, sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short Next(short min, short max) => (short)(Instance.Sample() * (max - min) + min);

    /// <inheritdoc cref="Next(sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort Next(ushort max) => (ushort)(Instance.Sample() * max);

    /// <inheritdoc cref="Next(sbyte, sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort Next(ushort min, ushort max) => (ushort)(Instance.Sample() * (max - min) + min);

    /// <inheritdoc cref="Next(sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Next(int max) => (int)(Instance.Sample() * max);

    /// <inheritdoc cref="Next(sbyte, sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Next(int min, int max) => (int)(Instance.Sample() * (max - min) + min);

    /// <inheritdoc cref="Next(sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Next(uint max) => (uint)(Instance.Sample() * max);

    /// <inheritdoc cref="Next(sbyte, sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Next(uint min, uint max) => (uint)(Instance.Sample() * (max - min) + min);

    /// <inheritdoc cref="Next(sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Next(long max) => (long)(Instance.Sample() * max);

    /// <inheritdoc cref="Next(sbyte, sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Next(long min, long max) => (long)(Instance.Sample() * (max - min) + min);

    /// <inheritdoc cref="Next(sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Next(ulong max) => (ulong)(Instance.Sample() * max);

    /// <inheritdoc cref="Next(sbyte, sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Next(ulong min, ulong max) => (ulong)(Instance.Sample() * (max - min) + min);

    /// <inheritdoc cref="Next(sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Next(float max) => (float)(Instance.Sample() * max);

    /// <inheritdoc cref="Next(sbyte, sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Next(float min, float max) => (float)(Instance.Sample() * (max - min) + min);

    /// <inheritdoc cref="Next(sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Next(double max) => (Instance.Sample() * max);

    /// <inheritdoc cref="Next(sbyte, sbyte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Next(double min, double max) => (Instance.Sample() * (max - min) + min);

    /// <returns>
    ///     <see langword="true"/> <paramref name="antecedent"/> out of <paramref name="consequent"/> times.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NextBool(int consequent = 2, int antecedent = 1)
    {
        return Next(consequent) < antecedent;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Next(Vector2 size)
    {
        size.X *= Next(1f);
        size.Y *= Next(1f);

        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 NextSquare(float min, float max)
    {
        return new Vector2(Next(min, max), Next(min, max));
    }

    /// <summary>
    ///     Generates a random unit vector with the given radius.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 NextUnitVector(float radius = 1f)
    {
        return Instance.NextVector2Unit() * radius;
    }

    /// <summary>
    ///     Generates a random vector in the bounds of <paramref name="rect"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Next(Rectangle rect)
    {
        var position = new Vector2(Next(1f) * rect.Width, Next(1f) * rect.Height);

        position += rect.TopLeft();

        return position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NextDirection()
    {
        return NextBool() ? 1 : -1;
    }

    extension(FastRandom random)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat(float min, float max)
        {
            return random.NextFloat() * (max - min) + min;
        }
    }
}

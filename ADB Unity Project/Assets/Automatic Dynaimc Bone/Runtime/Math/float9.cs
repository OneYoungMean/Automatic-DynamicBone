// ReSharper disable InconsistentNaming - Using Unity math naming convention
// GENERATED CODE
using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Unity.Mathematics;

#pragma warning disable 0660, 0661

namespace Mathematics.Extensions
{
    [DebuggerTypeProxy(typeof(DebuggerProxy))]
    [Serializable]
    public struct float9 : IEquatable<float9>, IFormattable
    {
        public float r0;
        public float r1;
        public float r2;
        public float r3;
        public float r4;
        public float r5;
        public float r6;
        public float r7;
        public float r8;

        /// <summary>float9 zero value.</summary>
        // ReSharper disable once UnassignedReadonlyField - Purposefully using zeroed out value.
        public static readonly float9 zero;

        /// <summary>Constructs a float9 vector from float values.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9(float r0, float r1, float r2, float r3, float r4, float r5, float r6, float r7, float r8)
        {
            this.r0 = r0;
            this.r1 = r1;
            this.r2 = r2;
            this.r3 = r3;
            this.r4 = r4;
            this.r5 = r5;
            this.r6 = r6;
            this.r7 = r7;
            this.r8 = r8;
        }

        /// <summary>Constructs a float9 vector from three float3 vectors.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9(float3 r0r1r2, float3 r3r4r5, float3 r6r7r8)
        {
            r0 = r0r1r2.x;
            r1 = r0r1r2.y;
            r2 = r0r1r2.z;
            r3 = r3r4r5.x;
            r4 = r3r4r5.y;
            r5 = r3r4r5.z;
            r6 = r6r7r8.x;
            r7 = r6r7r8.y;
            r8 = r6r7r8.z;
        }

        /// <summary>Constructs a float9 vector from a float9 vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9(float9 r0r1r2r3r4r5r6r7r8)
        {
            r0 = r0r1r2r3r4r5r6r7r8.r0;
            r1 = r0r1r2r3r4r5r6r7r8.r1;
            r2 = r0r1r2r3r4r5r6r7r8.r2;
            r3 = r0r1r2r3r4r5r6r7r8.r3;
            r4 = r0r1r2r3r4r5r6r7r8.r4;
            r5 = r0r1r2r3r4r5r6r7r8.r5;
            r6 = r0r1r2r3r4r5r6r7r8.r6;
            r7 = r0r1r2r3r4r5r6r7r8.r7;
            r8 = r0r1r2r3r4r5r6r7r8.r8;
        }

        /// <summary>Constructs a float9 vector from a single float value by assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9(float v)
        {
            r0 = v;
            r1 = v;
            r2 = v;
            r3 = v;
            r4 = v;
            r5 = v;
            r6 = v;
            r7 = v;
            r8 = v;
        }

        /// <summary>Constructs a float9 vector from a single bool value by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9(bool v)
        {
            r0 = v ? 1.0f : 0.0f;
            r1 = v ? 1.0f : 0.0f;
            r2 = v ? 1.0f : 0.0f;
            r3 = v ? 1.0f : 0.0f;
            r4 = v ? 1.0f : 0.0f;
            r5 = v ? 1.0f : 0.0f;
            r6 = v ? 1.0f : 0.0f;
            r7 = v ? 1.0f : 0.0f;
            r8 = v ? 1.0f : 0.0f;
        }


        /// <summary>Constructs a float9 vector from a single int value by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9(int v)
        {
            r0 = v;
            r1 = v;
            r2 = v;
            r3 = v;
            r4 = v;
            r5 = v;
            r6 = v;
            r7 = v;
            r8 = v;
        }


        /// <summary>Constructs a float9 vector from a single uint value by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9(uint v)
        {
            r0 = v;
            r1 = v;
            r2 = v;
            r3 = v;
            r4 = v;
            r5 = v;
            r6 = v;
            r7 = v;
            r8 = v;
        }
        /// <summary>Constructs a float9 vector from a single half value by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9(half v)
        {
            r0 = v;
            r1 = v;
            r2 = v;
            r3 = v;
            r4 = v;
            r5 = v;
            r6 = v;
            r7 = v;
            r8 = v;
        }


        /// <summary>Constructs a float9 vector from a single double value by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9(double v)
        {
            r0 = (float)v;
            r1 = (float)v;
            r2 = (float)v;
            r3 = (float)v;
            r4 = (float)v;
            r5 = (float)v;
            r6 = (float)v;
            r7 = (float)v;
            r8 = (float)v;
        }


        /// <summary>Implicitly converts a single float value to a float9 vector by assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float9(float v) { return new float9(v); }

        /// <summary>Explicitly converts a single bool value to a float9 vector by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator float9(bool v) { return new float9(v); }
        /// <summary>Implicitly converts a single int value to a float9 vector by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float9(int v) { return new float9(v); }

        /// <summary>Implicitly converts a single uint value to a float9 vector by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float9(uint v) { return new float9(v); }
        /// <summary>Implicitly converts a single half value to a float9 vector by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float9(half v) { return new float9(v); }

        /// <summary>Explicitly converts a single double value to a float9 vector by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator float9(double v) { return new float9(v); }
        /// <summary>Returns the result of a componentwise multiplication operation on two float9 vectors.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator *(float9 lhs, float9 rhs) { return new float9(lhs.r0 * rhs.r0, lhs.r1 * rhs.r1, lhs.r2 * rhs.r2, lhs.r3 * rhs.r3, lhs.r4 * rhs.r4, lhs.r5 * rhs.r5, lhs.r6 * rhs.r6, lhs.r7 * rhs.r7, lhs.r8 * rhs.r8); }

        /// <summary>Returns the result of a componentwise multiplication operation on a float9 vector and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator *(float9 lhs, float rhs) { return new float9(lhs.r0 * rhs, lhs.r1 * rhs, lhs.r2 * rhs, lhs.r3 * rhs, lhs.r4 * rhs, lhs.r5 * rhs, lhs.r6 * rhs, lhs.r7 * rhs, lhs.r8 * rhs); }

        /// <summary>Returns the result of a componentwise multiplication operation on a float value and a float9 vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator *(float lhs, float9 rhs) { return new float9(lhs * rhs.r0, lhs * rhs.r1, lhs * rhs.r2, lhs * rhs.r3, lhs * rhs.r4, lhs * rhs.r5, lhs * rhs.r6, lhs * rhs.r7, lhs * rhs.r8); }


        /// <summary>Returns the result of a componentwise addition operation on two float9 vectors.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator +(float9 lhs, float9 rhs) { return new float9(lhs.r0 + rhs.r0, lhs.r1 + rhs.r1, lhs.r2 + rhs.r2, lhs.r3 + rhs.r3, lhs.r4 + rhs.r4, lhs.r5 + rhs.r5, lhs.r6 + rhs.r6, lhs.r7 + rhs.r7, lhs.r8 + rhs.r8); }

        /// <summary>Returns the result of a componentwise addition operation on a float9 vector and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator +(float9 lhs, float rhs) { return new float9(lhs.r0 + rhs, lhs.r1 + rhs, lhs.r2 + rhs, lhs.r3 + rhs, lhs.r4 + rhs, lhs.r5 + rhs, lhs.r6 + rhs, lhs.r7 + rhs, lhs.r8 + rhs); }

        /// <summary>Returns the result of a componentwise addition operation on a float value and a float9 vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator +(float lhs, float9 rhs) { return new float9(lhs + rhs.r0, lhs + rhs.r1, lhs + rhs.r2, lhs + rhs.r3, lhs + rhs.r4, lhs + rhs.r5, lhs + rhs.r6, lhs + rhs.r7, lhs + rhs.r8); }


        /// <summary>Returns the result of a componentwise subtraction operation on two float9 vectors.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator -(float9 lhs, float9 rhs) { return new float9(lhs.r0 - rhs.r0, lhs.r1 - rhs.r1, lhs.r2 - rhs.r2, lhs.r3 - rhs.r3, lhs.r4 - rhs.r4, lhs.r5 - rhs.r5, lhs.r6 - rhs.r6, lhs.r7 - rhs.r7, lhs.r8 - rhs.r8); }

        /// <summary>Returns the result of a componentwise subtraction operation on a float9 vector and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator -(float9 lhs, float rhs) { return new float9(lhs.r0 - rhs, lhs.r1 - rhs, lhs.r2 - rhs, lhs.r3 - rhs, lhs.r4 - rhs, lhs.r5 - rhs, lhs.r6 - rhs, lhs.r7 - rhs, lhs.r8 - rhs); }

        /// <summary>Returns the result of a componentwise subtraction operation on a float value and a float9 vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator -(float lhs, float9 rhs) { return new float9(lhs - rhs.r0, lhs - rhs.r1, lhs - rhs.r2, lhs - rhs.r3, lhs - rhs.r4, lhs - rhs.r5, lhs - rhs.r6, lhs - rhs.r7, lhs - rhs.r8); }


        /// <summary>Returns the result of a componentwise division operation on two float9 vectors.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator /(float9 lhs, float9 rhs) { return new float9(lhs.r0 / rhs.r0, lhs.r1 / rhs.r1, lhs.r2 / rhs.r2, lhs.r3 / rhs.r3, lhs.r4 / rhs.r4, lhs.r5 / rhs.r5, lhs.r6 / rhs.r6, lhs.r7 / rhs.r7, lhs.r8 / rhs.r8); }

        /// <summary>Returns the result of a componentwise division operation on a float9 vector and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator /(float9 lhs, float rhs) { return new float9(lhs.r0 / rhs, lhs.r1 / rhs, lhs.r2 / rhs, lhs.r3 / rhs, lhs.r4 / rhs, lhs.r5 / rhs, lhs.r6 / rhs, lhs.r7 / rhs, lhs.r8 / rhs); }

        /// <summary>Returns the result of a componentwise division operation on a float value and a float9 vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator /(float lhs, float9 rhs) { return new float9(lhs / rhs.r0, lhs / rhs.r1, lhs / rhs.r2, lhs / rhs.r3, lhs / rhs.r4, lhs / rhs.r5, lhs / rhs.r6, lhs / rhs.r7, lhs / rhs.r8); }


        /// <summary>Returns the result of a componentwise modulus operation on two float9 vectors.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator %(float9 lhs, float9 rhs) { return new float9(lhs.r0 % rhs.r0, lhs.r1 % rhs.r1, lhs.r2 % rhs.r2, lhs.r3 % rhs.r3, lhs.r4 % rhs.r4, lhs.r5 % rhs.r5, lhs.r6 % rhs.r6, lhs.r7 % rhs.r7, lhs.r8 % rhs.r8); }

        /// <summary>Returns the result of a componentwise modulus operation on a float9 vector and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator %(float9 lhs, float rhs) { return new float9(lhs.r0 % rhs, lhs.r1 % rhs, lhs.r2 % rhs, lhs.r3 % rhs, lhs.r4 % rhs, lhs.r5 % rhs, lhs.r6 % rhs, lhs.r7 % rhs, lhs.r8 % rhs); }

        /// <summary>Returns the result of a componentwise modulus operation on a float value and a float9 vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator %(float lhs, float9 rhs) { return new float9(lhs % rhs.r0, lhs % rhs.r1, lhs % rhs.r2, lhs % rhs.r3, lhs % rhs.r4, lhs % rhs.r5, lhs % rhs.r6, lhs % rhs.r7, lhs % rhs.r8); }


        /// <summary>Returns the result of a componentwise increment operation on a float9 vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator ++(float9 val) { return new float9(++val.r0, ++val.r1, ++val.r2, ++val.r3, ++val.r4, ++val.r5, ++val.r6, ++val.r7, ++val.r8); }


        /// <summary>Returns the result of a componentwise decrement operation on a float9 vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator --(float9 val) { return new float9(--val.r0, --val.r1, --val.r2, --val.r3, --val.r4, --val.r5, --val.r6, --val.r7, --val.r8); }


        /// <summary>Returns the result of a componentwise unary minus operation on a float9 vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator -(float9 val) { return new float9(-val.r0, -val.r1, -val.r2, -val.r3, -val.r4, -val.r5, -val.r6, -val.r7, -val.r8); }


        /// <summary>Returns the result of a componentwise unary plus operation on a float9 vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9 operator +(float9 val) { return new float9(+val.r0, +val.r1, +val.r2, +val.r3, +val.r4, +val.r5, +val.r6, +val.r7, +val.r8); }

        /// <summary>Returns the float element at a specified index.</summary>
        public unsafe ref float this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint)index >= 9)
                    throw new ArgumentException("index must be between[0...8]");
#endif
                fixed (float9* array = &this)
                {
                    return ref ((float*)array)[index];
                }
            }
        }

        /// <summary>Returns true if the float9 is equal to a given float9, false otherwise.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(float9 rhs) { return r0 == rhs.r0 && r1 == rhs.r1 && r2 == rhs.r2 && r3 == rhs.r3 && r4 == rhs.r4 && r5 == rhs.r5 && r6 == rhs.r6 && r7 == rhs.r7 && r8 == rhs.r8; }

        /// <summary>Returns true if the float9 is equal to a given float9, false otherwise.</summary>
        public override bool Equals(object o) { return o != null && Equals((float9)o); }


        /// <summary>Returns a hash code for the float9.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() { return (int)hash(); }


        /// <summary>Returns a string representation of the float9.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return string.Format("float9({0}f, {1}f, {2}f, {3}f, {4}f, {5}f, {6}f, {7}f, {8}f)", r0, r1, r2, r3, r4, r5, r6, r7, r8);
        }

        /// <summary>Returns a string representation of the float9 using a specified format and culture-specific format information.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("float9({0}f, {1}f, {2}f, {3}f, {4}f, {5}f, {6}f, {7}f, {8}f)", r0.ToString(format, formatProvider), r1.ToString(format, formatProvider), r2.ToString(format, formatProvider), r3.ToString(format, formatProvider), r4.ToString(format, formatProvider), r5.ToString(format, formatProvider), r6.ToString(format, formatProvider), r7.ToString(format, formatProvider), r8.ToString(format, formatProvider));
        }

        internal sealed class DebuggerProxy
        {
            public float r0;
            public float r1;
            public float r2;
            public float r3;
            public float r4;
            public float r5;
            public float r6;
            public float r7;
            public float r8;
            public DebuggerProxy(float9 v)
            {
                r0 = v.r0;
                r1 = v.r1;
                r2 = v.r2;
                r3 = v.r3;
                r4 = v.r4;
                r5 = v.r5;
                r6 = v.r6;
                r7 = v.r7;
                r8 = v.r8;
            }
        }


        /// <summary>Returns a uint hash code of a float9 vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint hash()
        {

            return math.asuint(r0) * 0x6E624EB7u +
                   math.asuint(r1) * 0x7383ED49u +
                   math.asuint(r2) * 0xDD49C23Bu +
                   math.asuint(r3) * 0xEBD0D005u +
                   math.asuint(r4) * 0x91475DF7u +
                   math.asuint(r5) * 0x55E84827u +
                   math.asuint(r6) * 0x90A285BBu +
                   math.asuint(r7) * 0x5D19E1D5u +
                   math.asuint(r8) * 0xFAAF07DDu +
                   0x625C45BDu;
        }
    }
}

// ReSharper disable InconsistentNaming - Using Unity math naming convention
// GENERATED CODE

using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

#pragma warning disable 0660, 0661

namespace Mathematics.Extensions
{
    [Serializable]
    public struct float9x9 : IEquatable<float9x9>, IFormattable
    {
        public float9 c0;
        public float9 c1;
        public float9 c2;
        public float9 c3;
        public float9 c4;
        public float9 c5;
        public float9 c6;
        public float9 c7;
        public float9 c8;

        /// <summary>float9x9 identity transform.</summary>
        public static readonly float9x9 identity = new float9x9(1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f);

        /// <summary>float9x9 zero value.</summary>
        // ReSharper disable once UnassignedReadonlyField - Purposefully using zeroed out value.
        public static readonly float9x9 zero;

        /// <summary>Constructs a float9x9 matrix from float9 vectors.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9x9(float9 c0, float9 c1, float9 c2, float9 c3, float9 c4, float9 c5, float9 c6, float9 c7, float9 c8)
        {
            this.c0 = c0;
            this.c1 = c1;
            this.c2 = c2;
            this.c3 = c3;
            this.c4 = c4;
            this.c5 = c5;
            this.c6 = c6;
            this.c7 = c7;
            this.c8 = c8;
        }

        /// <summary>Constructs a float9x9 matrix from 81 float values given in row-major order.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9x9(float m00, float m01, float m02, float m03, float m04, float m05, float m06, float m07, float m08,
                        float m10, float m11, float m12, float m13, float m14, float m15, float m16, float m17, float m18,
                        float m20, float m21, float m22, float m23, float m24, float m25, float m26, float m27, float m28,
                        float m30, float m31, float m32, float m33, float m34, float m35, float m36, float m37, float m38,
                        float m40, float m41, float m42, float m43, float m44, float m45, float m46, float m47, float m48,
                        float m50, float m51, float m52, float m53, float m54, float m55, float m56, float m57, float m58,
                        float m60, float m61, float m62, float m63, float m64, float m65, float m66, float m67, float m68,
                        float m70, float m71, float m72, float m73, float m74, float m75, float m76, float m77, float m78,
                        float m80, float m81, float m82, float m83, float m84, float m85, float m86, float m87, float m88)
        {
            c0 = new float9(m00, m10, m20, m30, m40, m50, m60, m70, m80);
            c1 = new float9(m01, m11, m21, m31, m41, m51, m61, m71, m81);
            c2 = new float9(m02, m12, m22, m32, m42, m52, m62, m72, m82);
            c3 = new float9(m03, m13, m23, m33, m43, m53, m63, m73, m83);
            c4 = new float9(m04, m14, m24, m34, m44, m54, m64, m74, m84);
            c5 = new float9(m05, m15, m25, m35, m45, m55, m65, m75, m85);
            c6 = new float9(m06, m16, m26, m36, m46, m56, m66, m76, m86);
            c7 = new float9(m07, m17, m27, m37, m47, m57, m67, m77, m87);
            c8 = new float9(m08, m18, m28, m38, m48, m58, m68, m78, m88);
        }

        /// <summary>Constructs a float9x9 matrix from a single float value by assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9x9(float v)
        {
            c0 = v;
            c1 = v;
            c2 = v;
            c3 = v;
            c4 = v;
            c5 = v;
            c6 = v;
            c7 = v;
            c8 = v;
        }


        /// <summary>Constructs a float9x9 matrix from a single int value by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9x9(int v)
        {
            c0 = v;
            c1 = v;
            c2 = v;
            c3 = v;
            c4 = v;
            c5 = v;
            c6 = v;
            c7 = v;
            c8 = v;
        }

        /// <summary>Constructs a float9x9 matrix from a single uint value by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9x9(uint v)
        {
            c0 = v;
            c1 = v;
            c2 = v;
            c3 = v;
            c4 = v;
            c5 = v;
            c6 = v;
            c7 = v;
            c8 = v;
        }


        /// <summary>Constructs a float9x9 matrix from a single double value by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9x9(double v)
        {
            c0 = (float9)v;
            c1 = (float9)v;
            c2 = (float9)v;
            c3 = (float9)v;
            c4 = (float9)v;
            c5 = (float9)v;
            c6 = (float9)v;
            c7 = (float9)v;
            c8 = (float9)v;
        }


        /// <summary>Implicitly converts a single float value to a float9x9 matrix by assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float9x9(float v) { return new float9x9(v); }

        /// <summary>Implicitly converts a single int value to a float9x9 matrix by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float9x9(int v) { return new float9x9(v); }
        /// <summary>Implicitly converts a single uint value to a float9x9 matrix by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float9x9(uint v) { return new float9x9(v); }
        /// <summary>Explicitly converts a single double value to a float9x9 matrix by converting it to float and assigning it to every component.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator float9x9(double v) { return new float9x9(v); }

        /// <summary>Returns the result of a componentwise multiplication operation on two float9x9 matrices.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator *(float9x9 lhs, float9x9 rhs) { return new float9x9(lhs.c0 * rhs.c0, lhs.c1 * rhs.c1, lhs.c2 * rhs.c2, lhs.c3 * rhs.c3, lhs.c4 * rhs.c4, lhs.c5 * rhs.c5, lhs.c6 * rhs.c6, lhs.c7 * rhs.c7, lhs.c8 * rhs.c8); }

        /// <summary>Returns the result of a componentwise multiplication operation on a float9x9 matrix and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator *(float9x9 lhs, float rhs) { return new float9x9(lhs.c0 * rhs, lhs.c1 * rhs, lhs.c2 * rhs, lhs.c3 * rhs, lhs.c4 * rhs, lhs.c5 * rhs, lhs.c6 * rhs, lhs.c7 * rhs, lhs.c8 * rhs); }

        /// <summary>Returns the result of a componentwise multiplication operation on a float value and a float9x9 matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator *(float lhs, float9x9 rhs) { return new float9x9(lhs * rhs.c0, lhs * rhs.c1, lhs * rhs.c2, lhs * rhs.c3, lhs * rhs.c4, lhs * rhs.c5, lhs * rhs.c6, lhs * rhs.c7, lhs * rhs.c8); }


        /// <summary>Returns the result of a componentwise addition operation on two float9x9 matrices.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator +(float9x9 lhs, float9x9 rhs) { return new float9x9(lhs.c0 + rhs.c0, lhs.c1 + rhs.c1, lhs.c2 + rhs.c2, lhs.c3 + rhs.c3, lhs.c4 + rhs.c4, lhs.c5 + rhs.c5, lhs.c6 + rhs.c6, lhs.c7 + rhs.c7, lhs.c8 + rhs.c8); }

        /// <summary>Returns the result of a componentwise addition operation on a float9x9 matrix and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator +(float9x9 lhs, float rhs) { return new float9x9(lhs.c0 + rhs, lhs.c1 + rhs, lhs.c2 + rhs, lhs.c3 + rhs, lhs.c4 + rhs, lhs.c5 + rhs, lhs.c6 + rhs, lhs.c7 + rhs, lhs.c8 + rhs); }

        /// <summary>Returns the result of a componentwise addition operation on a float value and a float9x9 matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator +(float lhs, float9x9 rhs) { return new float9x9(lhs + rhs.c0, lhs + rhs.c1, lhs + rhs.c2, lhs + rhs.c3, lhs + rhs.c4, lhs + rhs.c5, lhs + rhs.c6, lhs + rhs.c7, lhs + rhs.c8); }


        /// <summary>Returns the result of a componentwise subtraction operation on two float9x9 matrices.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator -(float9x9 lhs, float9x9 rhs) { return new float9x9(lhs.c0 - rhs.c0, lhs.c1 - rhs.c1, lhs.c2 - rhs.c2, lhs.c3 - rhs.c3, lhs.c4 - rhs.c4, lhs.c5 - rhs.c5, lhs.c6 - rhs.c6, lhs.c7 - rhs.c7, lhs.c8 - rhs.c8); }

        /// <summary>Returns the result of a componentwise subtraction operation on a float9x9 matrix and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator -(float9x9 lhs, float rhs) { return new float9x9(lhs.c0 - rhs, lhs.c1 - rhs, lhs.c2 - rhs, lhs.c3 - rhs, lhs.c4 - rhs, lhs.c5 - rhs, lhs.c6 - rhs, lhs.c7 - rhs, lhs.c8 - rhs); }

        /// <summary>Returns the result of a componentwise subtraction operation on a float value and a float9x9 matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator -(float lhs, float9x9 rhs) { return new float9x9(lhs - rhs.c0, lhs - rhs.c1, lhs - rhs.c2, lhs - rhs.c3, lhs - rhs.c4, lhs - rhs.c5, lhs - rhs.c6, lhs - rhs.c7, lhs - rhs.c8); }


        /// <summary>Returns the result of a componentwise division operation on two float9x9 matrices.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator /(float9x9 lhs, float9x9 rhs) { return new float9x9(lhs.c0 / rhs.c0, lhs.c1 / rhs.c1, lhs.c2 / rhs.c2, lhs.c3 / rhs.c3, lhs.c4 / rhs.c4, lhs.c5 / rhs.c5, lhs.c6 / rhs.c6, lhs.c7 / rhs.c7, lhs.c8 / rhs.c8); }

        /// <summary>Returns the result of a componentwise division operation on a float9x9 matrix and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator /(float9x9 lhs, float rhs) { return new float9x9(lhs.c0 / rhs, lhs.c1 / rhs, lhs.c2 / rhs, lhs.c3 / rhs, lhs.c4 / rhs, lhs.c5 / rhs, lhs.c6 / rhs, lhs.c7 / rhs, lhs.c8 / rhs); }

        /// <summary>Returns the result of a componentwise division operation on a float value and a float9x9 matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator /(float lhs, float9x9 rhs) { return new float9x9(lhs / rhs.c0, lhs / rhs.c1, lhs / rhs.c2, lhs / rhs.c3, lhs / rhs.c4, lhs / rhs.c5, lhs / rhs.c6, lhs / rhs.c7, lhs / rhs.c8); }


        /// <summary>Returns the result of a componentwise modulus operation on two float9x9 matrices.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator %(float9x9 lhs, float9x9 rhs) { return new float9x9(lhs.c0 % rhs.c0, lhs.c1 % rhs.c1, lhs.c2 % rhs.c2, lhs.c3 % rhs.c3, lhs.c4 % rhs.c4, lhs.c5 % rhs.c5, lhs.c6 % rhs.c6, lhs.c7 % rhs.c7, lhs.c8 % rhs.c8); }

        /// <summary>Returns the result of a componentwise modulus operation on a float9x9 matrix and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator %(float9x9 lhs, float rhs) { return new float9x9(lhs.c0 % rhs, lhs.c1 % rhs, lhs.c2 % rhs, lhs.c3 % rhs, lhs.c4 % rhs, lhs.c5 % rhs, lhs.c6 % rhs, lhs.c7 % rhs, lhs.c8 % rhs); }

        /// <summary>Returns the result of a componentwise modulus operation on a float value and a float9x9 matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator %(float lhs, float9x9 rhs) { return new float9x9(lhs % rhs.c0, lhs % rhs.c1, lhs % rhs.c2, lhs % rhs.c3, lhs % rhs.c4, lhs % rhs.c5, lhs % rhs.c6, lhs % rhs.c7, lhs % rhs.c8); }


        /// <summary>Returns the result of a componentwise increment operation on a float9x9 matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator ++(float9x9 val) { return new float9x9(++val.c0, ++val.c1, ++val.c2, ++val.c3, ++val.c4, ++val.c5, ++val.c6, ++val.c7, ++val.c8); }


        /// <summary>Returns the result of a componentwise decrement operation on a float9x9 matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator --(float9x9 val) { return new float9x9(--val.c0, --val.c1, --val.c2, --val.c3, --val.c4, --val.c5, --val.c6, --val.c7, --val.c8); }


        /// <summary>Returns the result of a componentwise unary minus operation on a float9x9 matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator -(float9x9 val) { return new float9x9(-val.c0, -val.c1, -val.c2, -val.c3, -val.c4, -val.c5, -val.c6, -val.c7, -val.c8); }


        /// <summary>Returns the result of a componentwise unary plus operation on a float9x9 matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float9x9 operator +(float9x9 val) { return new float9x9(+val.c0, +val.c1, +val.c2, +val.c3, +val.c4, +val.c5, +val.c6, +val.c7, +val.c8); }



        /// <summary>Returns the float9 element at a specified index.</summary>
        unsafe public ref float9 this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint)index >= 9)
                    throw new ArgumentException("index must be between[0...8]");
#endif
                fixed (float9x9* array = &this) { return ref ((float9*)array)[index]; }
            }
        }

        /// <summary>Returns true if the float9x9 is equal to a given float9x9, false otherwise.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(float9x9 rhs) { return c0.Equals(rhs.c0) && c1.Equals(rhs.c1) && c2.Equals(rhs.c2) && c3.Equals(rhs.c3) && c4.Equals(rhs.c4) && c5.Equals(rhs.c5) && c6.Equals(rhs.c6) && c7.Equals(rhs.c7) && c8.Equals(rhs.c8); }

        /// <summary>Returns true if the float9x9 is equal to a given float9x9, false otherwise.</summary>
        public override bool Equals(object o) { return o!= null && Equals((float9x9)o); }


        /// <summary>Returns a string representation of the float9x9.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return string.Format("float9x9(\n{0}f, {1}f, {2}f, {3}f, {4}f, {5}f, {6}f, {7}f, {8}f,\n{9}f, {10}f, {11}f, {12}f, {13}f, {14}f, {15}f, {16}f, {17}f,\n{18}f, {19}f, {20}f, {21}f, {22}f, {23}f, {24}f, {25}f, {26}f,\n{27}f, {28}f, {29}f, {30}f, {31}f, {32}f, {33}f, {34}f, {35}f,\n{36}f, {37}f, {38}f, {39}f, {40}f, {41}f, {42}f, {43}f, {44}f,\n{45}f, {46}f, {47}f, {48}f, {49}f, {50}f, {51}f, {52}f, {53}f,\n{54}f, {55}f, {56}f, {57}f, {58}f, {59}f, {60}f, {61}f, {62}f,\n{63}f, {64}f, {65}f, {66}f, {67}f, {68}f, {69}f, {70}f, {71}f,\n{72}f, {73}f, {74}f, {75}f, {76}f, {77}f, {78}f, {79}f, {80}f)", c0.r0, c1.r0, c2.r0, c3.r0, c4.r0, c5.r0, c6.r0, c7.r0, c8.r0, c0.r1, c1.r1, c2.r1, c3.r1, c4.r1, c5.r1, c6.r1, c7.r1, c8.r1, c0.r2, c1.r2, c2.r2, c3.r2, c4.r2, c5.r2, c6.r2, c7.r2, c8.r2, c0.r3, c1.r3, c2.r3, c3.r3, c4.r3, c5.r3, c6.r3, c7.r3, c8.r3, c0.r4, c1.r4, c2.r4, c3.r4, c4.r4, c5.r4, c6.r4, c7.r4, c8.r4, c0.r5, c1.r5, c2.r5, c3.r5, c4.r5, c5.r5, c6.r5, c7.r5, c8.r5, c0.r6, c1.r6, c2.r6, c3.r6, c4.r6, c5.r6, c6.r6, c7.r6, c8.r6, c0.r7, c1.r7, c2.r7, c3.r7, c4.r7, c5.r7, c6.r7, c7.r7, c8.r7, c0.r8, c1.r8, c2.r8, c3.r8, c4.r8, c5.r8, c6.r8, c7.r8, c8.r8);
        }

        /// <summary>Returns a string representation of the float9x9 using a specified format and culture-specific format information.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("float9x9({0}f, {1}f, {2}f, {3}f, {4}f, {5}f, {6}f, {7}f, {8}f,  {9}f, {10}f, {11}f, {12}f, {13}f, {14}f, {15}f, {16}f, {17}f,  {18}f, {19}f, {20}f, {21}f, {22}f, {23}f, {24}f, {25}f, {26}f,  {27}f, {28}f, {29}f, {30}f, {31}f, {32}f, {33}f, {34}f, {35}f,  {36}f, {37}f, {38}f, {39}f, {40}f, {41}f, {42}f, {43}f, {44}f,  {45}f, {46}f, {47}f, {48}f, {49}f, {50}f, {51}f, {52}f, {53}f,  {54}f, {55}f, {56}f, {57}f, {58}f, {59}f, {60}f, {61}f, {62}f,  {63}f, {64}f, {65}f, {66}f, {67}f, {68}f, {69}f, {70}f, {71}f,  {72}f, {73}f, {74}f, {75}f, {76}f, {77}f, {78}f, {79}f, {80}f)", c0.r0.ToString(format, formatProvider), c1.r0.ToString(format, formatProvider), c2.r0.ToString(format, formatProvider), c3.r0.ToString(format, formatProvider), c4.r0.ToString(format, formatProvider), c5.r0.ToString(format, formatProvider), c6.r0.ToString(format, formatProvider), c7.r0.ToString(format, formatProvider), c8.r0.ToString(format, formatProvider), c0.r1.ToString(format, formatProvider), c1.r1.ToString(format, formatProvider), c2.r1.ToString(format, formatProvider), c3.r1.ToString(format, formatProvider), c4.r1.ToString(format, formatProvider), c5.r1.ToString(format, formatProvider), c6.r1.ToString(format, formatProvider), c7.r1.ToString(format, formatProvider), c8.r1.ToString(format, formatProvider), c0.r2.ToString(format, formatProvider), c1.r2.ToString(format, formatProvider), c2.r2.ToString(format, formatProvider), c3.r2.ToString(format, formatProvider), c4.r2.ToString(format, formatProvider), c5.r2.ToString(format, formatProvider), c6.r2.ToString(format, formatProvider), c7.r2.ToString(format, formatProvider), c8.r2.ToString(format, formatProvider), c0.r3.ToString(format, formatProvider), c1.r3.ToString(format, formatProvider), c2.r3.ToString(format, formatProvider), c3.r3.ToString(format, formatProvider), c4.r3.ToString(format, formatProvider), c5.r3.ToString(format, formatProvider), c6.r3.ToString(format, formatProvider), c7.r3.ToString(format, formatProvider), c8.r3.ToString(format, formatProvider), c0.r4.ToString(format, formatProvider), c1.r4.ToString(format, formatProvider), c2.r4.ToString(format, formatProvider), c3.r4.ToString(format, formatProvider), c4.r4.ToString(format, formatProvider), c5.r4.ToString(format, formatProvider), c6.r4.ToString(format, formatProvider), c7.r4.ToString(format, formatProvider), c8.r4.ToString(format, formatProvider), c0.r5.ToString(format, formatProvider), c1.r5.ToString(format, formatProvider), c2.r5.ToString(format, formatProvider), c3.r5.ToString(format, formatProvider), c4.r5.ToString(format, formatProvider), c5.r5.ToString(format, formatProvider), c6.r5.ToString(format, formatProvider), c7.r5.ToString(format, formatProvider), c8.r5.ToString(format, formatProvider), c0.r6.ToString(format, formatProvider), c1.r6.ToString(format, formatProvider), c2.r6.ToString(format, formatProvider), c3.r6.ToString(format, formatProvider), c4.r6.ToString(format, formatProvider), c5.r6.ToString(format, formatProvider), c6.r6.ToString(format, formatProvider), c7.r6.ToString(format, formatProvider), c8.r6.ToString(format, formatProvider), c0.r7.ToString(format, formatProvider), c1.r7.ToString(format, formatProvider), c2.r7.ToString(format, formatProvider), c3.r7.ToString(format, formatProvider), c4.r7.ToString(format, formatProvider), c5.r7.ToString(format, formatProvider), c6.r7.ToString(format, formatProvider), c7.r7.ToString(format, formatProvider), c8.r7.ToString(format, formatProvider), c0.r8.ToString(format, formatProvider), c1.r8.ToString(format, formatProvider), c2.r8.ToString(format, formatProvider), c3.r8.ToString(format, formatProvider), c4.r8.ToString(format, formatProvider), c5.r8.ToString(format, formatProvider), c6.r8.ToString(format, formatProvider), c7.r8.ToString(format, formatProvider), c8.r8.ToString(format, formatProvider));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return new float9(
                math.asfloat(c0.hash()),
                math.asfloat(c1.hash()),
                math.asfloat(c2.hash()),
                math.asfloat(c3.hash()),
                math.asfloat(c4.hash()),
                math.asfloat(c5.hash()),
                math.asfloat(c6.hash()),
                math.asfloat(c7.hash()),
                math.asfloat(c8.hash())
            ).GetHashCode();
        }

        /// <summary>Return the float9x9 transpose of a float9x9 matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9x9 transpose()
        {
            return new float9x9(
                c0.r0, c0.r1, c0.r2, c0.r3, c0.r4, c0.r5, c0.r6, c0.r7, c0.r8,
                c1.r0, c1.r1, c1.r2, c1.r3, c1.r4, c1.r5, c1.r6, c1.r7, c1.r8,
                c2.r0, c2.r1, c2.r2, c2.r3, c2.r4, c2.r5, c2.r6, c2.r7, c2.r8,
                c3.r0, c3.r1, c3.r2, c3.r3, c3.r4, c3.r5, c3.r6, c3.r7, c3.r8,
                c4.r0, c4.r1, c4.r2, c4.r3, c4.r4, c4.r5, c4.r6, c4.r7, c4.r8,
                c5.r0, c5.r1, c5.r2, c5.r3, c5.r4, c5.r5, c5.r6, c5.r7, c5.r8,
                c6.r0, c6.r1, c6.r2, c6.r3, c6.r4, c6.r5, c6.r6, c6.r7, c6.r8,
                c7.r0, c7.r1, c7.r2, c7.r3, c7.r4, c7.r5, c7.r6, c7.r7, c7.r8,
                c8.r0, c8.r1, c8.r2, c8.r3, c8.r4, c8.r5, c8.r6, c8.r7, c8.r8);
        }
        // Gauss-Jordan inverse algorithm. 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float9x9 inverse()
        {
            // rows are columns, ok?
            // index using [row][column]
            float9x9 aT = transpose();
            float9x9 augmented = float9x9.identity;

            for (int column = 0; column < 9; column++)
            {

                //Find Pivot
                float max = 0;
                int pivot = column;

                for (int row = column; row < 9; row++)
                {
                    var val = math.abs(aT[row][column]);
                    if (val > max)
                    {
                        pivot = row;
                        max = val;
                    }
                }
                if (max <= 0) throw new ArithmeticException("Un-invertible Matrix");

                // Row Swap if necessary
                if (pivot != column)
                {
                    // these are all rows.
                    var temp = aT[column];
                    aT[column] = aT[pivot];
                    aT[pivot] = temp;

                    temp = augmented[column]; // this is a row.
                    augmented[column] = augmented[pivot];
                    augmented[pivot] = temp;
                }

                // Row scale to set diagonals to 1
                {
                    var scale = aT[column][column];

                    aT[column] /= scale;
                    augmented[column] /= scale;
                }

                // Eliminate.
                for (int row = 0; row < 9; row++)
                {
                    if (row == column)
                    {
                        if (aT[row][column] != 1)
                        {
                            throw new ArithmeticException("Un-available Matrix");
                        }
                        continue;
                    }

                    var scale = aT[row][column];
                    if (aT[row][column] == 0) continue;

                    aT[row] -= aT[column] * scale;
                    augmented[row] -= augmented[column] * scale;
                }
            }

            return augmented.transpose();
        }

    }
}

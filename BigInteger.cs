using System;
using System.Security.Cryptography;

// compile with: /doc:BigIntegerDocComment.xml

/// <summary>
/// This is a BigInteger class. Holds integer that is more than 64-bit (long).
/// </summary>
/// <remarks>
/// This class contains overloaded arithmetic operators(+, -, *, /, %), bitwise operators(&amp;, |) and other
/// operations that can be done with normal integer. It also contains implementation of various prime test.
/// This class also contains methods dealing with cryptography such as generating prime number, generating 
/// a coprime number.
/// </remarks>

public class BigInteger
{
    // maximum length of the BigInteger in uint (4 bytes)
    // change this to suit the required level of precision.
    private const int maxLength = 70;

    // primes smaller than 2000 to test the generated prime number
    public static readonly int[] primesBelow2000 = {
           2,    3,    5,    7,   11,   13,   17,   19,   23,   29,   31,   37,   41,   43,   47,   53,   59,   61,   67,   71,
          73,   79,   83,   89,   97,  101,  103,  107,  109,  113,  127,  131,  137,  139,  149,  151,  157,  163,  167,  173,
         179,  181,  191,  193,  197,  199,  211,  223,  227,  229,  233,  239,  241,  251,  257,  263,  269,  271,  277,  281,
         283,  293,  307,  311,  313,  317,  331,  337,  347,  349,  353,  359,  367,  373,  379,  383,  389,  397,  401,  409,
         419,  421,  431,  433,  439,  443,  449,  457,  461,  463,  467,  479,  487,  491,  499,  503,  509,  521,  523,  541,
         547,  557,  563,  569,  571,  577,  587,  593,  599,  601,  607,  613,  617,  619,  631,  641,  643,  647,  653,  659,
         661,  673,  677,  683,  691,  701,  709,  719,  727,  733,  739,  743,  751,  757,  761,  769,  773,  787,  797,  809,
         811,  821,  823,  827,  829,  839,  853,  857,  859,  863,  877,  881,  883,  887,  907,  911,  919,  929,  937,  941,
         947,  953,  967,  971,  977,  983,  991,  997, 1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069,
        1087, 1091, 1093, 1097, 1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181, 1187, 1193, 1201, 1213, 1217, 1223,
        1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289, 1291, 1297, 1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373,
        1381, 1399, 1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471, 1481, 1483, 1487, 1489, 1493, 1499, 1511,
        1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583, 1597, 1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657,
        1663, 1667, 1669, 1693, 1697, 1699, 1709, 1721, 1723, 1733, 1741, 1747, 1753, 1759, 1777, 1783, 1787, 1789, 1801, 1811,
        1823, 1831, 1847, 1861, 1867, 1871, 1873, 1877, 1879, 1889, 1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973, 1979, 1987,
        1993, 1997, 1999 };

    private uint[] data = null;            // stores bytes from the Big Integer
    public int dataLength;                 // number of actual chars used

    /// Default constructor for BigInteger of value 0
    public BigInteger()
    {
        data = new uint[maxLength];
        dataLength = 1;
    }

    /// <summary>
    /// Constructor (Default value provided by long)
    /// </summary>
    /// <param name="value">Turn the long value into BigInteger type</param>
    public BigInteger(long value)
    {
        data = new uint[maxLength];
        long tempVal = value;

        // copy bytes from long to BigInteger without any assumption of
        // the length of the long datatype
        dataLength = 0;
        while (value != 0 && dataLength < maxLength)
        {
            data[dataLength] = (uint)(value & 0xFFFFFFFF);
            value >>= 32;
            dataLength++;
        }

        if (tempVal > 0)         // overflow check for +ve value
        {
            if (value != 0 || (data[maxLength - 1] & 0x80000000) != 0)
                throw (new ArithmeticException("Positive overflow in constructor."));
        }
        else if (tempVal < 0)    // underflow check for -ve value
        {
            if (value != -1 || (data[dataLength - 1] & 0x80000000) == 0)
                throw (new ArithmeticException("Negative underflow in constructor."));
        }

        if (dataLength == 0)
            dataLength = 1;
    }

    /// Constructor (Default value provided by ulong)
    /// <param name="value">Turn the unsigned long value into BigInteger type</param>
    public BigInteger(ulong value)
    {
        data = new uint[maxLength];

        // copy bytes from ulong to BigInteger without any assumption of
        // the length of the ulong datatype
        dataLength = 0;
        while (value != 0 && dataLength < maxLength)
        {
            data[dataLength] = (uint)(value & 0xFFFFFFFF);
            value >>= 32;
            dataLength++;
        }

        if (value != 0 || (data[maxLength - 1] & 0x80000000) != 0)
            throw (new ArithmeticException("Positive overflow in constructor."));

        if (dataLength == 0)
            dataLength = 1;
    }

    ///Constructor (Default value provided by BigInteger)
    //<param name="bi"></param>
    public BigInteger(BigInteger bi)
    {
        data = new uint[maxLength];

        dataLength = bi.dataLength;

        for (int i = 0; i < dataLength; i++)
            data[i] = bi.data[i];
    }

    ///                     Constructor (Default value provided by a string of digits of the specified base)
    /// <example>           To initialize "a" with the default value of 1234 in base 10:   BigInteger a = new BigInteger("1234", 10)
    ///                     To initialize "a" with the default value of -0x1D4F in base 16: BigInteger a = new BigInteger("-1D4F", 16) </example>
    // <param name="value">String value in the format of [sign][magnitude]</param><param name="radix">The base of value</param>
    public BigInteger(string value, int radix)
    {
        BigInteger multiplier = new BigInteger(1);
        BigInteger result = new BigInteger();
        value = (value.ToUpper()).Trim();
        int limit = 0;

        if (value[0] == '-')
            limit = 1;

        for (int i = value.Length - 1; i >= limit; i--)
        {
            int posVal = (int)value[i];

            if (posVal >= '0' && posVal <= '9')
                posVal -= '0';
            else if (posVal >= 'A' && posVal <= 'Z')
                posVal = (posVal - 'A') + 10;
            else
                posVal = 9999999;       // arbitrary large


            if (posVal >= radix)
                throw (new ArithmeticException("Invalid string in constructor."));
            else
            {
                if (value[0] == '-')
                    posVal = -posVal;

                result = result + (multiplier * posVal);

                if ((i - 1) >= limit)
                    multiplier = multiplier * radix;
            }
        }

        if (value[0] == '-')     // negative values
        {
            if ((result.data[maxLength - 1] & 0x80000000) == 0)
                throw (new ArithmeticException("Negative underflow in constructor."));
        }
        else    // positive values
        {
            if ((result.data[maxLength - 1] & 0x80000000) != 0)
                throw (new ArithmeticException("Positive overflow in constructor."));
        }

        data = new uint[maxLength];
        for (int i = 0; i < result.dataLength; i++)
            data[i] = result.data[i];

        dataLength = result.dataLength;
    }

    ///                     Constructor (Default value provided by an array of bytes of the specified length.)
    //<param name="inData"> A list of byte values</param> <param name="length">Default -1</param> <param name="offset">Default 0</param>
    public BigInteger(System.Collections.Generic.IList<byte> inData, int length = -1, int offset = 0)
    {
        var inLen = length == -1 ? inData.Count - offset : length;

        dataLength = inLen >> 2;

        int leftOver = inLen & 0x3;
        if (leftOver != 0)         // length not multiples of 4
            dataLength++;

        if (dataLength > maxLength || inLen > inData.Count - offset)
            throw (new ArithmeticException("Byte overflow in constructor."));


        data = new uint[maxLength];

        for (int i = inLen - 1, j = 0; i >= 3; i -= 4, j++)
        {
            data[j] = (uint)((inData[offset + i - 3] << 24) + (inData[offset + i - 2] << 16) +
                             (inData[offset + i - 1] << 8) + inData[offset + i]);
        }

        if (leftOver == 1)
            data[dataLength - 1] = (uint)inData[offset + 0];
        else if (leftOver == 2)
            data[dataLength - 1] = (uint)((inData[offset + 0] << 8) + inData[offset + 1]);
        else if (leftOver == 3)
            data[dataLength - 1] = (uint)((inData[offset + 0] << 16) + (inData[offset + 1] << 8) + inData[offset + 2]);


        if (dataLength == 0)
            dataLength = 1;

        while (dataLength > 1 && data[dataLength - 1] == 0)
            dataLength--;
    }

    ///                     Constructor (Default value provided by an array of unsigned integers)
    //<param name="inData"> Array of unsigned integer</param>
    public BigInteger(uint[] inData)
    {
        dataLength = inData.Length;

        if (dataLength > maxLength)
            throw (new ArithmeticException("Byte overflow in constructor."));

        data = new uint[maxLength];

        for (int i = dataLength - 1, j = 0; i >= 0; i--, j++)
            data[j] = inData[i];

        while (dataLength > 1 && data[dataLength - 1] == 0)
            dataLength--;
    }

    ///                     Cast a type long value to type BigInteger value
    //<param name="value">  A long value</param>
    public static implicit operator BigInteger(long value)
    {
        return (new BigInteger(value));
    }

    ///                     Cast a type ulong value to type BigInteger value
    //<param name="value">  An unsigned long value</param>
    public static implicit operator BigInteger(ulong value)
    {
        return (new BigInteger(value));
    }

    ///                     Cast a type int value to type BigInteger value
    //<param name="value">  An int value</param>
    public static implicit operator BigInteger(int value)
    {
        return (new BigInteger((long)value));
    }

    ///                     Cast a type uint value to type BigInteger value
    //<param name="value">  An unsigned int value</param>
    public static implicit operator BigInteger(uint value)
    {
        return (new BigInteger((ulong)value));
    }

    ///                     Overloading of addition operator
    // <param name="bi1">   First BigInteger</param> <param name="bi2">Second BigInteger</param>
    /// <returns>           Result of the addition of 2 BigIntegers</returns>
    public static BigInteger operator +(BigInteger bi1, BigInteger bi2)
    {
        BigInteger result = new BigInteger()
        {
            dataLength = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength
        };

        long carry = 0;
        for (int i = 0; i < result.dataLength; i++)
        {
            long sum = (long)bi1.data[i] + (long)bi2.data[i] + carry;
            carry = sum >> 32;
            result.data[i] = (uint)(sum & 0xFFFFFFFF);
        }

        if (carry != 0 && result.dataLength < maxLength)
        {
            result.data[result.dataLength] = (uint)(carry);
            result.dataLength++;
        }

        while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
            result.dataLength--;

        // overflow check
        int lastPos = maxLength - 1;
        if ((bi1.data[lastPos] & 0x80000000) == (bi2.data[lastPos] & 0x80000000) &&
           (result.data[lastPos] & 0x80000000) != (bi1.data[lastPos] & 0x80000000))
        {
            throw (new ArithmeticException());
        }

        return result;
    }

    ///                     Overloading of the unary ++ operator, which increments BigInteger by 1
    // <param name="bi1">   A BigInteger</param>
    /// <returns>           Incremented BigInteger</returns>
    public static BigInteger operator ++(BigInteger bi1)
    {
        BigInteger result = new BigInteger(bi1);

        long val, carry = 1;
        int index = 0;

        while (carry != 0 && index < maxLength)
        {
            val = (long)(result.data[index]);
            val++;

            result.data[index] = (uint)(val & 0xFFFFFFFF);
            carry = val >> 32;

            index++;
        }

        if (index > result.dataLength)
            result.dataLength = index;
        else
        {
            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
                result.dataLength--;
        }

        // overflow check
        int lastPos = maxLength - 1;

        // overflow if initial value was +ve but ++ caused a sign
        // change to negative.

        if ((bi1.data[lastPos] & 0x80000000) == 0 &&
           (result.data[lastPos] & 0x80000000) != (bi1.data[lastPos] & 0x80000000))
        {
            throw (new ArithmeticException("Overflow in ++."));
        }
        return result;
    }

    ///                     Overloading of subtraction operator
    // <param name="bi1">   First BigInteger</param> <param name="bi2">Second BigInteger</param>
    /// <returns>           Result of the subtraction of 2 BigIntegers</returns>
    public static BigInteger operator -(BigInteger bi1, BigInteger bi2)
    {
        BigInteger result = new BigInteger()
        {
            dataLength = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength
        };

        long carryIn = 0;
        for (int i = 0; i < result.dataLength; i++)
        {
            long diff;

            diff = (long)bi1.data[i] - (long)bi2.data[i] - carryIn;
            result.data[i] = (uint)(diff & 0xFFFFFFFF);

            if (diff < 0)
                carryIn = 1;
            else
                carryIn = 0;
        }

        // roll over to negative
        if (carryIn != 0)
        {
            for (int i = result.dataLength; i < maxLength; i++)
                result.data[i] = 0xFFFFFFFF;
            result.dataLength = maxLength;
        }

        // fixed in v1.03 to give correct datalength for a - (-b)
        while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
            result.dataLength--;

        // overflow check
        int lastPos = maxLength - 1;
        if ((bi1.data[lastPos] & 0x80000000) != (bi2.data[lastPos] & 0x80000000) &&
           (result.data[lastPos] & 0x80000000) != (bi1.data[lastPos] & 0x80000000))
        {
            throw (new ArithmeticException());
        }

        return result;
    }

    ///                     Overloading of the unary -- operator, decrements BigInteger by 1
    // <param name="bi1">   A BigInteger</param>
    /// <returns>           Decremented BigInteger</returns>
    public static BigInteger operator --(BigInteger bi1)
    {
        BigInteger result = new BigInteger(bi1);

        long val;
        bool carryIn = true;
        int index = 0;

        while (carryIn && index < maxLength)
        {
            val = (long)(result.data[index]);
            val--;

            result.data[index] = (uint)(val & 0xFFFFFFFF);

            if (val >= 0)
                carryIn = false;

            index++;
        }

        if (index > result.dataLength)
            result.dataLength = index;

        while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
            result.dataLength--;

        // overflow check
        int lastPos = maxLength - 1;

        // overflow if initial value was -ve but -- caused a sign
        // change to positive.

        if ((bi1.data[lastPos] & 0x80000000) != 0 &&
           (result.data[lastPos] & 0x80000000) != (bi1.data[lastPos] & 0x80000000))
        {
            throw (new ArithmeticException("Underflow in --."));
        }

        return result;
    }

    ///                     Overloading of multiplication operator
    // <param name="bi1">   First BigInteger</param> <param name="bi2">Second BigInteger</param>
    /// <returns>           Result of the multiplication of 2 BigIntegers</returns>
    public static BigInteger operator *(BigInteger bi1, BigInteger bi2)
    {
        int lastPos = maxLength - 1;
        bool bi1Neg = false, bi2Neg = false;

        // take the absolute value of the inputs
        try
        {
            if ((bi1.data[lastPos] & 0x80000000) != 0)     // bi1 negative
            {
                bi1Neg = true; bi1 = -bi1;
            }
            if ((bi2.data[lastPos] & 0x80000000) != 0)     // bi2 negative
            {
                bi2Neg = true; bi2 = -bi2;
            }
        }
        catch (Exception) { }

        BigInteger result = new BigInteger();

        // multiply the absolute values
        try
        {
            for (int i = 0; i < bi1.dataLength; i++)
            {
                if (bi1.data[i] == 0) continue;

                ulong mcarry = 0;
                for (int j = 0, k = i; j < bi2.dataLength; j++, k++)
                {
                    // k = i + j
                    ulong val = ((ulong)bi1.data[i] * (ulong)bi2.data[j]) +
                                 (ulong)result.data[k] + mcarry;

                    result.data[k] = (uint)(val & 0xFFFFFFFF);
                    mcarry = (val >> 32);
                }

                if (mcarry != 0)
                    result.data[i + bi2.dataLength] = (uint)mcarry;
            }
        }
        catch (Exception)
        {
            throw (new ArithmeticException("Multiplication overflow."));
        }

        result.dataLength = bi1.dataLength + bi2.dataLength;
        if (result.dataLength > maxLength)
            result.dataLength = maxLength;

        while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
            result.dataLength--;

        // overflow check (result is -ve)
        if ((result.data[lastPos] & 0x80000000) != 0)
        {
            if (bi1Neg != bi2Neg && result.data[lastPos] == 0x80000000)    // different sign
            {
                // handle the special case where multiplication produces
                // a max negative number in 2's complement.
                if (result.dataLength == 1)
                    return result;
                else
                {
                    bool isMaxNeg = true;
                    for (int i = 0; i < result.dataLength - 1 && isMaxNeg; i++)
                    {
                        if (result.data[i] != 0)
                            isMaxNeg = false;
                    }

                    if (isMaxNeg)
                        return result;
                }
            }

            throw (new ArithmeticException("Multiplication overflow."));
        }

        // if input has different signs, then result is -ve
        if (bi1Neg != bi2Neg)
            return -result;

        return result;
    }

    ///                     Overloading of the unary &lt;&lt; operator (left shift)
    ///                     Shifting by a negative number is an undefined behaviour (UB).
    //<param name="bi1">    A BigInteger</param><param name="shiftVal">Left shift by shiftVal bit</param>
    /// <returns>           Left-shifted BigInteger</returns>
    public static BigInteger operator <<(BigInteger bi1, int shiftVal)
    {
        BigInteger result = new BigInteger(bi1);
        result.dataLength = shiftLeft(result.data, shiftVal);

        return result;
    }

    //least significant bits at lower part of buffer
    private static int shiftLeft(uint[] buffer, int shiftVal)
    {
        int shiftAmount = 32;
        int bufLen = buffer.Length;

        while (bufLen > 1 && buffer[bufLen - 1] == 0)
            bufLen--;

        for (int count = shiftVal; count > 0;)
        {
            if (count < shiftAmount)
                shiftAmount = count;

            ulong carry = 0;
            for (int i = 0; i < bufLen; i++)
            {
                ulong val = ((ulong)buffer[i]) << shiftAmount;
                val |= carry;

                buffer[i] = (uint)(val & 0xFFFFFFFF);
                carry = val >> 32;
            }

            if (carry != 0)
            {
                if (bufLen + 1 <= buffer.Length)
                {
                    buffer[bufLen] = (uint)carry;
                    bufLen++;
                }
            }
            count -= shiftAmount;
        }
        return bufLen;
    }

    ///                     Overloading of the unary &gt;&gt; operator (right shift)
    ///                     Shifting by a negative number is an undefined behaviour (UB).
    // <param name="bi1">   A BigInteger</param> <param name="shiftVal">Right shift by shiftVal bit</param>
    /// <returns>           Right-shifted BigInteger</returns>
    public static BigInteger operator >>(BigInteger bi1, int shiftVal)
    {
        BigInteger result = new BigInteger(bi1);
        result.dataLength = shiftRight(result.data, shiftVal);


        if ((bi1.data[maxLength - 1] & 0x80000000) != 0) // negative
        {
            for (int i = maxLength - 1; i >= result.dataLength; i--)
                result.data[i] = 0xFFFFFFFF;

            uint mask = 0x80000000;
            for (int i = 0; i < 32; i++)
            {
                if ((result.data[result.dataLength - 1] & mask) != 0)
                    break;

                result.data[result.dataLength - 1] |= mask;
                mask >>= 1;
            }
            result.dataLength = maxLength;
        }

        return result;
    }

    private static int shiftRight(uint[] buffer, int shiftVal)
    {
        int shiftAmount = 32;
        int invShift = 0;
        int bufLen = buffer.Length;

        while (bufLen > 1 && buffer[bufLen - 1] == 0)
            bufLen--;

        for (int count = shiftVal; count > 0;)
        {
            if (count < shiftAmount)
            {
                shiftAmount = count;
                invShift = 32 - shiftAmount;
            }

            ulong carry = 0;
            for (int i = bufLen - 1; i >= 0; i--)
            {
                ulong val = ((ulong)buffer[i]) >> shiftAmount;
                val |= carry;

                carry = (((ulong)buffer[i]) << invShift) & 0xFFFFFFFF;
                buffer[i] = (uint)(val);
            }

            count -= shiftAmount;
        }

        while (bufLen > 1 && buffer[bufLen - 1] == 0)
            bufLen--;

        return bufLen;
    }
 
    ///                     Overloading of the NEGATE operator (2's complement)
    // <param name="bi1">   A BigInteger</param>
    /// <returns>           Negated BigInteger or default BigInteger value if bi1 is 0</returns>
    public static BigInteger operator -(BigInteger bi1)
    {
        // handle neg of zero separately since it'll cause an overflow
        // if we proceed.

        if (bi1.dataLength == 1 && bi1.data[0] == 0)
            return (new BigInteger());

        BigInteger result = new BigInteger(bi1);

        // 1's complement
        for (int i = 0; i < maxLength; i++)
            result.data[i] = (uint)(~(bi1.data[i]));

        // add one to result of 1's complement
        long val, carry = 1;
        int index = 0;

        while (carry != 0 && index < maxLength)
        {
            val = (long)(result.data[index]);
            val++;

            result.data[index] = (uint)(val & 0xFFFFFFFF);
            carry = val >> 32;

            index++;
        }

        if ((bi1.data[maxLength - 1] & 0x80000000) == (result.data[maxLength - 1] & 0x80000000))
            throw (new ArithmeticException("Overflow in negation.\n"));

        result.dataLength = maxLength;

        while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
            result.dataLength--;
        return result;
    }

    ///                     Overloading of equality operator, allows comparing 2 BigIntegers with == operator
    // <param name="bi1">   First BigInteger</param> <param name="bi2">Second BigInteger</param>
    /// <returns>           Boolean result of the comparison</returns>
    public static bool operator ==(BigInteger bi1, BigInteger bi2)
    {
        return bi1.Equals(bi2);
    }

    ///                     Overloading of not equal operator, allows comparing 2 BigIntegers with != operator
    // <param name="bi1">   First BigInteger</param> <param name="bi2">Second BigInteger</param>
    /// <returns>           Boolean result of the comparison</returns>
    public static bool operator !=(BigInteger bi1, BigInteger bi2)
    {
        return !(bi1.Equals(bi2));
    }

    ///                     Overriding of Equals method, allows comparing BigInteger with an arbitary object
    // <param name="o">     Input object, to be casted into BigInteger type for comparison</param>
    /// <returns>           Boolean result of the comparison</returns>
    public override bool Equals(object o)
    {
        BigInteger bi = (BigInteger)o;

        if (this.dataLength != bi.dataLength)
            return false;

        for (int i = 0; i < this.dataLength; i++)
        {
            if (this.data[i] != bi.data[i])
                return false;
        }
        return true;
    }

    ///                     Overloading of greater than operator, allows comparing 2 BigIntegers with &gt; operator
    // <param name="bi1">   First BigInteger</param> <param name="bi2">Second BigInteger</param>
    /// <returns>           Boolean result of the comparison</returns>
    public static bool operator >(BigInteger bi1, BigInteger bi2)
    {
        int pos = maxLength - 1;

        // bi1 is negative, bi2 is positive
        if ((bi1.data[pos] & 0x80000000) != 0 && (bi2.data[pos] & 0x80000000) == 0)
            return false;

        // bi1 is positive, bi2 is negative
        else if ((bi1.data[pos] & 0x80000000) == 0 && (bi2.data[pos] & 0x80000000) != 0)
            return true;

        // same sign
        int len = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength;
        for (pos = len - 1; pos >= 0 && bi1.data[pos] == bi2.data[pos]; pos--) ;

        if (pos >= 0)
        {
            if (bi1.data[pos] > bi2.data[pos])
                return true;
            return false;
        }
        return false;
    }

    ///                     Overloading of greater than operator, allows comparing 2 BigIntegers with &lt; operator
    ///<param name="bi1">   First BigInteger</param> <param name="bi2">Second BigInteger</param>
    /// <returns>           Boolean result of the comparison</returns>
    public static bool operator <(BigInteger bi1, BigInteger bi2)
    {
        int pos = maxLength - 1;

        // bi1 is negative, bi2 is positive
        if ((bi1.data[pos] & 0x80000000) != 0 && (bi2.data[pos] & 0x80000000) == 0)
            return true;

        // bi1 is positive, bi2 is negative
        else if ((bi1.data[pos] & 0x80000000) == 0 && (bi2.data[pos] & 0x80000000) != 0)
            return false;

        // same sign
        int len = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength;
        for (pos = len - 1; pos >= 0 && bi1.data[pos] == bi2.data[pos]; pos--) ;

        if (pos >= 0)
        {
            if (bi1.data[pos] < bi2.data[pos])
                return true;
            return false;
        }
        return false;
    }

    ///                     Overloading of greater than or equal to operator, allows comparing 2 BigIntegers with &gt;= operator
    // <param name="bi1">   First BigInteger</param> <param name="bi2">Second BigInteger</param>
    /// <returns>           Boolean result of the comparison</returns>
    public static bool operator >=(BigInteger bi1, BigInteger bi2)
    {
        return (bi1 == bi2 || bi1 > bi2);
    }

    ///                     Overloading of less than or equal to operator, allows comparing 2 BigIntegers with &lt;= operator
    // <param name="bi1">   First BigInteger</param> <param name="bi2">Second BigInteger</param>
    /// <returns>           Boolean result of the comparison</returns>
    public static bool operator <=(BigInteger bi1, BigInteger bi2)
    {
        return (bi1 == bi2 || bi1 < bi2);
    }


    //******************************************************************************************************
    // Private function that supports the division of two numbers with a divisor that has more than 1 digit.
    //******************************************************************************************************
    private static void multiByteDivide(BigInteger bi1, BigInteger bi2,
                                        BigInteger outQuotient, BigInteger outRemainder)
    {
        uint[] result = new uint[maxLength];

        int remainderLen = bi1.dataLength + 1;
        uint[] remainder = new uint[remainderLen];

        uint mask = 0x80000000;
        uint val = bi2.data[bi2.dataLength - 1];
        int shift = 0, resultPos = 0;

        while (mask != 0 && (val & mask) == 0)
        {
            shift++; mask >>= 1;
        }

        for (int i = 0; i < bi1.dataLength; i++)
            remainder[i] = bi1.data[i];
        shiftLeft(remainder, shift);
        bi2 = bi2 << shift;

        int j = remainderLen - bi2.dataLength;
        int pos = remainderLen - 1;

        ulong firstDivisorByte = bi2.data[bi2.dataLength - 1];
        ulong secondDivisorByte = bi2.data[bi2.dataLength - 2];

        int divisorLen = bi2.dataLength + 1;
        uint[] dividendPart = new uint[divisorLen];

        while (j > 0)
        {
            ulong dividend = ((ulong)remainder[pos] << 32) + (ulong)remainder[pos - 1];

            ulong q_hat = dividend / firstDivisorByte;
            ulong r_hat = dividend % firstDivisorByte;

            bool done = false;
            while (!done)
            {
                done = true;

                if (q_hat == 0x100000000 ||
                   (q_hat * secondDivisorByte) > ((r_hat << 32) + remainder[pos - 2]))
                {
                    q_hat--;
                    r_hat += firstDivisorByte;

                    if (r_hat < 0x100000000)
                        done = false;
                }
            }

            for (int h = 0; h < divisorLen; h++)
                dividendPart[h] = remainder[pos - h];

            BigInteger kk = new BigInteger(dividendPart);
            BigInteger ss = bi2 * (long)q_hat;

            while (ss > kk)
            {
                q_hat--;
                ss -= bi2;
            }
            BigInteger yy = kk - ss;

            for (int h = 0; h < divisorLen; h++)
                remainder[pos - h] = yy.data[bi2.dataLength - h];

            result[resultPos++] = (uint)q_hat;

            pos--;
            j--;
        }

        outQuotient.dataLength = resultPos;
        int y = 0;
        for (int x = outQuotient.dataLength - 1; x >= 0; x--, y++)
            outQuotient.data[y] = result[x];
        for (; y < maxLength; y++)
            outQuotient.data[y] = 0;

        while (outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0)
            outQuotient.dataLength--;

        if (outQuotient.dataLength == 0)
            outQuotient.dataLength = 1;

        outRemainder.dataLength = shiftRight(remainder, shift);

        for (y = 0; y < outRemainder.dataLength; y++)
            outRemainder.data[y] = remainder[y];
        for (; y < maxLength; y++)
            outRemainder.data[y] = 0;
    }


    //****************************************************************************************************
    // Private function that supports the division of two numbers with a divisor that has only 1 digit.
    //****************************************************************************************************
    private static void singleByteDivide(BigInteger bi1, BigInteger bi2,
                                         BigInteger outQuotient, BigInteger outRemainder)
    {
        uint[] result = new uint[maxLength];
        int resultPos = 0;

        // copy dividend to reminder
        for (int i = 0; i < maxLength; i++)
            outRemainder.data[i] = bi1.data[i];
        outRemainder.dataLength = bi1.dataLength;

        while (outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0)
            outRemainder.dataLength--;

        ulong divisor = (ulong)bi2.data[0];
        int pos = outRemainder.dataLength - 1;
        ulong dividend = (ulong)outRemainder.data[pos];

        if (dividend >= divisor)
        {
            ulong quotient = dividend / divisor;
            result[resultPos++] = (uint)quotient;

            outRemainder.data[pos] = (uint)(dividend % divisor);
        }
        pos--;

        while (pos >= 0)
        {
            dividend = ((ulong)outRemainder.data[pos + 1] << 32) + (ulong)outRemainder.data[pos];
            ulong quotient = dividend / divisor;
            result[resultPos++] = (uint)quotient;

            outRemainder.data[pos + 1] = 0;
            outRemainder.data[pos--] = (uint)(dividend % divisor);
        }

        outQuotient.dataLength = resultPos;
        int j = 0;
        for (int i = outQuotient.dataLength - 1; i >= 0; i--, j++)
            outQuotient.data[j] = result[i];
        for (; j < maxLength; j++)
            outQuotient.data[j] = 0;

        while (outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0)
            outQuotient.dataLength--;

        if (outQuotient.dataLength == 0)
            outQuotient.dataLength = 1;

        while (outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0)
            outRemainder.dataLength--;
    }


    /// <summary>           Overloading of division operator </summary>
    /// <remarks>           The dataLength of the divisor's absolute value must be less than maxLength    </remarks>
    // <param name="bi1">   Dividend</param> <param name="bi2">Divisor        </param>
    /// <returns>           Quotient of the division</returns>
    public static BigInteger operator /(BigInteger bi1, BigInteger bi2)
    {
        BigInteger quotient = new BigInteger();
        BigInteger remainder = new BigInteger();

        int lastPos = maxLength - 1;
        bool divisorNeg = false, dividendNeg = false;

        if ((bi1.data[lastPos] & 0x80000000) != 0)     // bi1 negative
        {
            bi1 = -bi1;
            dividendNeg = true;
        }
        if ((bi2.data[lastPos] & 0x80000000) != 0)     // bi2 negative
        {
            bi2 = -bi2;
            divisorNeg = true;
        }

        if (bi1 < bi2)
        {
            return quotient;
        }

        else
        {
            if (bi2.dataLength == 1)
                singleByteDivide(bi1, bi2, quotient, remainder);
            else
                multiByteDivide(bi1, bi2, quotient, remainder);

            if (dividendNeg != divisorNeg)
                return -quotient;

            return quotient;
        }
    }


    /// <summary> Overloading of modulus operator </summary>
    /// <remarks>The dataLength of the divisor's absolute value must be less than maxLength</remarks>
    // <param name="bi1">Dividend</param> <param name="bi2">Divisor</param>
    /// <returns>Remainder of the division</returns>
    public static BigInteger operator %(BigInteger bi1, BigInteger bi2)
    {
        BigInteger quotient = new BigInteger();
        BigInteger remainder = new BigInteger(bi1);

        int lastPos = maxLength - 1;
        bool dividendNeg = false;

        if ((bi1.data[lastPos] & 0x80000000) != 0)     // bi1 negative
        {
            bi1 = -bi1;
            dividendNeg = true;
        }
        if ((bi2.data[lastPos] & 0x80000000) != 0)     // bi2 negative
            bi2 = -bi2;

        if (bi1 < bi2)
        {
            return remainder;
        }

        else
        {
            if (bi2.dataLength == 1)
                singleByteDivide(bi1, bi2, quotient, remainder);
            else
                multiByteDivide(bi1, bi2, quotient, remainder);

            if (dividendNeg)
                return -remainder;

            return remainder;
        }
    }

    /// <summary> Overloading of bitwise AND operator</summary>
    // <param name="bi1">First BigInteger</param>
    // <param name="bi2">Second BigInteger</param>
    /// <returns>BigInteger result after performing &amp; operation</returns>
    public static BigInteger operator &(BigInteger bi1, BigInteger bi2)
    {
        BigInteger result = new BigInteger();

        int len = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength;

        for (int i = 0; i < len; i++)
        {
            uint sum = (uint)(bi1.data[i] & bi2.data[i]);
            result.data[i] = sum;
        }

        result.dataLength = maxLength;

        while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
            result.dataLength--;

        return result;
    }

    /// Overloading of bitwise OR operator
    // <param name="bi1">First BigInteger</param>
    // <param name="bi2">Second BigInteger</param>
    /// <returns>BigInteger result after performing | operation</returns>
    public static BigInteger operator |(BigInteger bi1, BigInteger bi2)
    {
        BigInteger result = new BigInteger();

        int len = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength;

        for (int i = 0; i < len; i++)
        {
            uint sum = (uint)(bi1.data[i] | bi2.data[i]);
            result.data[i] = sum;
        }

        result.dataLength = maxLength;

        while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
            result.dataLength--;

        return result;
    }

    /// Overloading of bitwise XOR operator
    // <param name="bi1">First BigInteger</param>
    // <param name="bi2">Second BigInteger</param>
    /// <returns>BigInteger result after performing ^ operation</returns>
    public static BigInteger operator ^(BigInteger bi1, BigInteger bi2)
    {
        BigInteger result = new BigInteger();

        int len = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength;

        for (int i = 0; i < len; i++)
        {
            uint sum = (uint)(bi1.data[i] ^ bi2.data[i]);
            result.data[i] = sum;
        }

        result.dataLength = maxLength;

        while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
            result.dataLength--;

        return result;
    }

    /// Returns a string representing the BigInteger in base 10
    /// <returns>string representation of the BigInteger</returns>
    public override string ToString()
    {
        return ToString(10);
    }

    /// Returns a string representing the BigInteger in [sign][magnitude] format in the specified radix
    /// <example>
    ///If the value of BigInteger is -255 in base 10, then ToString(16) returns "-FF"</example>
    // <param name="radix">Base</param>
    /// <returns>string representation of the BigInteger in [sign][magnitude] format</returns>
    public string ToString(int radix)
    {
        if (radix < 2 || radix > 36)
            throw (new ArgumentException("Radix must be >= 2 and <= 36"));

        string charSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string result = "";

        BigInteger a = this;

        bool negative = false;
        if ((a.data[maxLength - 1] & 0x80000000) != 0)
        {
            negative = true;
            try
            {
                a = -a;
            }
            catch (Exception) { }
        }

        BigInteger quotient = new BigInteger();
        BigInteger remainder = new BigInteger();
        BigInteger biRadix = new BigInteger(radix);

        if (a.dataLength == 1 && a.data[0] == 0)
            result = "0";
        else
        {
            while (a.dataLength > 1 || (a.dataLength == 1 && a.data[0] != 0))
            {
                singleByteDivide(a, biRadix, quotient, remainder);

                if (remainder.data[0] < 10)
                    result = remainder.data[0] + result;
                else
                    result = charSet[(int)remainder.data[0] - 10] + result;

                a = quotient;
            }
            if (negative)
                result = "-" + result;
        }

        return result;
    }

    /// Modulo Exponentiation
    //<param name="exp">Exponential</param>
    //<param name="n">Modulo</param>
    /// <returns>BigInteger result of raising this to the power of exp and then modulo n </returns>
    public BigInteger modPow(BigInteger exp, BigInteger n)
    {
        if ((exp.data[maxLength - 1] & 0x80000000) != 0)
            throw (new ArithmeticException("Positive exponents only."));

        BigInteger resultNum = 1;
        BigInteger tempNum;
        bool thisNegative = false;

        if ((this.data[maxLength - 1] & 0x80000000) != 0)   // negative this
        {
            tempNum = -this % n;
            thisNegative = true;
        }
        else
            tempNum = this % n;  // ensures (tempNum * tempNum) < b^(2k)

        if ((n.data[maxLength - 1] & 0x80000000) != 0)   // negative n
            n = -n;

        // calculate constant = b^(2k) / m
        BigInteger constant = new BigInteger();

        int i = n.dataLength << 1;
        constant.data[i] = 0x00000001;
        constant.dataLength = i + 1;

        constant = constant / n;
        int totalBits = exp.bitCount();
        int count = 0;

        // perform squaring and multiply exponentiation
        for (int pos = 0; pos < exp.dataLength; pos++)
        {
            uint mask = 0x01;

            for (int index = 0; index < 32; index++)
            {
                if ((exp.data[pos] & mask) != 0)
                    resultNum = BarrettReduction(resultNum * tempNum, n, constant);

                mask <<= 1;

                tempNum = BarrettReduction(tempNum * tempNum, n, constant);


                if (tempNum.dataLength == 1 && tempNum.data[0] == 1)
                {
                    if (thisNegative && (exp.data[0] & 0x1) != 0)    //odd exp
                        return -resultNum;
                    return resultNum;
                }
                count++;
                if (count == totalBits)
                    break;
            }
        }

        if (thisNegative && (exp.data[0] & 0x1) != 0)    //odd exp
            return -resultNum;

        return resultNum;
    }

    /// Fast calculation of modular reduction using Barrett's reduction
    /// Requires x &lt; b^(2k), where b is the base.  In this case, base is 2^32 (uint).
    /// </remarks>
    // <param name="x"></param>
    // <param name="n"></param>
    // <param name="constant"></param>
    /// <returns></returns>
    private BigInteger BarrettReduction(BigInteger x, BigInteger n, BigInteger constant)
    {
        int k = n.dataLength,
            kPlusOne = k + 1,
            kMinusOne = k - 1;

        BigInteger q1 = new BigInteger();

        // q1 = x / b^(k-1)
        for (int i = kMinusOne, j = 0; i < x.dataLength; i++, j++)
            q1.data[j] = x.data[i];
        q1.dataLength = x.dataLength - kMinusOne;
        if (q1.dataLength <= 0)
            q1.dataLength = 1;


        BigInteger q2 = q1 * constant;
        BigInteger q3 = new BigInteger();

        // q3 = q2 / b^(k+1)
        for (int i = kPlusOne, j = 0; i < q2.dataLength; i++, j++)
            q3.data[j] = q2.data[i];
        q3.dataLength = q2.dataLength - kPlusOne;
        if (q3.dataLength <= 0)
            q3.dataLength = 1;


        // r1 = x mod b^(k+1)
        // i.e. keep the lowest (k+1) words
        BigInteger r1 = new BigInteger();
        int lengthToCopy = (x.dataLength > kPlusOne) ? kPlusOne : x.dataLength;
        for (int i = 0; i < lengthToCopy; i++)
            r1.data[i] = x.data[i];
        r1.dataLength = lengthToCopy;


        // r2 = (q3 * n) mod b^(k+1)
        // partial multiplication of q3 and n

        BigInteger r2 = new BigInteger();
        for (int i = 0; i < q3.dataLength; i++)
        {
            if (q3.data[i] == 0) continue;

            ulong mcarry = 0;
            int t = i;
            for (int j = 0; j < n.dataLength && t < kPlusOne; j++, t++)
            {
                // t = i + j
                ulong val = ((ulong)q3.data[i] * (ulong)n.data[j]) +
                             (ulong)r2.data[t] + mcarry;

                r2.data[t] = (uint)(val & 0xFFFFFFFF);
                mcarry = (val >> 32);
            }

            if (t < kPlusOne)
                r2.data[t] = (uint)mcarry;
        }
        r2.dataLength = kPlusOne;
        while (r2.dataLength > 1 && r2.data[r2.dataLength - 1] == 0)
            r2.dataLength--;

        r1 -= r2;
        if ((r1.data[maxLength - 1] & 0x80000000) != 0)        // negative
        {
            BigInteger val = new BigInteger();
            val.data[kPlusOne] = 0x00000001;
            val.dataLength = kPlusOne + 1;
            r1 += val;
        }

        while (r1 >= n)
            r1 -= n;

        return r1;
    }

    /// Returns gcd(this, bi)
    // <param name="bi"></param>
    /// <returns>Greatest Common Divisor of this and bi</returns>
    public BigInteger gcd(BigInteger bi)
    {
        BigInteger x;
        BigInteger y;

        if ((data[maxLength - 1] & 0x80000000) != 0)     // negative
            x = -this;
        else
            x = this;

        if ((bi.data[maxLength - 1] & 0x80000000) != 0)     // negative
            y = -bi;
        else
            y = bi;

        BigInteger g = y;

        while (x.dataLength > 1 || (x.dataLength == 1 && x.data[0] != 0))
        {
            g = x;
            x = y % x;
            y = g;
        }

        return g;
    }

    /// Populates "this" with the specified amount of random bits
    // <param name="bits"></param>
    // <param name="rand"></param>
    public void genRandomBits(int bits, Random rand)
    {
        int dwords = bits >> 5;
        int remBits = bits & 0x1F;

        if (remBits != 0)
            dwords++;

        if (dwords > maxLength || bits <= 0)
            throw (new ArithmeticException("Number of required bits is not valid."));

        byte[] randBytes = new byte[dwords * 4];
        rand.NextBytes(randBytes);

        for (int i = 0; i < dwords; i++)
            data[i] = BitConverter.ToUInt32(randBytes, i * 4);

        for (int i = dwords; i < maxLength; i++)
            data[i] = 0;

        if (remBits != 0)
        {
            uint mask;

            if (bits != 1)
            {
                mask = (uint)(0x01 << (remBits - 1));
                data[dwords - 1] |= mask;
            }

            mask = (uint)(0xFFFFFFFF >> (32 - remBits));
            data[dwords - 1] &= mask;
        }
        else
            data[dwords - 1] |= 0x80000000;

        dataLength = dwords;

        if (dataLength == 0)
            dataLength = 1;
    }

    /// Populates "this" with the specified amount of random bits (secured version)
    // <param name="bits"></param>
    // <param name="rng"></param>
    public void genRandomBits(int bits, RNGCryptoServiceProvider rng)
    {
        int dwords = bits >> 5;
        int remBits = bits & 0x1F;

        if (remBits != 0)
            dwords++;

        if (dwords > maxLength || bits <= 0)
            throw (new ArithmeticException("Number of required bits is not valid."));

        byte[] randomBytes = new byte[dwords * 4];
        rng.GetBytes(randomBytes);

        for (int i = 0; i < dwords; i++)
            data[i] = BitConverter.ToUInt32(randomBytes, i * 4);

        for (int i = dwords; i < maxLength; i++)
            data[i] = 0;

        if (remBits != 0)
        {
            uint mask;

            if (bits != 1)
            {
                mask = (uint)(0x01 << (remBits - 1));
                data[dwords - 1] |= mask;
            }

            mask = (uint)(0xFFFFFFFF >> (32 - remBits));
            data[dwords - 1] &= mask;
        }
        else
            data[dwords - 1] |= 0x80000000;

        dataLength = dwords;

        if (dataLength == 0)
            dataLength = 1;
    }

    /// Returns the position of the most significant bit in the BigInteger
    /// <example>
    /// 1) The result is 1, if the value of BigInteger is 0...0000 0000
    /// 2) The result is 1, if the value of BigInteger is 0...0000 0001
    /// 3) The result is 2, if the value of BigInteger is 0...0000 0010
    /// 4) The result is 2, if the value of BigInteger is 0...0000 0011
    /// 5) The result is 5, if the value of BigInteger is 0...0001 0011
    /// </example>
    /// <returns></returns>
    public int bitCount()
    {
        while (dataLength > 1 && data[dataLength - 1] == 0)
            dataLength--;

        uint value = data[dataLength - 1];
        uint mask = 0x80000000;
        int bits = 32;

        while (bits > 0 && (value & mask) == 0)
        {
            bits--;
            mask >>= 1;
        }
        bits += ((dataLength - 1) << 5);

        return bits == 0 ? 1 : bits;
    }

    /// Probabilistic prime test based on Rabin-Miller's
    /// for any p &gt; 0 with p - 1 = 2^s * t
    /// p is probably prime (strong pseudoprime) if for any a &lt; p,
    /// 1) a^t mod p = 1 or
    /// 2) a^((2^j)*t) mod p = p-1 for some 0 &lt;= j &lt;= s-1
    /// Otherwise, p is composite.
    //<param name="confidence">Number of chosen bases</param>
    /// <returns>True if this is a strong pseudoprime to randomly chosen bases</returns>
    public bool RabinMillerTest(int confidence)
    {
        BigInteger thisVal;
        if ((this.data[maxLength - 1] & 0x80000000) != 0)        // negative
            thisVal = -this;
        else
            thisVal = this;

        if (thisVal.dataLength == 1)
        {
            // test small numbers
            if (thisVal.data[0] == 0 || thisVal.data[0] == 1)
                return false;
            else if (thisVal.data[0] == 2 || thisVal.data[0] == 3)
                return true;
        }

        if ((thisVal.data[0] & 0x1) == 0)     // even numbers
            return false;


        // calculate values of s and t
        BigInteger p_sub1 = thisVal - (new BigInteger(1));
        int s = 0;

        for (int index = 0; index < p_sub1.dataLength; index++)
        {
            uint mask = 0x01;

            for (int i = 0; i < 32; i++)
            {
                if ((p_sub1.data[index] & mask) != 0)
                {
                    index = p_sub1.dataLength;      // to break the outer loop
                    break;
                }
                mask <<= 1;
                s++;
            }
        }

        BigInteger t = p_sub1 >> s;

        int bits = thisVal.bitCount();
        BigInteger a = new BigInteger();
        Random rand = new Random();

        for (int round = 0; round < confidence; round++)
        {
            bool done = false;

            while (!done)		// generate a < n
            {
                int testBits = 0;

                // make sure "a" has at least 2 bits
                while (testBits < 2)
                    testBits = (int)(rand.NextDouble() * bits);

                a.genRandomBits(testBits, rand);

                int byteLen = a.dataLength;

                // make sure "a" is not 0
                if (byteLen > 1 || (byteLen == 1 && a.data[0] != 1))
                    done = true;
            }

            // check whether a factor exists (fix for version 1.03)
            BigInteger gcdTest = a.gcd(thisVal);
            if (gcdTest.dataLength == 1 && gcdTest.data[0] != 1)
                return false;

            BigInteger b = a.modPow(t, thisVal);

            bool result = false;

            if (b.dataLength == 1 && b.data[0] == 1)         // a^t mod p = 1
                result = true;

            for (int j = 0; result == false && j < s; j++)
            {
                if (b == p_sub1)         // a^((2^j)*t) mod p = p-1 for some 0 <= j <= s-1
                {
                    result = true;
                    break;
                }

                b = (b * b) % thisVal;
            }

            if (result == false)
                return false;
        }
        return true;
    }
    /// Determines whether a number is probably prime using the Rabin-Miller's test
    /// Before applying the test, the number is tested for divisibility by primes &lt; 2000
    // <param name="confidence">Number of chosen bases</param>
    /// <returns>True if this is probably prime</returns>
    public bool isProbablePrime(int confidence)
    {
        BigInteger thisVal;
        if ((this.data[maxLength - 1] & 0x80000000) != 0)        // negative
            thisVal = -this;
        else
            thisVal = this;

        // test for divisibility by primes < 2000
        for (int p = 0; p < primesBelow2000.Length; p++)
        {
            BigInteger divisor = primesBelow2000[p];

            if (divisor >= thisVal)
                break;

            BigInteger resultNum = thisVal % divisor;
            if (resultNum.IntValue() == 0)
                return false;
        }

        if (thisVal.RabinMillerTest(confidence))
            return true;
        else
            return false;
    }

    /// Determines whether this BigInteger is probably prime using a combination of base 2 strong pseudoprime test and Lucas strong pseudoprime test 
    /// The sequence of the primality test is as follows,
    /// 1) Trial divisions are carried out using prime numbers below 2000.
    ///    if any of the primes divides this BigInteger, then it is not prime.
    /// 2) Perform base 2 strong pseudoprime test.  If this BigInteger is a
    ///    base 2 strong pseudoprime, proceed on to the next step.
    /// 3) Perform strong Lucas pseudoprime test.
    /// For a detailed discussion of this primality test, see [6].
    /// <returns>True if this is probably prime</returns>
    public bool isProbablePrime()
    {
        BigInteger thisVal;
        if ((this.data[maxLength - 1] & 0x80000000) != 0)        // negative
            thisVal = -this;
        else
            thisVal = this;

        if (thisVal.dataLength == 1)
        {
            // test small numbers
            if (thisVal.data[0] == 0 || thisVal.data[0] == 1)
                return false;
            else if (thisVal.data[0] == 2 || thisVal.data[0] == 3)
                return true;
        }

        if ((thisVal.data[0] & 0x1) == 0)     // even numbers
            return false;


        // test for divisibility by primes < 2000
        for (int p = 0; p < primesBelow2000.Length; p++)
        {
            BigInteger divisor = primesBelow2000[p];

            if (divisor >= thisVal)
                break;

            BigInteger resultNum = thisVal % divisor;
            if (resultNum.IntValue() == 0)
                return false;
        }

        // Perform BASE 2 Rabin-Miller Test

        // calculate values of s and t
        BigInteger p_sub1 = thisVal - (new BigInteger(1));
        int s = 0;

        for (int index = 0; index < p_sub1.dataLength; index++)
        {
            uint mask = 0x01;

            for (int i = 0; i < 32; i++)
            {
                if ((p_sub1.data[index] & mask) != 0)
                {
                    index = p_sub1.dataLength;      // to break the outer loop
                    break;
                }
                mask <<= 1;
                s++;
            }
        }

        BigInteger t = p_sub1 >> s;

        int bits = thisVal.bitCount();
        BigInteger a = 2;

        // b = a^t mod p
        BigInteger b = a.modPow(t, thisVal);
        bool result = false;

        if (b.dataLength == 1 && b.data[0] == 1)         // a^t mod p = 1
            result = true;

        for (int j = 0; result == false && j < s; j++)
        {
            if (b == p_sub1)         // a^((2^j)*t) mod p = p-1 for some 0 <= j <= s-1
            {
                result = true;
                break;
            }

            b = (b * b) % thisVal;
        }
        return result;
    }

    /// Returns the lowest 4 bytes of the BigInteger as an int
    /// <returns>Lowest 4 bytes as integer</returns>
    public int IntValue()
    {
        return (int)data[0];
    }

    /// Returns the lowest 8 bytes of the BigInteger as a long
    /// <returns>Lowest 8 bytes as long</returns>
    public long LongValue()
    {
        long val = 0;

        val = (long)data[0];
        try
        {       // exception if maxLength = 1
            val |= (long)data[1] << 32;
        }
        catch (Exception)
        {
            if ((data[0] & 0x80000000) != 0) // negative
                val = (int)data[0];
        }

        return val;
    }

    /// Generates a positive BigInteger that is probably prime
    // <param name="bits">Number of bit</param>
    // <param name="confidence">Number of chosen bases</param>
    // <param name="rand">Random object</param>
    /// <returns>A probably prime number</returns>
    public static BigInteger genPseudoPrime(int bits, int confidence, Random rand)
    {
        BigInteger result = new BigInteger();
        bool done = false;

        while (!done)
        {
            result.genRandomBits(bits, rand);
            result.data[0] |= 0x01;		// make it odd

            // prime test
            done = result.isProbablePrime(confidence);
        }
        return result;
    }

    /// Generates a positive BigInteger that is probably prime (secured version)
    // <param name="bits">Number of bit</param>
    // <param name="confidence">Number of chosen bases</param>
    // <param name="rand">RNGCryptoServiceProvider object</param>
    /// <returns>A probably prime number</returns>
    public static BigInteger genPseudoPrime(int bits, int confidence, RNGCryptoServiceProvider rand)
    {
        BigInteger result = new BigInteger();
        bool done = false;

        while (!done)
        {
            result.genRandomBits(bits, rand);
            result.data[0] |= 0x01;		// make it odd

            // prime test
            done = result.isProbablePrime(confidence);
        }
        return result;
    }

    /// Returns the modulo inverse of this
    /// Throws ArithmeticException if the inverse does not exist.  (i.e. gcd(this, modulus) != 1)
    // <param name="modulus"></param>
    /// <returns>Modulo inverse of this</returns>
    public BigInteger modInverse(BigInteger modulus)
    {
        BigInteger[] p = { 0, 1 };
        BigInteger[] q = new BigInteger[2];    // quotients
        BigInteger[] r = { 0, 0 };             // remainders

        int step = 0;

        BigInteger a = modulus;
        BigInteger b = this;

        while (b.dataLength > 1 || (b.dataLength == 1 && b.data[0] != 0))
        {
            BigInteger quotient = new BigInteger();
            BigInteger remainder = new BigInteger();

            if (step > 1)
            {
                BigInteger pval = (p[0] - (p[1] * q[0])) % modulus;
                p[0] = p[1];
                p[1] = pval;
            }

            if (b.dataLength == 1)
                singleByteDivide(a, b, quotient, remainder);
            else
                multiByteDivide(a, b, quotient, remainder);

            q[0] = q[1];
            r[0] = r[1];
            q[1] = quotient; r[1] = remainder;

            a = b;
            b = remainder;

            step++;
        }
        if (r[0].dataLength > 1 || (r[0].dataLength == 1 && r[0].data[0] != 1))
            throw (new ArithmeticException("No inverse!"));

        BigInteger result = ((p[0] - (p[1] * q[0])) % modulus);

        if ((result.data[maxLength - 1] & 0x80000000) != 0)
            result += modulus;  // get the least positive modulus

        return result;
    }

    /// Returns the value of the BigInteger as a byte array
    /// The lowest index contains the MSB
    /// <returns>Byte array containing value of the BigInteger</returns>
    public byte[] getBytes()
    {
        int numBits = bitCount();

        int numBytes = numBits >> 3;
        if ((numBits & 0x7) != 0)
            numBytes++;

        byte[] result = new byte[numBytes];

        int pos = 0;
        uint tempVal, val = data[dataLength - 1];


        if ((tempVal = (val >> 24 & 0xFF)) != 0)
            result[pos++] = (byte)tempVal;

        if ((tempVal = (val >> 16 & 0xFF)) != 0)
            result[pos++] = (byte)tempVal;
        else if (pos > 0)
            pos++;

        if ((tempVal = (val >> 8 & 0xFF)) != 0)
            result[pos++] = (byte)tempVal;
        else if (pos > 0)
            pos++;

        if ((tempVal = (val & 0xFF)) != 0)
            result[pos++] = (byte)tempVal;
        else if (pos > 0)
            pos++;


        for (int i = dataLength - 2; i >= 0; i--, pos += 4)
        {
            val = data[i];
            result[pos + 3] = (byte)(val & 0xFF);
            val >>= 8;
            result[pos + 2] = (byte)(val & 0xFF);
            val >>= 8;
            result[pos + 1] = (byte)(val & 0xFF);
            val >>= 8;
            result[pos] = (byte)(val & 0xFF);
        }

        return result;
    }

    /// Returns a value that is equivalent to the integer square root of this
    /// The integer square root of "this" is defined as the largest integer n, such that (n * n) &lt;= this.
    /// Square root of negative integer is an undefined behaviour (UB).
    /// <returns>Integer square root of this</returns>
    public BigInteger sqrt()
    {
        uint numBits = (uint)this.bitCount();

        if ((numBits & 0x1) != 0)        // odd number of bits
            numBits = (numBits >> 1) + 1;
        else
            numBits = (numBits >> 1);

        uint bytePos = numBits >> 5;
        byte bitPos = (byte)(numBits & 0x1F);

        uint mask;

        BigInteger result = new BigInteger();
        if (bitPos == 0)
            mask = 0x80000000;
        else
        {
            mask = (uint)1 << bitPos;
            bytePos++;
        }
        result.dataLength = (int)bytePos;

        for (int i = (int)bytePos - 1; i >= 0; i--)
        {
            while (mask != 0)
            {
                // guess
                result.data[i] ^= mask;

                // undo the guess if its square is larger than this
                if ((result * result) > this)
                    result.data[i] ^= mask;

                mask >>= 1;
            }
            mask = 0x80000000;
        }
        return result;
    }

  
}

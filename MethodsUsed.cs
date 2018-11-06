/*
***********************************************************************************************************************
******************************************Constructors******************************************************************
************************************************************************************************************************
BigInteger(byte[])          Creates a new instance from an array of signed byte values

BigInteger(long)

BigInteger(int)             Creates a new instance representing the numeric value specified by the long or int argument

BigInteger(BigInteger)      Creates a new instance with the same value as the argument

***********************************************************************************************************************
******************************************* Methods********************************************************************
***********************************************************************************************************************

modPow(BigInteger exp, BigInteger m)                    Returns the value(thisexp mod m)

modInverse(BigInteger m)                                Returns the value(this-1mod m)

getBytes()                                              Converts the integer into a series of signed byte values

bitCount()                                              Calculates how many bits are required to express the integer value

genPseudoPrime(int bits, int confidence, Random rand)   Generates a random positive BigInteger with the specified number of bits and confidence level, using the provided random number generator

genRandomBits(int bits, Random rand)                    Creates a random number with the specified number of bits, using the provided random number generator */
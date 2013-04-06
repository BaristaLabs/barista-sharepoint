namespace Barista.UnitTests.Jurassic
{
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using Barista.Jurassic;

  /// <summary>
  /// Test the BigInteger struct.
  /// </summary>
  [TestClass]
  public class BigIntegerTests
  {
    readonly string[] m_significantNumbers =
        {
            "0",
            "1",
            "-1",
            "100",
            "-100",
            "429496729",
            "4294967295",
            "-4294967295",
            "4294967296",
            "-4294967296",
            "10000000000",
            "-10000000000",
            "123456789012345678901",
            "-123456789012345678901"
        };

    [TestMethod]
    public void Add()
    {
      for (int i = 0; i < m_significantNumbers.Length; i++)
        for (int j = 0; j < m_significantNumbers.Length; j++)
        {
          var expected = System.Numerics.BigInteger.Parse(m_significantNumbers[i]) + System.Numerics.BigInteger.Parse(m_significantNumbers[j]);
          var actual = BigInteger.Add(BigInteger.Parse(m_significantNumbers[i]), BigInteger.Parse(m_significantNumbers[j]));
          Assert.AreEqual(expected, Convert(actual), string.Format("Computing {0} + {1}", m_significantNumbers[i], m_significantNumbers[j]));
        }
    }

    [TestMethod]
    public void Subtract()
    {
      for (int i = 0; i < m_significantNumbers.Length; i++)
        for (int j = 0; j < m_significantNumbers.Length; j++)
        {
          var expected = System.Numerics.BigInteger.Parse(m_significantNumbers[i]) - System.Numerics.BigInteger.Parse(m_significantNumbers[j]);
          var actual = BigInteger.Subtract(BigInteger.Parse(m_significantNumbers[i]), BigInteger.Parse(m_significantNumbers[j]));
          Assert.AreEqual(expected, Convert(actual), string.Format("Computing {0} - {1}", m_significantNumbers[i], m_significantNumbers[j]));
        }
    }

    [TestMethod]
    public void Multiply()
    {
      for (int i = 0; i < m_significantNumbers.Length; i++)
        for (int j = 0; j < m_significantNumbers.Length; j++)
        {
          var expected = System.Numerics.BigInteger.Parse(m_significantNumbers[i]) * System.Numerics.BigInteger.Parse(m_significantNumbers[j]);
          var actual = BigInteger.Multiply(BigInteger.Parse(m_significantNumbers[i]), BigInteger.Parse(m_significantNumbers[j]));
          Assert.AreEqual(expected, Convert(actual), string.Format("Computing {0} * {1}", m_significantNumbers[i], m_significantNumbers[j]));
        }
    }

    [TestMethod]
    public void LessThan()
    {
      for (int i = 0; i < m_significantNumbers.Length; i++)
        for (int j = 0; j < m_significantNumbers.Length; j++)
        {
          var expected = System.Numerics.BigInteger.Parse(m_significantNumbers[i]) < System.Numerics.BigInteger.Parse(m_significantNumbers[j]);
          var actual = BigInteger.Compare(BigInteger.Parse(m_significantNumbers[i]), BigInteger.Parse(m_significantNumbers[j])) < 0;
          Assert.AreEqual(expected, actual, string.Format("Computing {0} < {1}", m_significantNumbers[i], m_significantNumbers[j]));
        }
    }

    [TestMethod]
    public void LessThanOrEqual()
    {
      for (int i = 0; i < m_significantNumbers.Length; i++)
        for (int j = 0; j < m_significantNumbers.Length; j++)
        {
          var expected = System.Numerics.BigInteger.Parse(m_significantNumbers[i]) <= System.Numerics.BigInteger.Parse(m_significantNumbers[j]);
          var actual = BigInteger.Compare(BigInteger.Parse(m_significantNumbers[i]), BigInteger.Parse(m_significantNumbers[j])) <= 0;
          Assert.AreEqual(expected, actual, string.Format("Computing {0} <= {1}", m_significantNumbers[i], m_significantNumbers[j]));
        }
    }

    [TestMethod]
    public void LeftShift()
    {
      for (int i = 0; i < m_significantNumbers.Length; i++)
        for (int j = 0; j < 100; j += 19)
        {
          var expected = System.Numerics.BigInteger.Parse(m_significantNumbers[i]) << j;
          var actual = BigInteger.LeftShift(Barista.Jurassic.BigInteger.Parse(m_significantNumbers[i]), j);
          Assert.AreEqual(expected, Convert(actual), string.Format("Computing {0} << {1}", m_significantNumbers[i], j));
        }
    }

    [TestMethod]
    public void RightShift()
    {
      for (int i = 0; i < m_significantNumbers.Length; i++)
        for (int j = 0; j < 100; j += 19)
        {
          var expected = System.Numerics.BigInteger.Parse(m_significantNumbers[i]);
          if (expected.Sign > 0)
            expected = expected >> j;
          else
            expected = System.Numerics.BigInteger.Negate(System.Numerics.BigInteger.Negate(expected) >> j);
          var actual = BigInteger.RightShift(BigInteger.Parse(m_significantNumbers[i]), j);
          Assert.AreEqual(expected, Convert(actual), string.Format("Computing {0} >> {1}", m_significantNumbers[i], j));
        }
    }

    [TestMethod]
    public void Pow()
    {
      int[] radixValues = new[] { 2, 3, 10 };
      for (int i = 0; i < radixValues.Length; i++)
        for (int j = 0; j < 50; j++)
        {
          var expected = System.Numerics.BigInteger.Pow(radixValues[i], j);
          var actual = Barista.Jurassic.BigInteger.Pow(radixValues[i], j);
          Assert.AreEqual(expected, Convert(actual), string.Format("Computing Pow({0}, {1})", radixValues[i], j));
        }
    }

    [TestMethod]
    public new void ToString()
    {
      //Barista.Jurassic.BigInteger.Parse("4294967295").ToString();
      for (int i = 0; i < m_significantNumbers.Length; i++)
      {
        var expected = System.Numerics.BigInteger.Parse(m_significantNumbers[i]).ToString();
        var actual = Barista.Jurassic.BigInteger.Parse(m_significantNumbers[i]).ToString();
        Assert.AreEqual(expected, actual, string.Format("Computing ToString({0})", m_significantNumbers[i]));
      }
    }

    [TestMethod]
    public void Abs()
    {
      for (int i = 0; i < m_significantNumbers.Length; i++)
      {
        var expected = System.Numerics.BigInteger.Abs(System.Numerics.BigInteger.Parse(m_significantNumbers[i]));
        var actual = Barista.Jurassic.BigInteger.Abs(Barista.Jurassic.BigInteger.Parse(m_significantNumbers[i]));
        Assert.AreEqual(expected, Convert(actual), string.Format("Computing Abs({0})", m_significantNumbers[i]));
      }
    }

    [TestMethod]
    public void Log()
    {
      int[] baseValues = new[] { 2, 3, 10 };
      for (int i = 0; i < m_significantNumbers.Length; i++)
      {
        for (int j = 0; j < baseValues.Length; j++)
        {
          var expected = System.Numerics.BigInteger.Log(System.Numerics.BigInteger.Parse(m_significantNumbers[i]), baseValues[j]);
          var actual = BigInteger.Log(Barista.Jurassic.BigInteger.Parse(m_significantNumbers[i]), baseValues[j]);
          Assert.AreEqual(expected, actual, string.Format("Computing Log({0}, {1})", m_significantNumbers[i], baseValues[j]));
        }
      }
    }

    [TestMethod]
    public void FromDouble()
    {
      for (int i = 0; i < m_significantNumbers.Length; i++)
      {
        var expected = new System.Numerics.BigInteger(double.Parse(m_significantNumbers[i]));
        var actual = BigInteger.FromDouble(double.Parse(m_significantNumbers[i]));
        Assert.AreEqual(expected, Convert(actual), string.Format("Computing FromDouble({0})", m_significantNumbers[i]));
      }
    }

    [TestMethod]
    public void ToDouble()
    {
      for (int i = 0; i < m_significantNumbers.Length; i++)
      {
        var expected = (double)System.Numerics.BigInteger.Parse(m_significantNumbers[i]);
        var actual = Barista.Jurassic.BigInteger.Parse(m_significantNumbers[i]).ToDouble();
        Assert.AreEqual(expected, actual, string.Format("Computing ToDouble({0})", m_significantNumbers[i]));
      }
    }

    // Helper method.
    private System.Numerics.BigInteger Convert(Barista.Jurassic.BigInteger value)
    {
      var result = System.Numerics.BigInteger.Zero;
      for (int i = value.WordCount - 1; i >= 0; i--)
      {
        result <<= 32;
        result += value.Words[i];
      }
      if (value.Sign == -1)
        result = result * -1;
      return result;
    }
  }
}

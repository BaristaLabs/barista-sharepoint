﻿namespace OFS.OrcaDB.Core
{
  using System.Text;
  using System.Security.Cryptography;

  public static class StringHelper
  {
    public static string CreateMD5Hash(string input)
    {
      // Use input string to calculate MD5 hash
      MD5 md5 = System.Security.Cryptography.MD5.Create();
      byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
      byte[] hashBytes = md5.ComputeHash(inputBytes);

      // Convert the byte array to hexadecimal string
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < hashBytes.Length; i++)
      {
        sb.Append(hashBytes[i].ToString("X2"));
        // To force the hex string to lower-case letters instead of
        // upper-case, use he following line instead:
        // sb.Append(hashBytes[i].ToString("x2")); 
      }
      return sb.ToString();
    }
  }
}

﻿namespace Barista.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;

  /// <summary>
  /// Defines extension methods for the <see cref="ArgumentHelper"/> class.
  /// </summary>
  /// <remarks>
  /// This class defines extensions methods for the <see cref="ArgumentHelper"/>. All extension methods simply delegate to the
  /// appropriate member of the <see cref="ArgumentHelper"/> class.
  /// </remarks>
  public static class ArgumentHelperExtensions
  {
    /// <include file='ArgumentHelper.doc.xml' path='doc/member[@name="AssertNotNull{T}(T,string)"]/*' />
    [DebuggerHidden]
    public static void AssertNotNull<T>(this T arg, string argName)
      where T : class
    {
      ArgumentHelper.AssertNotNull(arg, argName);
    }

    /// <include file='ArgumentHelper.doc.xml' path='doc/member[@name="AssertNotNull{T}(Nullable{T},string)"]/*' />
    [DebuggerHidden]
    public static void AssertNotNull<T>(this T? arg, string argName)
      where T : struct
    {
      ArgumentHelper.AssertNotNull(arg, argName);
    }

    /// <include file='ArgumentHelper.doc.xml' path='doc/member[@name="AssertGenericArgumentNotNull{T}(T,string)"]/*' />
    [DebuggerHidden]
    public static void AssertGenericArgumentNotNull<T>(this T arg, string argName)
    {
      ArgumentHelper.AssertGenericArgumentNotNull(arg, argName);
    }

    /// <include file='ArgumentHelper.doc.xml' path='doc/member[@name="AssertNotNull{T}(IEnumerable{T},string,bool)"]/*' />
    [DebuggerHidden]
    public static void AssertNotNull<T>(this IEnumerable<T> arg, string argName, bool assertContentsNotNull)
    {
      ArgumentHelper.AssertNotNull(arg, argName, assertContentsNotNull);
    }

    /// <include file='ArgumentHelper.doc.xml' path='doc/member[@name="AssertNotNullOrEmpty(string,string)"]/*' />
    [DebuggerHidden]
    public static void AssertNotNullOrEmpty(this string arg, string argName)
    {
      ArgumentHelper.AssertNotNullOrEmpty(arg, argName);
    }

    /// <include file='ArgumentHelper.doc.xml' path='doc/member[@name="AssertNotNullOrEmpty(string,string,bool)"]/*' />
    [DebuggerHidden]
    public static void AssertNotNullOrEmpty(this string arg, string argName, bool trim)
    {
      ArgumentHelper.AssertNotNullOrEmpty(arg, argName, trim);
    }

    /// <include file='ArgumentHelper.doc.xml' path='doc/member[@name="AssertEnumMember{TEnum}(TEnum,string)"]/*' />
    [DebuggerHidden]
    public static void AssertEnumMember<TEnum>(this TEnum enumValue, string argName)
      where TEnum : struct, IConvertible
    {
      ArgumentHelper.AssertEnumMember(enumValue, argName);
    }

    /// <include file='ArgumentHelper.doc.xml' path='doc/member[@name="AssertEnumMember{TEnum}(TEnum,string,TEnum[])"]/*' />
    [DebuggerHidden]
    public static void AssertEnumMember<TEnum>(this TEnum enumValue, string argName, params TEnum[] validValues)
      where TEnum : struct, IConvertible
    {
      ArgumentHelper.AssertEnumMember(enumValue, argName, validValues);
    }
  }
}
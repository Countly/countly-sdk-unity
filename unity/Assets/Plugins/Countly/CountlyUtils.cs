/*
 * Copyright (c) 2014 Mario Freitas (imkira@gmail.com)
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;
using System;
using System.Text;

namespace Countly
{
  public class Utils
  {
    protected static DateTime EPOCH_TIME =
      new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static double GetCurrentTime()
    {
      TimeSpan t = (DateTime.UtcNow - EPOCH_TIME);
      return t.TotalSeconds;
    }
	

    public static string EscapeURL(string str)
    {
      return WWW.EscapeURL(str);
    }

    public static void JSONSerializeDouble(StringBuilder builder, double val)
    {
      builder.Append(val.ToString("R"));
    }

    public static void JSONSerializeString(StringBuilder builder, string str)
    {
      if (str == null)
      {
        builder.Append("null");
        return;
      }

      char c;
      int length = str.Length;

      builder.Append('\"');

      for (int i = 0; i < length; i++)
      {
        c = str[i];

        switch (c)
        {
          case '\b':
            builder.Append("\\b");
            break;

          case '\t':
            builder.Append("\\t");
            break;

          case '\n':
            builder.Append("\\n");
            break;

          case '\f':
            builder.Append("\\f");
            break;

          case '\r':
            builder.Append("\\r");
            break;

          case '"':
            builder.Append("\\\"");
            break;

          case '\\':
            builder.Append("\\\\");
            break;

          default:
            int cp = System.Convert.ToInt32(c);

            if ((cp >= 32) && (cp <= 126))
            {
              builder.Append(c);
            }
            else
            {
              builder.Append("\\u");
              builder.Append(cp.ToString("x4"));
            }
            break;
        }
      }

      builder.Append('\"');
    }
  }
}

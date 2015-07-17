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
using System.Collections.Generic;
using System.Text;

namespace Countly
{
  public class Event
  {
    public string Key {get; set;}

    public long Count {get; set;}

    public long Timestamp {get; set;}

    protected bool _usesSum = false;
    public bool UsesSum
    {
      get
      {
        return _usesSum;
      }
    }

    protected double _sum;
    public double Sum
    {
      get
      {
        return _sum;
      }
      set
      {
        _usesSum = true;
        _sum = value;
      }
    }

    public Dictionary<string, string> Segmentation {get; set;}

    public Event()
    {
      Timestamp = (long)Utils.GetCurrentTime();
    }

    public void JSONSerialize(StringBuilder builder)
    {
      // open event object
      builder.Append("{");

      builder.Append("\"key\":");
      Utils.JSONSerializeString(builder, Key);

      builder.Append(",\"count\":");
      builder.Append(Count);

      builder.Append(",\"timestamp\":");
      builder.Append(Timestamp);

      if (UsesSum == true)
      {
        builder.Append(",\"sum\":");
        Utils.JSONSerializeDouble(builder, Sum);
      }

      if ((Segmentation != null) && (Segmentation.Count > 0))
      {
        builder.Append(",\"segmentation\":");
        JSONSerializeSegmentation(builder);
      }

      // close event object
      builder.Append("}");
    }

    protected void JSONSerializeSegmentation(StringBuilder builder)
    {
      bool first = true;

      // open segmentation object
      builder.Append("{");

      foreach (KeyValuePair<string, string> s in Segmentation)
      {
        if (first == true)
        {
          first = false;
        }
        else
        {
          builder.Append(",");
        }

        // serialize key
        Utils.JSONSerializeString(builder, s.Key);

        builder.Append(":");

        // serialize value
        Utils.JSONSerializeString(builder, s.Value);
      }

      // close segmentation object
      builder.Append("}");
    }
  }
}

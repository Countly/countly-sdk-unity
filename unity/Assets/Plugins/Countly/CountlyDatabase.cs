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
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Countly
{
  public class Queue
  {
    protected const string BASENAME = "Countly.dat";
    protected string _filename;
    protected Queue<string> _queue;
    protected int _maxCapacity;
    protected bool _useStorage;

    public int Count
    {
      get
      {
        return _queue.Count;
      }
    }

    public Queue(int capacity, int maxCapacity, bool useStorage)
    {
      _filename = Path.Combine(Application.persistentDataPath, BASENAME);
      _queue = new Queue<string>(capacity);
      _maxCapacity = maxCapacity;
      _useStorage = useStorage;

      if (_useStorage == true)
      {
        EnqueueFromFile();
      }
    }

    public string Peek()
    {
      return _queue.Peek();
    }

    public void Enqueue(string data)
    {
      if (_queue.Count < _maxCapacity)
      {
        _queue.Enqueue(data);
        if (_useStorage == true)
        {
          AppendToFile(data);
        }
      }
    }

    public void Dequeue()
    {
      if (_queue.Count > 0)
      {
        _queue.Dequeue();
        if (_useStorage == true)
        {
          SaveToFile();
        }
      }
    }

    protected void EnqueueFromFile()
    {
      string[] lines = null;

      try
      {
        // read all lines from file
        lines = File.ReadAllLines(_filename, Encoding.UTF8);
      }
      catch (System.Exception)
      {
        lines = null;
      }

      // couldn't read file?
      if (lines == null)
      {
        return;
      }

      int enqueueCount = lines.Length;
      int newCount = enqueueCount + _queue.Count;

      // check capacity, check overflow
      if ((_queue.Count >= _maxCapacity) ||
          (enqueueCount <= 0) ||
          (newCount < enqueueCount) ||
          (newCount < _queue.Count))
      {
        return;
      }

      if (newCount > _maxCapacity)
      {
        // read last lines
        enqueueCount = _maxCapacity - _queue.Count;
      }

      string data;

      for (int i = 1; i <= enqueueCount; ++i)
      {
        data = lines[lines.Length - i];
        _queue.Enqueue(data);
      }
    }

    protected void AppendToFile(string data)
    {
      try
      {
        using (StreamWriter file = new StreamWriter(_filename, true))
        {
          file.WriteLine(data);
        }
      }
      catch (System.Exception)
      {
      }
    }

    public void SaveToFile()
    {
      if (_queue.Count > 0)
      {
        try
        {
          using (StreamWriter file = new StreamWriter(_filename))
          {
            foreach (string data in _queue)
            {
              file.WriteLine(data);
            }
          }
        }
        catch (System.Exception)
        {
        }
      }
      else
      {
        DeleteFile();
      }
    }

    public void DeleteFile()
    {
      try
      {
        File.Delete(_filename);
      }
      catch (System.Exception)
      {
      }
    }
  }
}

/*
 * Copyright (c) 2013 Mario Freitas (imkira@gmail.com)
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

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class CountlyPostprocessor : Editor
{
  [PostProcessBuild(9999)]
  public static void OnPostprocessBuild(BuildTarget target,
      string pathToBuildProject)
  {
    if (target != BuildTarget.iOS)
    {
      return;
    }

    string script = "CountlyXCodePostprocessor.py";
    string pathToXCodeProject = GetXCodeProjectPath(pathToBuildProject);
    string args = String.Format("\"{0}\"", pathToXCodeProject);
    string result;
    int exitCode;

    try
    {
      exitCode = ExecuteScript(script, args, out result);
    }
    catch (Exception e)
    {
      Debug.LogError("CountlyPostprocessor: Could not execute " +
          script + "! Make sure the script has executable permissions!\n" +
          "Exception: (" + e + ")");
      return;
    }

    if ((exitCode != 0) || (string.IsNullOrEmpty(result)))
    {
      Debug.LogError("CountlyPostprocessor: Postprocess failed: " + result);
      return;
    }

    Debug.Log(result);
  }

  private static string GetXCodeProjectPath(string pathToBuildProject)
  {
    string pbxproj = pathToBuildProject;
    pbxproj = Path.Combine(pbxproj, "Unity-iPhone.xcodeproj");
    pbxproj = Path.Combine(pbxproj, "project.pbxproj");
    return Path.GetFullPath(pbxproj);
  }

  private static int ExecuteScript(string script, string arguments,
      out string output)
  {
    StackFrame sf = new StackFrame(true);
    StackTrace st = new StackTrace(sf);

    sf = st.GetFrame(0);
    string dir = Path.GetDirectoryName(sf.GetFileName());
    string fileName = Path.Combine(dir, script);

    AddExecutablePermissionToScript(fileName);
    return Execute(fileName, arguments, out output);
  }

  private static void AddExecutablePermissionToScript(string script)
  {
    string args = String.Format("u+x \"{0}\"", script);
    string result;

    try
    {
      Execute("chmod", args, out result);
    }
    catch (Exception)
    {
    }
  }

  public static int Execute(string fileName, string arguments,
      out string output)
  {
    ProcessStartInfo psi = new ProcessStartInfo();
    psi.UseShellExecute = false;
    psi.RedirectStandardError = true;
    psi.RedirectStandardOutput = true;
    psi.RedirectStandardInput = true;
    psi.WindowStyle = ProcessWindowStyle.Hidden;
    psi.CreateNoWindow = true;
    psi.ErrorDialog = false;
    psi.FileName = fileName;
    psi.Arguments = arguments;

    using (Process process = Process.Start(psi))
    {
      process.StandardInput.Close();
      StreamReader sOut = process.StandardOutput;
      StreamReader sErr = process.StandardError;
      output = sOut.ReadToEnd() + sErr.ReadToEnd();
      sOut.Close();
      sErr.Close();
      process.WaitForExit();
      return process.ExitCode;
    }
  }
}

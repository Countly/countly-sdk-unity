#!/usr/bin/env python

import sys

sys.dont_write_bytecode = True

import os
from mod_pbxproj import XcodeProject

if len(sys.argv) != 2:
  sys.exit('Syntax: <path to .pbxproj file>')

pbxproj_path = sys.argv[1]

if not os.path.exists(pbxproj_path):
  sys.exit('File not found: %s' % pbxproj_path)

# load XCode project
project = XcodeProject.Load(pbxproj_path)

# add Security framework
project.add_file('System/Library/Frameworks/CoreTelephony.framework', tree='SDKROOT')

# save
if project.modified:
  project.backup()
  project.saveFormat3_2()
  print('CountlyPostprocessor: Successfully updated %s' % pbxproj_path);

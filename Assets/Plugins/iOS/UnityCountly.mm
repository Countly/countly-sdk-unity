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

#import <Foundation/Foundation.h>
#import <CoreTelephony/CTTelephonyNetworkInfo.h>
#import <CoreTelephony/CTCarrier.h>

#define NSSTRING_TO_STR_OR_EMPTY(__t__) \
  (((__t__) == nil) ? NULL : strdup([(__t__) UTF8String]))

extern "C"
{
  char* _CountlyGetAppVersion();
  char* _CountlyGetLocaleDescription();
  char* _CountlyGetCarrierName();
}

char* _CountlyGetAppVersion()
{
  NSString *version = [[NSBundle mainBundle]
    objectForInfoDictionaryKey:(NSString*)kCFBundleVersionKey];
  return NSSTRING_TO_STR_OR_EMPTY(version);
}

char* _CountlyGetLocaleDescription()
{
  NSString* localeIdentifier = [[NSLocale currentLocale] localeIdentifier];
  return NSSTRING_TO_STR_OR_EMPTY(localeIdentifier);
}

char* _CountlyGetCarrierName()
{
  char* carrierName = NULL;

  CTTelephonyNetworkInfo* netinfo = [[CTTelephonyNetworkInfo alloc] init];

  if (netinfo != nil)
  {
    CTCarrier* carrier = [netinfo subscriberCellularProvider];

    if (carrier != nil)
    {
      carrierName = NSSTRING_TO_STR_OR_EMPTY([carrier carrierName]);
    }

    [netinfo release];
  }

  return carrierName;
}

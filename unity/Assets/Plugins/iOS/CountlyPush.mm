 #import "JSONKit.h"
 #import "CountlyPush.h"

 @implementation CountlyPush


 - (id)init
{
    self = [super init];
    return self;
}

 - (void)callUnityObject:(const char*)object Method:(const char*)method Parameter:(const char*)parameter
{
    UnitySendMessage(object, method, parameter);
}

 -(void)application:(UIApplication *)application didReceiveRemoteNotification : (NSDictionary *)userInfo
{
[self callUnityObject:"CountlyManager" Method:"OnMessageId" Parameter: userInfo];
}



char* AutonomousStringCopy (const char* string)
 {
     if (string == NULL)
         return NULL;
     
     char* res = (char*)malloc(strlen(string) + 1);
     strcpy(res, string);
     return res;
 }

-(void) displayAlertView:(NSString *)initWithTitle  message:(NSString *)message cancelButtonTitle:(NSString *)cancelButtonTitle
{
UIAlertView* alert = [[UIAlertView alloc] initWithTitle:initWithTitle message:message delegate:nil cancelButtonTitle:cancelButtonTitle otherButtonTitles: nil];
[alert show];
[alert release];
}

- (void)didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken {
    unsigned char *tokenBytes = (unsigned char *)[deviceToken bytes];
    NSString *token = [NSString stringWithFormat:@"%08x%08x%08x%08x%08x%08x%08x%08x",
                       ntohl(tokenBytes[0]), ntohl(tokenBytes[1]), ntohl(tokenBytes[2]),
                       ntohl(tokenBytes[3]), ntohl(tokenBytes[4]), ntohl(tokenBytes[5]),
                       ntohl(tokenBytes[6]), ntohl(tokenBytes[7])];
   [self callUnityObject:"CountlyManager" Method:"OnRegisterId" Parameter:[token UTF8String]];
}

- (void)didFailToRegisterForRemoteNotifications {
   
}

@end

static CountlyPush *countly = nil;

NSString* CreateNSString (const char* string)
{
	if (string)
		return [NSString stringWithUTF8String: string];
	else
		return [NSString stringWithUTF8String: ""];
}




extern "C" {
	void _Init () {
		if (countly == nil) 
			countly = [[CountlyPush alloc] init];
	}



	void _DisplayView (const string* title, const string* message, const string* cancelTitle) {
		[countly displayAlertView: title message:message cancelButtonTitle:cancelTitle];
	}
}
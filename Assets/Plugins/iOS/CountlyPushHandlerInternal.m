#import <objc/runtime.h>
#import "CountlyPushHandlerInternal.h"
#import <UserNotifications/UserNotifications.h>

NSString* const kCountlyPNKeyCountlyPayload     = @"c";
NSString* const kCountlyPNKeyNotificationID     = @"i";
NSString* const kCountlyPNKeyButtons            = @"b";
NSString* const kCountlyPNKeyDefaultURL         = @"l";
NSString* const kCountlyPNKeyAttachment         = @"a";
NSString* const kCountlyPNKeyActionButtonIndex  = @"b";
NSString* const kCountlyPNKeyActionButtonTitle  = @"t";
NSString* const kCountlyPNKeyActionButtonURL    = @"l";
NSString* const kCountlySavedPayload            = @"saved_payload";
NSString* const kCountlyActionIdentifier        = @"CountlyActionIdentifier";
NSString* const kCountlyCategoryIdentifier      = @"CountlyCategoryIdentifier";


@interface UIApplication(countlyPushHandlerInternal) <UNUserNotificationCenterDelegate>

@end

char * listenerGameObject = 0;
void setListenerGameObject(char * listenerName)
{
    free(listenerGameObject);
    listenerGameObject = 0;
    unsigned long len = strlen(listenerName);
    listenerGameObject = malloc(len+1);
    strcpy(listenerGameObject, listenerName);
}

void registerForRemoteNotifications()
{
    UIApplication *application = [UIApplication sharedApplication];
    // Register for Push Notitications, if running on iOS >= 8
    if ([application respondsToSelector:@selector(registerUserNotificationSettings:)])
    {
        UIUserNotificationType userNotificationTypes = (UIUserNotificationTypeAlert |
                                                        UIUserNotificationTypeBadge |
                                                        UIUserNotificationTypeSound);
        UIUserNotificationSettings *settings = [UIUserNotificationSettings settingsForTypes:userNotificationTypes
                                                                                 categories:nil];
        [application registerUserNotificationSettings:settings];
        [application registerForRemoteNotifications];
    }
    else
    {
        // Register for Push Notifications, if running iOS version < 8
        [application registerForRemoteNotificationTypes:(UIRemoteNotificationTypeBadge |
                                                         UIRemoteNotificationTypeAlert |
                                                         UIRemoteNotificationTypeSound)];
    }
    
    // If App opened with Notification
    NSString *savedPayload = [[NSUserDefaults standardUserDefaults]
        stringForKey:kCountlySavedPayload];
    if (savedPayload != nil) {
        [[NSUserDefaults standardUserDefaults] removeObjectForKey:kCountlySavedPayload];
        const char * payload = [savedPayload UTF8String];
        UnitySendMessage(listenerGameObject, "OnPushNotificationsReceived", payload);
    }
}

@implementation UIApplication(countlyPushHandlerInternal)

+(void)load
{
    NSLog(@"%s",__FUNCTION__);
    method_exchangeImplementations(class_getInstanceMethod(self, @selector(setDelegate:)), class_getInstanceMethod(self, @selector(setcountlyDelegate:)));
}

BOOL countlyRunTimeDidFinishLaunching(id self, SEL _cmd, id application, id launchOptions)
{
	BOOL result = YES;
	
	if ([self respondsToSelector:@selector(application:countlydidFinishLaunchingWithOptions:)])
    {
		result = (BOOL) [self application:application countlydidFinishLaunchingWithOptions:launchOptions];
	}
    else
    {
		[self applicationDidFinishLaunching:application];
		result = YES;
	}
    
    NSDictionary *notification =[launchOptions objectForKey:UIApplicationLaunchOptionsRemoteNotificationKey];
         if (notification) {
             NSError *error;
             NSDictionary * payload = notification[kCountlyPNKeyCountlyPayload];
             NSData *jsonData = [NSJSONSerialization dataWithJSONObject:payload
                                                                options:NSJSONWritingPrettyPrinted
                                                                  error:&error];
             if (jsonData != nil)
             {
                 NSString *jsonString = nil;
                 jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
                 NSString *valueToSave = jsonString;
                 [[NSUserDefaults standardUserDefaults] setObject:valueToSave forKey:kCountlySavedPayload];
                 [[NSUserDefaults standardUserDefaults] synchronize];
             }
         }
   
	return result;
}

void countlyRunTimeDidRegisterUserNotificationSettings(id self, SEL _cmd, id application, id notificationSettings)
{
    if ([self respondsToSelector:@selector(application:countlydidRegisterUserNotificationSettings:)])
    {
        [self application:application countlydidRegisterUserNotificationSettings:notificationSettings];
    }
    NSString *setting = @"DidRegisterUserNotificationSettings successfully";
    const char * str = [setting UTF8String];
    
    UnitySendMessage(listenerGameObject, "OnDidRegisterUserNotificationSettings", str);
}

void countlyRunTimeDidRegisterForRemoteNotificationsWithDeviceToken(id self, SEL _cmd, id application, id devToken)
{
	if ([self respondsToSelector:@selector(application:countlydidRegisterForRemoteNotificationsWithDeviceToken:)])
    {
		[self application:application countlydidRegisterForRemoteNotificationsWithDeviceToken:devToken];
	}
    
    NSData * deviceToken = devToken;
    const char* bytes = [devToken bytes];
    NSMutableString *token = NSMutableString.new;
    for (NSUInteger i = 0; i < deviceToken.length; i++)
    {
        [token appendFormat:@"%02hhx", bytes[i]];
    }
        
    const char * str = [token UTF8String];
    UnitySendMessage(listenerGameObject, "OnDidRegisterForRemoteNotificationsWithDeviceToken", str);
}

void countlyRunTimeDidFailToRegisterForRemoteNotificationsWithError(id self, SEL _cmd, id application, id error)
{
	if ([self respondsToSelector:@selector(application:countlydidFailToRegisterForRemoteNotificationsWithError:)])
    {
		[self application:application countlydidFailToRegisterForRemoteNotificationsWithError:error];
	}
	NSString *errorString = [error description];
    const char * str = [errorString UTF8String];
    UnitySendMessage(listenerGameObject, "OnDidFailToRegisterForRemoteNotificationsWithError", str);
}

void countlyRunTimeDidReceiveRemoteNotification(id self, SEL _cmd, id application, id userInfo)
{
	if ([self respondsToSelector:@selector(application:countlydidReceiveRemoteNotification:)])
    {
		[self application:application countlydidReceiveRemoteNotification:userInfo];
	}
    
    //NSString* notificationID = countlyPayload[kCountlyPNKeyNotificationID];
   
    NSDictionary * notification = userInfo;

    id alert = notification[@"aps"][@"alert"];
    NSString* message = nil;
    NSString* title = nil;

    if ([alert isKindOfClass:NSDictionary.class])
    {
        message = alert[@"body"];
        title = alert[@"title"];
    }
    else
    {
        message = (NSString*)alert;
        title = [NSBundle.mainBundle objectForInfoDictionaryKey:@"CFBundleDisplayName"];
    }

    if (!message)
    {
        return;
    }
    
    if (((UIApplication *)application).applicationState == UIApplicationStateActive)
    {
            // Nothing to do if applicationState is Inactive, the iOS already displayed an alert view.
        
            UIAlertView *alertView = [[UIAlertView alloc] initWithTitle:title message:message delegate:self cancelButtonTitle:@"OK" otherButtonTitles:nil];
            [alertView show];
        
//        UIAlertController* alert = [UIAlertController alertControllerWithTitle:title
//                                   message:message
//                                   preferredStyle:UIAlertControllerStyleAlert];
//
//        UIAlertAction* defaultAction = [UIAlertAction actionWithTitle:@"OK" style:UIAlertActionStyleDefault
//                                       handler:^(UIAlertAction * action) {
//            NSString* defaultURL = countlyPayload[kCountlyPNKeyDefaultURL];
//            if (defaultURL)
//            {
//                [self openURL:[NSURL URLWithString:defaultURL] completionHandler:nil];
//            }
//        }];
//
//        [alert addAction:defaultAction];
    }
//    NSString* defaultURL = countlyPayload[kCountlyPNKeyDefaultURL];
//
//
//    NSArray* buttons = countlyPayload[kCountlyPNKeyButtons];
    
    NSError *error;
    NSDictionary* countlyPayload = userInfo[kCountlyPNKeyCountlyPayload];
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:countlyPayload
                                                       options:NSJSONWritingPrettyPrinted // Pass 0 if you don't care about the readability of the generated string
                                                         error:&error];
    NSString *jsonString = nil;
    if (! jsonData)
    {
        NSLog(@"Got an error: %@", error);
        return;
    }
    
    jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    const char * str = [jsonString UTF8String];
    UnitySendMessage(listenerGameObject, "OnPushNotificationsReceived", str);
}



static void exchangeMethodImplementations(Class class, SEL oldMethod, SEL newMethod, IMP impl, const char * signature)
{
	Method method = nil;
    //Check whether method exists in the class
	method = class_getInstanceMethod(class, oldMethod);
	
	if (method)
    {
		//if method exists add a new method 
		class_addMethod(class, newMethod, impl, signature);
        //and then exchange with original method implementation
		method_exchangeImplementations(class_getInstanceMethod(class, oldMethod), class_getInstanceMethod(class, newMethod));
	}
    else
    {
		//if method does not exist, simply add as orignal method
		class_addMethod(class, oldMethod, impl, signature);
	}
}

- (void) setcountlyDelegate:(id<UIApplicationDelegate>)delegate
{
    
	static Class delegateClass = nil;
	
	if(delegateClass == [delegate class])
	{
		[self setcountlyDelegate:delegate];
		return;
	}
	
	delegateClass = [delegate class];
    
    
    
	exchangeMethodImplementations(delegateClass, @selector(application:didFinishLaunchingWithOptions:),
                                  @selector(application:countlydidFinishLaunchingWithOptions:), (IMP)countlyRunTimeDidFinishLaunching, "v@:::");
    
    exchangeMethodImplementations(delegateClass, @selector(application:didRegisterUserNotificationSettings:),
                                  @selector(application:countlydidRegisterUserNotificationSettings:), (IMP)countlyRunTimeDidRegisterUserNotificationSettings, "v@:::");
    
    exchangeMethodImplementations(delegateClass, @selector(application:didRegisterForRemoteNotificationsWithDeviceToken:),
		   @selector(application:countlydidRegisterForRemoteNotificationsWithDeviceToken:), (IMP)countlyRunTimeDidRegisterForRemoteNotificationsWithDeviceToken, "v@:::");
    
	exchangeMethodImplementations(delegateClass, @selector(application:didFailToRegisterForRemoteNotificationsWithError:),
		   @selector(application:countlydidFailToRegisterForRemoteNotificationsWithError:), (IMP)countlyRunTimeDidFailToRegisterForRemoteNotificationsWithError, "v@:::");
    
	exchangeMethodImplementations(delegateClass, @selector(application:didReceiveRemoteNotification:),
		   @selector(application:countlydidReceiveRemoteNotification:), (IMP)countlyRunTimeDidReceiveRemoteNotification, "v@:::");

    if (@available(iOS 10.0, macOS 10.14, *))
        UNUserNotificationCenter.currentNotificationCenter.delegate = self;
	[self setcountlyDelegate:delegate];
}

-  (const char *)payloadToJsonString:(NSDictionary*)payload
{
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:payload
                                                       options:NSJSONWritingPrettyPrinted
                                                         error:&error];
    NSString *jsonString = nil;
    if (! jsonData)
    {
        NSLog(@"Got an error: %@", error);
        return NULL;
    }
    
    jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    const char * str = [jsonString UTF8String];
    
    return str;
}

- (void)onNotificationReceived:(NSDictionary*)payload
{
    const char * str = [self payloadToJsonString:payload];
    UnitySendMessage(listenerGameObject, "OnPushNotificationsReceived", str);
}

- (void)onNotificationClick:(NSDictionary*)payload
{
    const char * str = [self payloadToJsonString:payload];
    UnitySendMessage(listenerGameObject, "OnPushNotificationsClicked", str);
}

- (void)userNotificationCenter:(UNUserNotificationCenter *)center willPresentNotification:(UNNotification *)notification withCompletionHandler:(void (^)(UNNotificationPresentationOptions options))completionHandler API_AVAILABLE(ios(10.0), macos(10.14))
{
    

    NSLog(@"userNotificationCenter");
    
    NSDictionary* countlyPayload = notification.request.content.userInfo[kCountlyPNKeyCountlyPayload];
    NSString* notificationID = countlyPayload[kCountlyPNKeyNotificationID];

    if (notificationID)
        completionHandler(UNNotificationPresentationOptionAlert);

    id<UNUserNotificationCenterDelegate> appDelegate = (id<UNUserNotificationCenterDelegate>)UIApplication.sharedApplication.delegate;

    if ([appDelegate respondsToSelector:@selector(userNotificationCenter:willPresentNotification:withCompletionHandler:)])
        [appDelegate userNotificationCenter:center willPresentNotification:notification withCompletionHandler:completionHandler];
    else
        completionHandler(UNNotificationPresentationOptionNone);
    
    [self onNotificationReceived:countlyPayload];
}

- (void)userNotificationCenter:(UNUserNotificationCenter *)center didReceiveNotificationResponse:(UNNotificationResponse *)response withCompletionHandler:(void (^)(void))completionHandler API_AVAILABLE(ios(10.0), macos(10.14))
{
    NSLog(@"userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:");
    
    NSDictionary* countlyPayload = response.notification.request.content.userInfo[kCountlyPNKeyCountlyPayload];
    NSString* notificationID = countlyPayload[kCountlyPNKeyNotificationID];

    if (notificationID)
    {
        int buttonIndex = 0;
        NSString* URL = nil;

        if ([response.actionIdentifier isEqualToString:UNNotificationDefaultActionIdentifier])
        {
            URL = countlyPayload[kCountlyPNKeyDefaultURL];
        }
        else if ([response.actionIdentifier hasPrefix:kCountlyActionIdentifier])
        {
            buttonIndex = [[response.actionIdentifier stringByReplacingOccurrencesOfString:kCountlyActionIdentifier withString:@""] intValue];
            URL = countlyPayload[kCountlyPNKeyButtons][buttonIndex - 1][kCountlyPNKeyActionButtonURL];
        }
        
        NSMutableDictionary *mutablePayload = [countlyPayload mutableCopy];

        [mutablePayload setObject:[NSNumber numberWithInt:buttonIndex] forKey:@"action_index"];
        [self onNotificationClick:mutablePayload];

       // [self openURL:URL];
    }
    
    id<UNUserNotificationCenterDelegate> appDelegate = (id<UNUserNotificationCenterDelegate>)UIApplication.sharedApplication.delegate;

    if ([appDelegate respondsToSelector:@selector(userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:)])
        [appDelegate userNotificationCenter:center didReceiveNotificationResponse:response withCompletionHandler:completionHandler];
    else
        completionHandler();
}
@end

#import <UIKit/UIKit.h>;
#import <Foundation/Foundation.h>;

@interface CountlyPush : NSObject

extern void UnitySendMessage(const char *, const char *, const char *);
-(void)application:(UIApplication *)application didReceiveRemoteNotification : (NSDictionary *)userInfo

@end

//
//  UnityConnect.mm
//  UnityFramework
//
//  Created by Alexandr on 22.11.2021.
//

#import "UnityConnect.h"
#import <Foundation/Foundation.h>
#include "UnityFramework/UnityFramework-Swift.h"

#pragma mark - C interface

@implementation UnityConnect

+ (instancetype)sharedInstance
{
    static UnityConnect *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[UnityConnect alloc] init];
    });

    return sharedInstance;
}

- (void)callToUnity:(NSString*)string {
    const char * message = string.UTF8String;
    DataPlatformCallbackDelegate delegate = _callbackDelegate;
    if (delegate != NULL) {
        delegate(message);
    }
}

- (void)sendEventName:(NSString *) name data:(NSString *) data eventId:(NSString *) eventId {
    [[DataPlatformWrapper shared] addEventItemWithName:name value:data date:[NSDate date] uuid:[NSUUID UUID]];
    [[DataPlatformWrapper shared] sendAllData];
}

- (void)setupKey:(NSString *) APIkey host:(NSString *) APIhost {
    [[DataPlatformWrapper shared] setupWithKey:APIkey host:APIhost];
}

@end

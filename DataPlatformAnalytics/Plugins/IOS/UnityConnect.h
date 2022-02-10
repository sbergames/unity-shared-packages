//
//  UnityConnect.h
//  Unity-iPhone
//
//  Created by Alexandr on 22.11.2021.
//

#ifndef UnityConnect_h
#define UnityConnect_h

#import <CoreData/CoreData.h>

typedef void (*DataPlatformCallbackDelegate)(const char* text);

@interface UnityConnect: NSObject

+ (instancetype)sharedInstance;

@property DataPlatformCallbackDelegate callbackDelegate;

- (void)callToUnity:(NSString*)string;
- (void)sendEventName:(NSString *) name data:(NSString *) data eventId:(NSString *) eventId;
- (void)setupKey:(NSString *) APIkey host:(NSString *) APIhost;

@end

#endif /* UnityConnect_h */

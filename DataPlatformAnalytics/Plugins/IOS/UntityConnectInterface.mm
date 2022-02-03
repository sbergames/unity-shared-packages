//
//  UntityConnectInterface.h
//  UnityFramework
//
//  Created by Alexandr on 24.01.2022.
//

#import <Foundation/Foundation.h>
#import "UnityConnect.h"

typedef void (*DataPlatformCallbackDelegate)(const char* text);

/// Функция, которая передает из Юнити CallbackDelegate - через который
/// мы можем отправлять данные в Юнити
extern "C" void setCallbackDelegate(DataPlatformCallbackDelegate delegate) {
    [[UnityConnect sharedInstance] setCallbackDelegate:delegate];
}

extern "C" void setupSettingsIOS(const char* APIkey, const char* APIhost) {
    NSString* key = [NSString stringWithCString:APIkey encoding:NSUTF8StringEncoding];
    NSString* host = [NSString stringWithCString:APIhost encoding:NSUTF8StringEncoding];
    [[UnityConnect sharedInstance] setupKey:key host:host];
}

extern "C" void sendEventIOS(const char* eventData, const char* eventName, const char* eventId) {
    NSString* data = [NSString stringWithCString:eventData encoding:NSUTF8StringEncoding];
    NSString* name = [NSString stringWithCString:eventName encoding:NSUTF8StringEncoding];
    NSString* eId = [NSString stringWithCString:eventId encoding:NSUTF8StringEncoding];
    
    [[UnityConnect sharedInstance] sendEventName:name data:data eventId:eId];
}

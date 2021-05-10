//
//  BugsnagNative.m
//  BugsnagNative
//
//  Created by Alexander Moinet on 14/09/2018.
//  Copyright © 2018 Bugsnag. All rights reserved.
//

#import "BugsnagNative.h"

@implementation BugsnagNative

+(void) crashMe
{
    NSArray *items = @[@1, @2, @3];
    NSLog(@"item 4: %ld", [items[4] longValue]);
}

@end

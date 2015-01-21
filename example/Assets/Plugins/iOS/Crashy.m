#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

void ExampleNativeCrash() {

    [NSException raise:@"NativeCrash" format:@"from the foreground objective C"];

}

void ExampleNativeSegfault() {
    printf("ohai %s", (char *)0);
}

void ExampleCrashInBackground() {

    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT,
                0), ^{

        [NSException raise:@"NativeCrash" format:@"from the background objective C"];
    });
}

bump:
ifeq ($(VERSION),)
	@$(error VERSION is not defined. Run with `make VERSION=number bump`)
endif
	@echo Bumping the version number to $(VERSION)
	@sed -i '' "s/@\"version\": @\".*\",/@\"version\": @\"$(VERSION)\",/" src/BugsnagUnity.mm
	@sed -i '' "s/NOTIFIER_VERSION = .*;/NOTIFIER_VERSION = \"$(VERSION)\";/"\
	 bugsnag-android-unity/src/main/java/com/bugsnag/android/unity/UnityCallback.java


#  src/BugsnagUnity.mm
# @"version": @"3.5.1",

# android/src/main/java/com/bugsnag/android/unity/UnityCallback.java
# static final String NOTIFIER_VERSION = "3.5.1";

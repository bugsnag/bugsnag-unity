bump:
ifeq ($(VERSION),)
		@$(error VERSION is not defined. Run with `make VERSION=number bump`)
endif
		@echo Bumping the version number to $(VERSION)
		@sed -i '' "s/## TBD/## $(VERSION) ($(shell date '+%Y-%m-%d'))/" CHANGELOG.md
		@sed -i '' "s/^var version = \".*\";/var version = \"$(VERSION)\";/" build.cake

bump_cake:
ifeq ($(VERSION),)
		@$(error VERSION is not defined. Run with `make VERSION=number bump_cake`)
endif
	if [[ "$OSTYPE" == "darwin"* ]]; then
		@sed -i '' "s/^var version = \".*\";/var version = \"$(VERSION)\";/" build.cake
	else
		@sed -i "s/^var version = \".*\";/var version = \"$(VERSION)\";/" build.cake
	fi
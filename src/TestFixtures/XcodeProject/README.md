# Xcode fixture project

A minimal, hand-authored `.xcodeproj` with one static-library target holding one Swift file,
used by `IntegrationTest/XcodeRegression` to exercise the Xcode path end to end: target discovery
via `xcodebuild -list -json`, symbol-graph extraction via `xcodebuild` with `OTHER_SWIFT_FLAGS`,
and `MARKETING_VERSION` read and written back in `project.pbxproj`.

The project directory is checked in as `App.xcodeproj.template` and renamed by the fixture when
it copies the tree to a temporary directory — for the same reason `SwiftPackage/Package.swift` is
a template. A real `App.xcodeproj` here would make this repository an Xcode tree, so every run of
EasySemVer over the repo, including the plain integration regression, would pay an `xcodebuild`.

`CURRENT_PROJECT_VERSION = 42` is present on purpose: it must survive a run untouched (MVR-06).

import XCTest
@testable import Widgets

// Present so the fixture covers a test target: UNI-03 makes those units too, and a plain
// `swift build` does not build them.
final class WidgetsTests: XCTestCase {
    func testPointOffsets() {
        XCTAssertEqual(Point(x: 1).offset(by: 2).x, 3)
    }
}

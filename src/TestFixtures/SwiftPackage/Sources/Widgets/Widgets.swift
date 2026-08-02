// A dependency-free fixture: `swift build` over it never resolves anything, so the live
// extraction test needs no network (TST-M6).

public struct Point {
    public var x: Int
    public var y: Int

    public init(x: Int, y: Int = 0) {
        self.x = x
        self.y = y
    }

    public func offset(by delta: Int) -> Point {
        Point(x: x + delta, y: y)
    }
}

public protocol Movable {
    func move(to point: Point) throws
}

public enum Colour {
    case red
    case green(shade: Int)
}

open class Gadget: Movable {
    public var name: String = ""

    public init(name: String) {
        self.name = name
    }

    public func move(to point: Point) throws {
    }
}

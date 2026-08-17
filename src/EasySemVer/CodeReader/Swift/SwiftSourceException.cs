namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// SWE-05 / D-03 - if Swift units exist and their source cannot be found, the run fails. There is
/// no skip-and-warn: a partial baseline would silently under-report the next change.
/// <para>
/// What counts as "cannot be found" is narrower than it was. A target whose source directory is
/// missing is a broken package and fails here; a target whose directory holds no Swift at all is
/// an ordinary Objective-C or C target, and is recorded as a unit with no API surface.
/// </para>
/// </summary>
public class SwiftSourceException(string unitDescription, string detail)
    : Exception($"Swift extraction failed for {unitDescription}: {detail}")
{
}

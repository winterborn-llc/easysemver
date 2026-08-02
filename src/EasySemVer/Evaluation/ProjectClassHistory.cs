using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluation;

internal class ProjectClassHistory(IProjectClass older, IProjectClass newer) : IProjectClassHistory
{
    public IProjectClass Older { get; } = older;

    public IProjectClass Newer { get; } = newer;
}
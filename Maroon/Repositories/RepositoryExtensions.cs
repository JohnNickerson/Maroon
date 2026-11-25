using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Repositories;

public static class RepositoryExtensions
{
	public static ActionItem? GetProject(this IRepository<ActionItem> repository, ActionItem child)
	{
		return child.ProjectId.HasValue ? repository.Find(child.ProjectId.Value) : null;
	}

	public static ActionItem? GetParent(this IRepository<ActionItem> repository, ActionItem child)
	{
		return child.ParentId.HasValue ? repository.Find(child.ParentId.Value) : null;
	}

	public static int GetRankDepth(this IRepository<ActionItem> repository, ActionItem child)
	{
		if (child.ParentId == null) return 0;
		var parent = repository.GetParent(child);
		if (parent == null) return 0;
		var ancestors = new List<ActionItem> { parent };
		var cursor = repository.GetParent(parent);
		while (cursor != null && !ancestors.Contains(cursor))
		{
			ancestors.Add(cursor);
			cursor = repository.GetParent(cursor);
		}

		return ancestors.Count;
	}
}
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Interfaces;

public interface IActionItemRepository : IRepository<ActionItem>
{
	public ActionItem? GetProject(ActionItem child)
	{
		return child.ProjectId.HasValue ? Find(child.ProjectId.Value) : null;
	}

	public ActionItem? GetParent(ActionItem? child)
	{
		if (child == null) return null;
		return child.ParentId.HasValue ? Find(child.ParentId.Value) : null;
	}

	public int GetRankDepth(ActionItem child)
	{
		if (child.ParentId == null) return 0;
		var parent = GetParent(child);
		if (parent == null) return 0;
		var ancestors = new List<ActionItem> { parent };
		var cursor = GetParent(parent);
		while (cursor != null && !ancestors.Contains(cursor))
		{
			ancestors.Add(cursor);
			cursor = GetParent(cursor);
		}

		return ancestors.Count;
	}

}
using System.Collections.Generic;

public static class TypeLogicalSort
{
    public static Dictionary<SpriteSorting, bool> visited = new Dictionary<SpriteSorting, bool>(64);
    public static List<SpriteSorting> allSprites = new List<SpriteSorting>(64);
    public static List<SpriteSorting> Sort(List<SpriteSorting> staticSprites, List<SpriteSorting> movableSprites, List<SpriteSorting> sorted)
    {
        allSprites.Clear();
        allSprites.AddRange(staticSprites);
        allSprites.AddRange(movableSprites);
        visited.Clear();
        for (int i = 0; i < allSprites.Count; i++)
        {
            Visit(allSprites[i], sorted, visited);
        }

        return sorted;
    }

    public static void Visit(SpriteSorting item, List<SpriteSorting> sorted, Dictionary<SpriteSorting, bool> visited)
    {
        bool inProcess;
        var alreadyVisited = visited.TryGetValue(item, out inProcess);

        if (!alreadyVisited)
        {
            visited[item] = true;

            List<SpriteSorting> dependencies = item.ActiveDependencies;
            for (int i = 0; i < dependencies.Count; i++)
            {
                Visit(dependencies[i], sorted, visited);
            }

            visited[item] = false;
            sorted.Add(item);
        }
    }
}

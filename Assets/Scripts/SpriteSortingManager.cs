using System.Collections.Generic;

public class SpriteSortingManager : Singleton<SpriteSortingManager>
{
    private static List<SpriteSorting> floorSpriteList = new List<SpriteSorting>(64);
    private static List<SpriteSorting> staticSpriteList = new List<SpriteSorting>(256);
    private static List<SpriteSorting> currentlyVisibleStaticSpriteList = new List<SpriteSorting>();

    private static List<SpriteSorting> moveableSpriteList = new List<SpriteSorting>(64);
    private static List<SpriteSorting> currentlyVisibleMoveableSpriteList = new List<SpriteSorting>();

    public static void RegisterSprite(SpriteSorting newSprite)
    {
        if (newSprite.renderBelowAll)
        {
            floorSpriteList.Add(newSprite);
            SortListSimple(floorSpriteList);
            SetSortOrderNegative(floorSpriteList);
        }
        else
        {
            if (newSprite.isMovable)
            {
                moveableSpriteList.Add(newSprite);
            }
            else
            {
                staticSpriteList.Add(newSprite);
                SetupStaticDependencies(newSprite);
            }
        }
    }

    private static void SetupStaticDependencies(SpriteSorting newSprite)
    {
        int the_count = staticSpriteList.Count;
        for (int i = 0; i < the_count; i++)
        {
            SpriteSorting otherSprite = staticSpriteList[i];
            if (CalculateBoundsIntersection(newSprite, otherSprite))
            {
                int compareResult = SpriteSorting.CompareIsoSorters(newSprite, otherSprite);
                if (compareResult == -1)
                {
                    otherSprite.staticDependencies.Add(newSprite);
                    newSprite.inverseStaticDependencies.Add(otherSprite);
                }
                else if (compareResult == 1)
                {
                    newSprite.staticDependencies.Add(otherSprite);
                    otherSprite.inverseStaticDependencies.Add(newSprite);
                }
            }
        }
    }

    public static void UnregisterSprite(SpriteSorting spriteToRemove)
    {
        if (spriteToRemove.renderBelowAll)
        {
            floorSpriteList.Remove(spriteToRemove);
        }
        else
        {
            if (spriteToRemove.isMovable)
            {
                moveableSpriteList.Remove(spriteToRemove);
            }
            else
            {
                staticSpriteList.Remove(spriteToRemove);
                RemoveStaticDependencies(spriteToRemove);
            }
        }
    }

    private static void RemoveStaticDependencies(SpriteSorting spriteToRemove)
    {
        for (int i = 0; i < spriteToRemove.inverseStaticDependencies.Count; i++)
        {
            SpriteSorting otherSprite = spriteToRemove.inverseStaticDependencies[i];
            otherSprite.staticDependencies.Remove(spriteToRemove);
        }
    }

    void Update()
    {
        UpdateSorting();
    }

    private static List<SpriteSorting> sortedSprites = new List<SpriteSorting>(64);
    public static void UpdateSorting()
    {
        FilterListByVisibility(staticSpriteList, currentlyVisibleStaticSpriteList);
        FilterListByVisibility(moveableSpriteList, currentlyVisibleMoveableSpriteList);

        ClearMovingDependencies(currentlyVisibleStaticSpriteList);
        ClearMovingDependencies(currentlyVisibleMoveableSpriteList);

        AddMovingDependenciesToStaticSprites(currentlyVisibleMoveableSpriteList, currentlyVisibleStaticSpriteList);
        AddMovingDependenciesToMovingSprites(currentlyVisibleMoveableSpriteList);

        sortedSprites.Clear();
        TypeLogicalSort.Sort(currentlyVisibleStaticSpriteList, currentlyVisibleMoveableSpriteList, sortedSprites);
        SetSortOrderBasedOnListOrder(sortedSprites);
    }

    private static void AddMovingDependenciesToStaticSprites(List<SpriteSorting> moveableList, List<SpriteSorting> staticList)
    {
        for (int i = 0; i < moveableList.Count; i++)
        {
            SpriteSorting moveSprite = moveableList[i];
            for (int j = 0; j < staticList.Count; j++)
            {
                SpriteSorting staticSprite = staticList[j];
                if (CalculateBoundsIntersection(moveSprite, staticSprite))
                {
                    int compareResult = SpriteSorting.CompareIsoSorters(moveSprite, staticSprite);
                    if (compareResult == -1)
                    {
                        staticSprite.movingDependencies.Add(moveSprite);
                    }
                    else if (compareResult == 1)
                    {
                        moveSprite.movingDependencies.Add(staticSprite);
                    }
                }
            }
        }
    }

    private static void AddMovingDependenciesToMovingSprites(List<SpriteSorting> moveableList)
    {
        for (int i = 0; i < moveableList.Count; i++)
        {
            SpriteSorting sprite1 = moveableList[i];
            for (int j = 0; j < moveableList.Count; j++)
            {
                SpriteSorting sprite2 = moveableList[j];
                if (CalculateBoundsIntersection(sprite1, sprite2))
                {
                    int compareResult = SpriteSorting.CompareIsoSorters(sprite1, sprite2);
                    if (compareResult == -1)
                    {
                        sprite2.movingDependencies.Add(sprite1);
                    }
                }
            }
        }
    }

    private static void ClearMovingDependencies(List<SpriteSorting> sprites)
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            sprites[i].movingDependencies.Clear();
        }
    }

    private static bool CalculateBoundsIntersection(SpriteSorting sprite, SpriteSorting otherSprite)
    {
        return sprite.TheBounds.Intersects(otherSprite.TheBounds);
    }

    private static void SetSortOrderBasedOnListOrder(List<SpriteSorting> spriteList)
    {
        int orderCurrent = 0;
        for (int i = 0; i < spriteList.Count; ++i)
        {
            spriteList[i].RendererSortingOrder = orderCurrent;
            orderCurrent += 1;
        }
    }

    private static void SetSortOrderNegative(List<SpriteSorting> spriteList)
    {
        int startOrder = -spriteList.Count - 1;
        for (int i = 0; i < spriteList.Count; ++i)
        {
            spriteList[i].RendererSortingOrder = startOrder + i;
        }
    }

    public static void FilterListByVisibility(List<SpriteSorting> fullList, List<SpriteSorting> destinationList)
    {
        destinationList.Clear();
        for (int i = 0; i < fullList.Count; i++)
        {
            SpriteSorting sprite = fullList[i];
            if (sprite.forceSort)
            {
                destinationList.Add(sprite);
                sprite.forceSort = false;
            }
            else
            {
                for (int j = 0; j < sprite.renderersToSort.Length; j++)
                {
                    if (sprite.renderersToSort[j].isVisible)
                    {
                        destinationList.Add(sprite);
                        break;
                    }
                }
            }
        }
    }

    private static void SortListSimple(List<SpriteSorting> list)
    {
        list.Sort((a, b) =>
        {
            if (!a || !b)
            {
                return 0;
            }
            else
            {
                return SpriteSorting.CompareIsoSorters(a, b);
            }
        });
    }
}

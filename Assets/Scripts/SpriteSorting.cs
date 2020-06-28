using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSorting : MonoBehaviour
{
    public bool isMovable;
    public bool renderBelowAll;

    [NonSerialized]
    public bool forceSort;

    [NonSerialized]
    public List<SpriteSorting> staticDependencies = new List<SpriteSorting>(16);
    [NonSerialized]
    public List<SpriteSorting> inverseStaticDependencies = new List<SpriteSorting>(16);
    public List<SpriteSorting> movingDependencies = new List<SpriteSorting>(8);

    private readonly List<SpriteSorting> visibleStaticDependencies = new List<SpriteSorting>(16);
    private List<SpriteSorting> activeDependencies = new List<SpriteSorting>(16);
    public List<SpriteSorting> ActiveDependencies
    {
        get
        {
            activeDependencies.Clear();
            SpriteSortingManager.FilterListByVisibility(staticDependencies, visibleStaticDependencies);
            activeDependencies.AddRange(visibleStaticDependencies);
            activeDependencies.AddRange(movingDependencies);
            return activeDependencies;
        }
    }

    public enum SortType
    {
        Point,
        Line
    }

    public SortType sortType = SortType.Point;

    public Vector3 SorterPositionOffset = new Vector3();
    public Vector3 SorterPositionOffset2 = new Vector3();

    private Transform t;

    private Vector3 SortingPoint1
    {
        get
        {
            return SorterPositionOffset + t.position;
        }
    }
    private Vector3 SortingPoint2
    {
        get
        {
            return SorterPositionOffset2 + t.position;
        }
    }

    public Vector3 AsPoint
    {
        get
        {
            if (sortType == SortType.Line)
                return ((SortingPoint1 + SortingPoint2) / 2);
            else
                return SortingPoint1;
        }
    }

    public bool IsNear(Vector3 point, float distance)
    {
        if (sortType == SortType.Point)
        {
            return Vector2.Distance(point, SortingPoint1) <= distance;
        }
        else
        {
            bool nearPoint1 = Vector2.Distance(point, SortingPoint1) <= distance;
            bool nearPoint2 = Vector2.Distance(point, SortingPoint2) <= distance;
            return nearPoint1 || nearPoint2;
        }
    }

    private float SortingLineCenterHeight
    {
        get
        {
            if (sortType == SortType.Line)
            {
                return ((SortingPoint1.y + SortingPoint2.y) / 2);
            }
            else
            {
                Debug.LogError("calling line center height on point type");
                return SortingPoint1.y;
            }
        }
    }

    public Renderer[] renderersToSort;

#if UNITY_EDITOR
    public void SortScene()
    {
        SpriteSorting[] isoSorters = FindObjectsOfType(typeof(SpriteSorting)) as SpriteSorting[];
        for (int i = 0; i < isoSorters.Length; i++)
        {
            isoSorters[i].Setup();
        }
        SpriteSortingManager.UpdateSorting();
        for (int i = 0; i < isoSorters.Length; i++)
        {
            isoSorters[i].Unregister();
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
#endif

    void Awake()
    {
        if (Application.isPlaying)
        {
            SpriteSortingManager temp = SpriteSortingManager.Instance; 
            Setup();
        }
    }

    private void Setup()
    {
        t = transform;
        if (renderersToSort == null || renderersToSort.Length == 0)
        {
            renderersToSort = new Renderer[] { GetComponent<Renderer>() };
        }
        if (!isMovable)
        {
            cachedBounds = new Bounds2D(renderersToSort[0].bounds);
        }
        System.Array.Sort(renderersToSort, (a, b) => a.sortingOrder.CompareTo(b.sortingOrder));
        SpriteSortingManager.RegisterSprite(this);
    }

    public static int CompairIsoSortersBasic(SpriteSorting sprite1, SpriteSorting sprite2)
    {
        float y1 = sprite1.sortType == SortType.Point ? sprite1.SortingPoint1.y : sprite1.SortingLineCenterHeight;
        float y2 = sprite2.sortType == SortType.Point ? sprite2.SortingPoint1.y : sprite2.SortingLineCenterHeight;
        return y2.CompareTo(y1);
    }

    public static int CompareIsoSorters(SpriteSorting sprite1, SpriteSorting sprite2)
    {
        if (sprite1.sortType == SortType.Point && sprite2.sortType == SortType.Point)
        {
            return sprite2.SortingPoint1.y.CompareTo(sprite1.SortingPoint1.y);
        }
        else if (sprite1.sortType == SortType.Line && sprite2.sortType == SortType.Line)
        {
            return CompareLineAndLine(sprite1, sprite2);
        }
        else if (sprite1.sortType == SortType.Point && sprite2.sortType == SortType.Line)
        {
            return ComparePointAndLine(sprite1.SortingPoint1, sprite2);
        }
        else if (sprite1.sortType == SortType.Line && sprite2.sortType == SortType.Point)
        {
            return -ComparePointAndLine(sprite2.SortingPoint1, sprite1);
        }
        else
        {
            return 0;
        }
    }

    private static int CompareLineAndLine(SpriteSorting line1, SpriteSorting line2)
    {
        Vector2 line1Point1 = line1.SortingPoint1;
        Vector2 line1Point2 = line1.SortingPoint2;
        Vector2 line2Point1 = line2.SortingPoint1;
        Vector2 line2Point2 = line2.SortingPoint2;

        int comp1 = ComparePointAndLine(line1Point1, line2);
        int comp2 = ComparePointAndLine(line1Point2, line2);
        int oneVStwo = int.MinValue;
        if (comp1 == comp2)
        {
            oneVStwo = comp1;
        }

        int comp3 = ComparePointAndLine(line2Point1, line1);
        int comp4 = ComparePointAndLine(line2Point2, line1);
        int twoVSone = int.MinValue;
        if (comp3 == comp4)
        {
            twoVSone = -comp3;
        }

        if (oneVStwo != int.MinValue && twoVSone != int.MinValue)
        {
            if (oneVStwo == twoVSone)
            {
                return oneVStwo;
            }
            return CompareLineCenters(line1, line2);
        }
        else if (oneVStwo != int.MinValue)
        {
            return oneVStwo;
        }
        else if (twoVSone != int.MinValue)
        {
            return twoVSone;
        }

        else
        {
            return CompareLineCenters(line1, line2);
        }
    }

    private static int CompareLineCenters(SpriteSorting line1, SpriteSorting line2)
    {
        return -line1.SortingLineCenterHeight.CompareTo(line2.SortingLineCenterHeight);
    }

    private static int ComparePointAndLine(Vector3 point, SpriteSorting line)
    {
        float pointY = point.y;
        if (pointY > line.SortingPoint1.y && pointY > line.SortingPoint2.y)
        {
            return -1;
        }
        else if (pointY < line.SortingPoint1.y && pointY < line.SortingPoint2.y)
        {
            return 1;
        }
        else
        {
            float slope = (line.SortingPoint2.y - line.SortingPoint1.y) / (line.SortingPoint2.x - line.SortingPoint1.x);
            float intercept = line.SortingPoint1.y - (slope * line.SortingPoint1.x);
            float yOnLineForPoint = (slope * point.x) + intercept;
            return yOnLineForPoint > point.y ? 1 : -1;
        }
    }

    private static bool PointWithinLineArea(Vector3 point, Vector3 linePoint1, Vector3 linePoint2)
    {
        bool xMatch = Mathf.Abs(linePoint1.x - point.x) + Mathf.Abs(linePoint2.x - point.x) == Mathf.Abs(linePoint1.x - linePoint2.x);
        bool yMatch = Mathf.Abs(linePoint1.y - point.y) + Mathf.Abs(linePoint2.y - point.y) == Mathf.Abs(linePoint1.y - linePoint2.y);
        return xMatch && yMatch;
    }

    public int RendererSortingOrder
    {
        get
        {
            if (renderersToSort.Length > 0)
            {
                return renderersToSort[0].sortingOrder;
            }
            else
            {
                return 0;
            }
        }
        set
        {
            for (int j = 0; j < renderersToSort.Length; ++j)
            {
                renderersToSort[j].sortingOrder = value;
            }
        }
    }

    private Bounds2D cachedBounds;
    public Bounds2D TheBounds
    {
        get
        {
            if (isMovable)
            {
                return new Bounds2D(renderersToSort[0].bounds);
            }
            else
            {
                return cachedBounds;
            }
        }
    }

    void OnDestroy()
    {
        if (Application.isPlaying)
        {
            Unregister();
        }
    }

    private void Unregister()
    {
        SpriteSortingManager.UnregisterSprite(this);
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PaintBucket : MonoBehaviour
{
    private RawImage rawImage;
    private Texture2D tex;
    public Color setColor = Color.red;
    public string fileName;
    Queue<Vector2Int> points = new Queue<Vector2Int>();
    bool[] bChecked ;
    private void OnEnable()
    {
        tex = Resources.Load<Texture2D>("Textures/" + fileName);
        rawImage = GetComponentInChildren<RawImage>();
        rawImage.texture = tex;
        rawImage.SetNativeSize();
    }

    private void Start()
    {
        bChecked = new bool[tex.width * tex.height];
        InitData();
    }

    private void InitData()
    {
        for (int i = 0; i < bChecked.Length; i++)
        {
            bChecked[i] = false;
        }

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Click(Input.mousePosition);
        }
    }

    private void Click(Vector2 mousePosition)
    {
        //官方解释 bool Returns true if the plane of the RectTransform is hit, regardless of whether the point is inside the rectangle.
        //ScreenPointToLocalPointInRectangle这个api有点蹊跷，判断的是平面，不是矩形，有点搞--,后面这句忽略点是否在矩形里面，我属实不明白要表达啥玩意
        //不过没关系，我只用到它的坐标转换。
        if (RectTransformUtility.RectangleContainsScreenPoint(rawImage.rectTransform, mousePosition) &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.rectTransform,
            mousePosition, null, out Vector2 localPoint))
        {
            int x = (int)localPoint.x + tex.width / 2;
            int y = (int)localPoint.y + tex.height / 2;
            FloodFill(x, y, tex.GetPixel(x, y));
        }

    }

    private void FloodFill(int x, int y, Color ClickColor)
    {
        if (ColorCompare(ClickColor, setColor))
        {
            return;
        }

        InitData();

        for (int i = 0; i < tex.width; i++)
        {
            bChecked[i] = false;
        }
        points.Enqueue(new Vector2Int(x, y));
        while (points.Count > 0)
        {
            var point = points.Dequeue();

            if (!(point.x < 0 || point.y < 0 || point.x >= tex.width || point.y >= tex.height))
            {
                
                if (!bChecked[point.y * tex.width + point.x] && ColorCompare(ClickColor, tex.GetPixel(point.x,point.y)))
                {
                    bChecked[point.y * tex.width + point.x] = true;
                    tex.SetPixel(point.x, point.y, setColor);
                    points.Enqueue(new Vector2Int(point.x - 1, point.y + 1));
                    points.Enqueue(new Vector2Int(point.x - 1, point.y));
                    points.Enqueue(new Vector2Int(point.x - 1, point.y - 1));

                    points.Enqueue(new Vector2Int(point.x, point.y + 1));
                    points.Enqueue(new Vector2Int(point.x, point.y - 1));

                    points.Enqueue(new Vector2Int(point.x + 1, point.y + 1));
                    points.Enqueue(new Vector2Int(point.x + 1, point.y));
                    points.Enqueue(new Vector2Int(point.x + 1, point.y - 1));

                }
            }
        }
        tex.Apply();
    }

    /// <summary>
    /// 这个颜色有点坑爹，两个color相等判断会有问题 比如点击获取的颜色(0.555,0.3,1) 可是比较的时候，有的颜色可能是(0.556,0.3,1)
    /// 在r 上会相差那么一点，应该是存储精度的问题，所以，用距离做了个判断。不然要裂开。
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    bool ColorCompare(Color first, Color second)
    {
        Mathf.Pow(first.r - second.r, 2);

        float dis = Mathf.Sqrt(Mathf.Pow(first.r - second.r, 2) + Mathf.Pow(first.g - second.g, 2) + Mathf.Pow(first.b - second.b, 2));
        return dis < 0.01;
    }


    private void OnDisable()
    {
#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();
#endif
    }
}

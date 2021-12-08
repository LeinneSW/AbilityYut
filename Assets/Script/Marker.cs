using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public class Point
    {
        // 출발 지점을 0으로 시작
        public int Index { get; set; }

        // 중간에 안쪽으로 들어가는 경우
        public int SubIndex { get; set; }

        public Point() : this(0, 0) { }

        public Point(int index) : this(index, 0) { }

        public Point(int index, int subIndex)
        {
            Index = index;
            SubIndex = subIndex;
        }

        public Vector3 GetBoardPosition(int index)
        {
            return GetBoardPosition(index, 0);
        }

        public Vector3 GetBoardPosition(int index, int player)
        {
            if (SubIndex == 3)
            {
                return new(0, 0, 0);
            }

            if (Index == 0)
            {
                // TODO: 플레이어별 차이
                return new((player == 0 ? 1 : -1) * new float[] { 5.5f, 6.5f }[index % 2], 0, new float[] { -3.5f, -4.5f }[index / 2]);
            }
            else if (Index <= 5) // -1, 1 ~ 5
            {
                if (SubIndex == 0)
                {
                    // 도: -2.44f ~ 윷까지 1.549 차이 뒷도: -4.13f
                    return new(4.131f, 0, Index == 5 ? 4.139f : -2.44f + (Index - 1) * 1.549f);
                }
                else
                {
                    // TODO: 5의 서브인덱스 대각선 좌상 -> 우하

                }
            }
            else if (Index <= 10) // 6 ~ 10
            {
                if (SubIndex == 0)
                {
                    return new(Index == 10 ? -4.13f : 2.365f - (Index - 6) * 1.549f, 0, 4.149f);
                }
                else
                {
                    // TODO: 10의 서브인덱스 대각선 우상 -> 좌하

                }
            }
            else if (Index <= 15)
            {
                return new(-4.131f, 0, Index < 15 ? -2.4f + (14 - Index) * 1.549f : -4.149f);
            }
            return new Vector3(Index == 20 ? 4.131f : -2.279f + (Index - 16) * 1.549f, 0, -4.149f);
        }

        public override string ToString()
        {
            return "MarkerPoint(" + Index + ", " + SubIndex + ")";
        }
    }

    private static Vector2 canvasSize = new(1281.642f, 720);

    public Player owner;
    public Vector3 target;
    public Point point = new();

    private Point CanMovePoint(Yut.Result value)
    {
        var intValue = value.GetValue();
        var index = point.Index + intValue;
        var sub = point.SubIndex + intValue;
        if(point.Index == 1 && intValue == -1)
        {
            return new(20);
        }
        else if (index == -1 || value == Yut.Result.NAK)
        {
            return null;
        }
        else if (point.Index <= 10 && point.Index % 5 == 0 && sub == -1)
        {
            return new(point.Index - 1);
        }

        if (point.Index == 5) // 중간으로 빠질 수 있다
        {
            if (point.SubIndex == 3)  // 중앙일경우
            {
                point.Index = 10;
                return CanMovePoint(value);
            }
            else if (sub > 6) // 서브인덱스를 벗어난 경우
            {
                return new(15 + intValue - 6 + point.SubIndex);
            }
            else
            {
                return new(5, point.SubIndex + intValue);
            }
        }
        else if (point.Index == 10) // SubIndex가 6이상이 될 경우 도착한다
        {
            if (sub > 6)
            {
                return new(20);
            }
            else if (sub == 6)
            {
                return new(-1);
            }
            return new(10, sub);
        }
        return new(index > 20 ? -1 : index);
    }

    public void OnClick(int index, Yut.Result[] results)
    {
        foreach (var point in CanMovePoints(results))
        {
            var view = Camera.main.WorldToViewportPoint(point.GetBoardPosition(index, owner.Index));
            var guide = Instantiate(
                owner.YutMarkerGuide,
                new((view.x * canvasSize.x) - (canvasSize.x * 0.5f), (view.y * canvasSize.y) - (canvasSize.y * 0.5f) + 23),
                Quaternion.identity
            );
            guide.transform.SetParent(owner.Canvas.transform, false);
            var yutMakerGuide = guide.GetComponent<YutMarkerGuide>();
            yutMakerGuide.marker = gameObject;
            yutMakerGuide.point = point;
        }
    }

    public List<Point> CanMovePoints(Yut.Result[] value)
    {
        var list = new List<Point>();
        foreach (var result in value)
        {
            var point = CanMovePoint(result);
            if (point != null) list.Add(point);
        }
        return list;
    }

    public void Move(Point point)
    {
        this.point = point;
        if (this.point.Index < 0)
        {
            Destroy(gameObject);
            return;
        }
        target = point.GetBoardPosition(0);
    }

    private void Start()
    {
        target = new(int.MinValue, 0);
    }

    private void Update()
    {
        if (target.x == int.MinValue) return;
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * 8);
    }

    public override string ToString()
    {
        return "Marker(Point: " + point + ")";
    }
}

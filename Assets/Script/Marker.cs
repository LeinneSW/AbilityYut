using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

        public Vector3 GetBoardPosition() => GetBoardPosition(0, 0);

        public Vector3 GetBoardPosition(int index) => GetBoardPosition(index, 0);

        public Vector3 GetBoardPosition(int index, int player)
        {
            if (SubIndex == 3)
            {
                return new(0, 0, 0);
            }

            if (Index == 0)
            {
                return new((player == 0 ? 1 : -1) * new float[] { 5.5f, 6.5f }[index % 2], 0, new float[] { -3.5f, -4.5f }[index / 2]);
            }
            else if (Index <= 5) // -1, 1 ~ 5
            {
                if (SubIndex == 0)
                {
                    // 도: -2.44f ~ 윷까지 1.549 차이 뒷도: -4.13f
                    return new(4.131f, 0, Index == 5 ? 4.139f : -2.44f + (Index - 1) * 1.549f);
                }
                else if (SubIndex < 6)
                {
                    // TODO: 5의 서브인덱스 대각선 좌상 -> 우하
                    return new(
                        new float[] { 2.488f, 1.326f, 0, -1.28f, -2.508f }[SubIndex - 1],
                        0,
                        new float[] { 2.511f, 1.352f, 0, -1.261f, -2.502f }[SubIndex - 1]
                    );
                }
                else return new(-4.131f, 0, -4.139f);
            }
            else if (Index <= 10) // 6 ~ 10
            {
                if (SubIndex == 0)
                {
                    return new(Index == 10 ? -4.13f : 2.365f - (Index - 6) * 1.549f, 0, 4.149f);
                }
                else if (SubIndex < 6)
                {
                    // TODO: 10의 서브인덱스 대각선 우상 -> 좌하
                    return new(
                        new float[] { -2.494f, -1.336f, 0, 1.284f, 2.508f }[SubIndex - 1],
                        0,
                        new float[] { 2.491f, 1.342f, 0, -1.276f, -2.521f }[SubIndex - 1]
                    );
                }
                else return new(4.131f, 0, -4.149f);
            }
            else if (Index <= 15)
            {
                return new(-4.131f, 0, Index < 15 ? 2.194f - (Index - 11) * 1.549f : -4.149f);
            }
            return new Vector3(Index == 20 ? 4.131f : -2.279f + (Index - 16) * 1.549f, 0, -4.149f);
        }

        public Point Clone()
        {
            var clone = (Point)MemberwiseClone();
            return clone;
        }

        public override string ToString()
        {
            return "MarkerPoint(" + Index + ", " + SubIndex + ")";
        }

        public override bool Equals(object obj) => this == obj;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Point a, object b)
        {
            var point = b as Point;
            return a?.Index == point?.Index && a?.SubIndex == point?.SubIndex;
        }

        public static bool operator !=(Point a, object b) => !(a == b);
    }

    private static Vector2 _canvasSize = new(1920, 1080);

    public Player owner;
    public GameObject hitParticle;

    public Point point = new();
    public Point goalPoint = null;
    public Point nextPoint = new();
    public Yut.Result moveResult;

    public TextMesh carryText;
    public Marker carryMarker;
    public List<Marker> carry = new();

    private void Start()
    {
        carryText = GetComponentInChildren<TextMesh>();
    }

    private Point CanMovePoint(Yut.Result value)
    {
        var intValue = value.GetValue();
        var index = point.Index + intValue;
        var sub = point.SubIndex + intValue;
        if (point.Index == 1 && value == Yut.Result.BACK_DO)
        {
            return new(20);
        }
        else if (index == -1 || value == Yut.Result.NAK)
        {
            return null;
        }
        else if (point.Index <= 10 && point.Index % 5 == 0 && sub == -1) // 모, 모모지점에서 빽도 발생시
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
            else if (sub > 5) // 서브인덱스를 벗어난 경우
            {
                return new(15 + sub - 6);
            }
            else
            {
                return new(5, sub);
            }
        }
        else if (point.Index == 10) // SubIndex가 6이상이 될 경우 도착한다
        {
            if (sub > 5)
            {
                return new(sub == 6 ? 20 : -1);
            }
            return new(10, sub);
        }
        return new(index > 20 ? -1 : index);
    }

    public void OnClick(int index, Yut.Result[] results)
    {
        if (goalPoint != null) return;
        for (int i = 0; i < results.Length; ++i)
        {
            var result = results[i];
            var point = CanMovePoint(result);
            if (point == null) continue;

            if (point.Index == -1)
            {
                GameManager.Instance.goalButton.SetActive(true);
                var button = GameManager.Instance.goalButton.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    Move(result, point);
                    GameManager.Instance.goalButton.SetActive(false);
                });
            }
            else
            {
                var view = Camera.main.WorldToViewportPoint(point.GetBoardPosition(index, owner.index));
                var guide = Instantiate(
                    GameManager.Instance.markerMoveGuide,
                    new((view.x * _canvasSize.x) - (_canvasSize.x * 0.5f), (view.y * _canvasSize.y) - (_canvasSize.y * 0.5f) + 23),
                    Quaternion.identity
                );
                guide.transform.SetParent(GameManager.Instance.gameUICanvas.transform, false);
                var markerMoveGuide = guide.GetComponent<MarkerMoveGuide>();
                markerMoveGuide.marker = this;
                markerMoveGuide.point = point;
                markerMoveGuide.Result = result;
            }
        }
    }

    public void Move(Yut.Result result, Point goal)
    {
        if (!owner.results.Remove(result))
        {
            return;
        }
        GameManager.Instance.turnState = GameManager.TurnState.ANIMATING_MARKER;

        moveResult = result;
        goalPoint = goal.Clone();
        var moveValue = result == Yut.Result.BACK_DO ? -1 : 1;
        if (point.Index == 0)
        {
            transform.position = new Point(20).GetBoardPosition(owner.index);
        }

        if (point.Index == 1 && result == Yut.Result.BACK_DO)
        {
            nextPoint = new(20);
        }
        else if (goal.SubIndex > 0 || point.SubIndex > 0)
        {
            nextPoint = new(point.Index, point.SubIndex + moveValue);
            if (nextPoint.SubIndex < 0) nextPoint = new(point.Index - 1);
            else if (nextPoint.SubIndex > 5) nextPoint = new(point.Index + 10);
        }
        else
        {
            nextPoint.Index = point.Index + (result == Yut.Result.BACK_DO ? -1 : 1);
            if (nextPoint.Index > 20) point = new(-1);
        }
        //Debug.Log($"[SET] current: {point}, goal: {goal}, next: {nextPoint}");
    }

    public void Carry(Marker marker)
    {
        //Debug.Log($"[Carry] 싣는놈: {this}, 실리는놈: {marker}");
        if(carryMarker != null)
        {
            carryMarker.Carry(marker);
            return;
        }
        else if(marker.carry.Count > 0)
        {
            foreach (var item in marker.carry)
            {
                carry.Add(item);
                item.carryMarker = this;
            }
            marker.carry.Clear();
        }
        marker.point = new(99);
        marker.carryMarker = this;
        marker.transform.position = new(-99, -99, -99);

        carry.Add(marker);
        carryText.text = "x" + (carry.Count + 1);

        marker.gameObject.SetActive(false);
    }

    public void ResetPoint(bool particle = true)
    {
        foreach (var item in carry)
        {
            item.ResetPoint(false);
        }
        carry.Clear();
        gameObject.SetActive(true);

        point = new();
        nextPoint = new();
        goalPoint = null;

        carryMarker = null;
        carryText.text = "";
        if (particle)
        {
            Instantiate(hitParticle, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
        }
        transform.position = point.GetBoardPosition(owner._markers.IndexOf(this), owner.index);
    }

    private void Update()
    {
        if (point.Index < 0)
        {
            owner._markers.Remove(this);
            owner.markers.Remove(gameObject);
            foreach (var item in carry)
            {
                item.point = point;
                item.gameObject.SetActive(true);
            }
            GameManager.Instance.turnState = GameManager.TurnState.TRY_THROW_YUT_OR_SELECT_MARKER;

            Destroy(gameObject);
            return;
        }

        if (goalPoint == null) return;

        var target = nextPoint.GetBoardPosition(owner.index);
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * 10);
        if (transform.position == target)
        {
            //Debug.Log($"[UPDATE] current: {point}, goal: {goalPoint}, next: {nextPoint}");
            if (nextPoint == goalPoint) // 도착
            {
                var marker = GameManager.Instance.GetMarker(nextPoint, out Player player);
                //Debug.Log(marker);
                //Debug.Log(player?.name);
                if (marker != null)
                {
                    if (player != owner) player.Attack(marker, moveResult);
                    else marker.Carry(this);
                }

                goalPoint = null;
                point = nextPoint.Clone();
                foreach(var item in carry) item.point = point;
                GameManager.Instance.turnState = GameManager.TurnState.TRY_THROW_YUT_OR_SELECT_MARKER;
                return;
            }

            if (nextPoint.SubIndex > 0)
            {
                nextPoint.SubIndex +=
                    (
                        nextPoint.Index <= goalPoint.Index &&
                        (goalPoint.SubIndex == 0 || nextPoint.SubIndex <= goalPoint.SubIndex)
                    ) || goalPoint.Index == -1 ? 1 : -1;
                if (nextPoint.SubIndex < 0) nextPoint = new(point.Index - 1);
                else if (nextPoint.SubIndex > 5) nextPoint = new(point.Index + 10);
            }
            else
            {
                nextPoint.Index += goalPoint.Index == -1 || nextPoint.Index <= goalPoint.Index ? 1 : -1;
                if (nextPoint.Index > 20) point.Index = -1;
            }
        }
    }

    public override string ToString()
    {
        return "Marker(Point: " + point + ")";
    }
}

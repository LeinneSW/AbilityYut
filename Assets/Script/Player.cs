using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public enum Ability
    {
        NONE = -1,
        CANNOT_MORE_THROW,
        CHANGE_YUT_RESULT,
        MAKE_ONE_CHANCE
    }

    public int index;
    public string gameName;
    public Ability ability;
    public GameObject attackParticle;

    public List<Marker> _markers = new();
    public List<GameObject> markers = new();
    public List<Yut.Result> results = new();

    public Text _nameText, _yutListText;

    private void Start()
    {
        foreach (var marker in markers)
        {
            var m = marker.GetComponent<Marker>();
            m.owner = this;
            _markers.Add(m);
        }
        ability = (Ability)Random.Range(0, 3);
    }

    private void Update()
    {
        if (GameManager.Instance.turnState == GameManager.TurnState.INTRO) return;

        _nameText.text = $"{gameName}\n능력: {ability.ToFriendlyString()}";
        if (GameManager.Instance.currentTurnIndex == index)
        {
            _nameText.text += "\n현재 차례";
            if (GameManager.Instance.turnState == GameManager.TurnState.SET_ORDER)
            {
                _yutListText.text = "순서 정하기";
                return;
            }

            var list = new int[6];
            foreach (var item in results) ++list[(int)item + 1];
            var str = "";
            for (int i = 0; i < 6; ++i)
            {
                var r = list[i];
                if (r > 0)
                {
                    if (str != "") str += ", ";
                    str += ((Yut.Result)(i - 1)).ToFriendlyString();
                    if (r > 1) str += $"x{r}";
                }
            }
            _yutListText.text = str;
        }

        if (
            Input.GetMouseButtonUp(0) &&
            GameManager.Instance.CanMoveMarker(this))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit);

            int index;
            var obj = hit.transform?.gameObject;
            if (obj != null && (index = markers.IndexOf(obj)) >= 0)
            {
                obj.GetComponent<Marker>().OnClick(index, results.Distinct().ToArray());
            }
        }
    }

    public bool AddYutResult(ref Yut.Result result, out Ability ability)
    {
        ability = Ability.NONE;
        GameManager.Instance.turnState = GameManager.TurnState.TRY_THROW_YUT_OR_SELECT_MARKER;
        switch (result)
        {
            case Yut.Result.NAK:
                return false;
            case Yut.Result.BACK_DO:
                foreach (var marker in _markers)
                {
                    if (marker.point.Index != 0)
                    {
                        results.Add(result);
                        return false;
                    }
                }
                if (results.Count > 0)
                {
                    results.Add(result);
                }
                return false;
            case Yut.Result.MO:
            case Yut.Result.YUT:
                var player = GameManager.Instance.GetOtherPlayer(this);
                if (player.ability == Ability.CHANGE_YUT_RESULT)
                {
                    if (Random.Range(0, 100) < 20)
                    {
                        ability = player.ability;
                        results.Add(result = (Yut.Result)Random.Range(1, 4));
                        return false;
                    }
                }
                results.Add(result);
                return true;
            default:
                results.Add(result);
                if (this.ability == Ability.MAKE_ONE_CHANCE)
                {
                    if (Random.Range(0, 100) < 10)
                    {
                        ability = this.ability;
                        return true;
                    }
                }
                return false;
        }
    }

    public Marker GetMarker(Marker.Point point)
    {
        var index = 0;
        foreach (var marker in _markers)
        {
            if (marker.point == point)
            {
                if(marker.carryMarker != null) return marker.carryMarker;
                return marker;
            }
            ++index;
        }
        return null;
    }

    public void Attack(Marker marker, Yut.Result result)
    {
        var index = markers.IndexOf(marker.gameObject);
        if (index == -1) return;
        var effect = true;
        if (result != Yut.Result.MO && result != Yut.Result.YUT)
        {
            if (ability == Ability.CANNOT_MORE_THROW && Random.Range(0, 100) < 85)
            {
                effect = false;
                Instantiate(attackParticle, marker.point.GetBoardPosition() + new Vector3(0, 1, 0), Quaternion.identity);
                GameManager.Instance.UseAbility(this, ability);
            }
            else
            {
                ++GameManager.Instance.canThrowCount;
            }
        }
        marker.ResetPoint(effect);
    }

    public void ThrowYut()
    {
        StartCoroutine(GameManager.Instance.ThrowYut(index));
    }
}

public static class AbilityExtensions
{
    public static string ToFriendlyString(this Player.Ability ability)
    {
        switch (ability)
        {
            case Player.Ability.CANNOT_MORE_THROW:
                return "더 못던지지롱~";
            case Player.Ability.CHANGE_YUT_RESULT:
                return "네 윷,모가 탐나는걸?";
            default:
                return "한번 더 할게요!";
        }
    }
}

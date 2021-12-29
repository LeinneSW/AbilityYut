using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            return _instance;
        }
    }

    public enum TurnState
    {
        INTRO,
        SET_ORDER,
        TRY_THROW_YUT_OR_SELECT_MARKER,
        THROWING_YUT,
        ANIMATING_MARKER,
        USE_ABILITY,
        END_GAME,
        PAUSE_GAME
    }

    private Player[] players;

    public TurnState turnState;
    public TurnState pauseBeforeState;
    public float beforeTimeScale;

    public int canThrowCount = 1;
    public int currentTurnIndex = 0;
    public bool paused = false;

    public InputField player1Name, player2Name;
    public GameObject yutObejct, yutObjectWithBackDo, markerMoveGuide, goalButton;
    public GameObject yutResultText, throwYutButton, winGroup, abilityGroup, warningText;
    public GameObject gameUICanvas, gameIntroCanvas;

    public AudioSource[] yutResultSound = new AudioSource[7];
    public AudioSource gameBackgroundMusic, useAbilitySound, startGameSound;

    private Button _throwYutButton;
    private Text _winText, _abilityText, _yutResultText;
    private readonly int[] checkOrder = new int[2] { 0, 0 };

    private void Start()
    {
        Screen.SetResolution(1920, 1080, true);
        Time.timeScale = 1;
        player1Name.characterLimit = 8;
        player1Name.onValueChanged.AddListener((word) => player1Name.text = Regex.Replace(word, @"[^0-9a-zA-Zㄱ-ㅎㅏ-ㅣ가-힣]", ""));
        player2Name.characterLimit = 8;
        player2Name.onValueChanged.AddListener((word) => player2Name.text = Regex.Replace(word, @"[^0-9a-zA-Zㄱ-ㅎㅏ-ㅣ가-힣]", ""));

        players = new Player[]
        {
            GameObject.Find("Player1").GetComponent<Player>(),
            GameObject.Find("Player2").GetComponent<Player>()
        };

        _winText = winGroup.GetComponentInChildren<Text>();
        _yutResultText = yutResultText.GetComponent<Text>();
        _throwYutButton = throwYutButton.GetComponent<Button>();
        _abilityText = abilityGroup.GetComponentInChildren<Text>();
        _throwYutButton.onClick.AddListener(() => players[0].ThrowYut());
    }

    public void Restart()
    {
        SceneManager.LoadScene("Main");
    }

    public void StartGame()
    {
        var player1 = player1Name.text;
        var player2 = player2Name.text;
        if (player1.Length == 0 || player2.Length == 0)
        {
            warningText.SetActive(true);
            return;
        }
        players[0].gameName = player1;
        players[1].gameName = player2;
        turnState = TurnState.SET_ORDER;
        gameUICanvas.SetActive(true);
        gameIntroCanvas.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PauseButton_OnClick()
    {
        paused = !paused;
        _winText.text = "일시정지";
        winGroup.SetActive(paused);
        if (paused)
        {
            pauseBeforeState = turnState;
            beforeTimeScale = Time.timeScale;

            Time.timeScale = 0;
            turnState = TurnState.PAUSE_GAME;
        }
        else
        {
            turnState = pauseBeforeState;
            Time.timeScale = beforeTimeScale;
        }
    }

    private void Update()
    {
        var buttonActive = false;
        switch (turnState)
        {
            case TurnState.SET_ORDER:
                buttonActive = true;
                break;
            case TurnState.TRY_THROW_YUT_OR_SELECT_MARKER:
                buttonActive = canThrowCount > 0;
                if (!buttonActive && players[currentTurnIndex].results.Count == 0)
                {
                    NextTurn();
                }
                break;
        }
        if (throwYutButton.activeSelf != buttonActive) throwYutButton.SetActive(buttonActive);

        if (winGroup.activeSelf) return;
        foreach (var player in players)
        {
            if (player.markers.Count <= 0)
            {
                turnState = TurnState.END_GAME;
                winGroup.SetActive(true);
                _winText.text = $"{player.gameName}님 우승!";
                Time.timeScale = 0;
            }
        }
    }

    public Player GetOtherPlayer(Player player) => players[(player.index + 1) % 2];

    public bool CanMoveMarker(Player player) => currentTurnIndex == player.index &&
            turnState == TurnState.TRY_THROW_YUT_OR_SELECT_MARKER;

    public void NextTurn()
    {
        canThrowCount = 1;
        currentTurnIndex = (currentTurnIndex + 1) % 2;
        turnState = TurnState.TRY_THROW_YUT_OR_SELECT_MARKER;
        _throwYutButton.onClick.RemoveAllListeners();
        _throwYutButton.onClick.AddListener(() => players[currentTurnIndex].ThrowYut());
    }

    public Marker GetMarker(Marker.Point point, out Player owner)
    {
        foreach (var player in players)
        {
            var marker = player.GetMarker(point);
            if (marker != null)
            {
                owner = player;
                return marker;
            }
        }
        owner = null;
        return null;
    }

    public void UseAbility(Player player, Player.Ability ability, Yut.Result result = Yut.Result.NAK)
    {
        StartCoroutine(UseAbilityCorutine(player, ability, result));
    }

    public IEnumerator UseAbilityCorutine(Player player, Player.Ability ability, Yut.Result result = Yut.Result.NAK)
    {
        turnState = TurnState.USE_ABILITY;
        abilityGroup.SetActive(true);
        _abilityText.text = $"{player.gameName}님의 능력 발동!\n";
        switch (ability)
        {
            case Player.Ability.MAKE_ONE_CHANCE:
                _abilityText.text += ability.ToFriendlyString();
                break;
            case Player.Ability.CHANGE_YUT_RESULT:
                _abilityText.text += result.ToFriendlyString();
                break;
        }
        useAbilitySound.Play();
        yield return new WaitForSeconds(1.2f);
        turnState = TurnState.TRY_THROW_YUT_OR_SELECT_MARKER;
    }

    public IEnumerator ThrowYut(int index)
    {
        if (turnState == TurnState.SET_ORDER && currentTurnIndex == index)
        {
            Time.timeScale = 2;
            turnState = TurnState.THROWING_YUT;
            var yut = new List<Yut>();
            for (var i = 0; i < 2; ++i)
            {
                var obj = Instantiate(
                    yutObejct,
                    new Vector3(Random.Range(-1f, 1), 3f, Random.Range(-1f, 1)),
                    Random.rotation
                );
                var cyut = obj.GetComponent<Yut>();
                cyut.Rigid.AddForce(new(Random.Range(-120, 120f), 200, Random.Range(-120, 120f)));
                yut.Add(cyut);
            }
            yield return new WaitForSeconds(0.2f);

            while (true)
            {
                var count = 0;
                foreach (var obj in yut) count += obj.stickResult != Yut.Stick.NONE ? 1 : 0;
                if (count == 2) break;
                yield return new WaitForSeconds(0.05f);
            }

            int stick = 0;
            foreach (var obj in yut)
            {
                stick += (int)obj.stickResult;
                Destroy(obj.gameObject, 1);
            }
            Time.timeScale = 1;
            checkOrder[index] = stick == 0 ? 3 : stick;

            var setOrder = false;
            if (checkOrder[0] > 0 && checkOrder[1] > 0)
            {
                setOrder = true;
                currentTurnIndex = checkOrder[0] >= checkOrder[1] ? 0 : 1;

                startGameSound.Play();
                yutResultText.SetActive(true);
                _yutResultText.text = players[currentTurnIndex].gameName + "님 부터 시작합니다.";
            }
            else
            {
                currentTurnIndex = 1;
            }
            _throwYutButton.onClick.RemoveAllListeners();
            _throwYutButton.onClick.AddListener(() => players[currentTurnIndex].ThrowYut());
            yield return new WaitForSeconds(1.1f);

            if (setOrder) turnState = TurnState.TRY_THROW_YUT_OR_SELECT_MARKER;
            else turnState = TurnState.SET_ORDER;
            yield break;
        }

        if (canThrowCount <= 0 ||
            index != currentTurnIndex ||
            turnState != TurnState.TRY_THROW_YUT_OR_SELECT_MARKER) yield break;

        Time.timeScale = 1.7f;
        turnState = TurnState.THROWING_YUT;
        --canThrowCount;

        var backDoIndex = Random.Range(0, 4);
        var list = new List<Yut>();
        for (var i = 0; i < 4; ++i)
        {
            var yut = Instantiate(
                backDoIndex == i ? yutObjectWithBackDo : yutObejct,
                new Vector3(Random.Range(-1.25f, 1.25f), 2.9f, Random.Range(-1.25f, 1.25f)),
                Random.rotation
            );
            var cyut = yut.GetComponent<Yut>();
            cyut.Rigid.AddForce(new(Random.Range(-220, 220f), 350, Random.Range(-220, 220f)));
            list.Add(cyut);
        }
        yield return new WaitForSeconds(0.2f);

        var isBack = false;
        var result = Yut.Result.MO;
        while (true)
        {
            var count = 0;
            foreach (var yut in list)
            {
                if (yut == null)
                {
                    result = Yut.Result.NAK;
                    break;
                }
                count += yut.stickResult != Yut.Stick.NONE ? 1 : 0;
            }
            if (result == Yut.Result.NAK || count == 4) break;
            yield return new WaitForSeconds(0.05f);
        }

        for (int i = 0; i < 4; ++i)
        {
            var yut = list[i];
            if (yut == null) continue;

            yut.Rigid.isKinematic = true;
            Destroy(yut.gameObject, 1f);
            if (result == Yut.Result.NAK) continue;

            result += (int)yut.stickResult;
            if (yut.isBackMark && yut.stickResult == Yut.Stick.BACK)
            {
                isBack = true;
            }

            if (i == 3 && isBack && result == Yut.Result.DO)
            {
                result = Yut.Result.BACK_DO;
            }
        }
        Time.timeScale = 1;
        yutResultText.SetActive(true);
        _yutResultText.text = $"{result.ToFriendlyString()}!";
        yutResultSound[(int)result + 2].Play();
        yield return new WaitForSeconds(1.05f);

        if (players[index].AddYutResult(ref result, out Player.Ability ability))
        {
            ++canThrowCount;
        }

        if (ability != Player.Ability.NONE)
        {
            UseAbility(players[index], ability, result);
        }
    }
}

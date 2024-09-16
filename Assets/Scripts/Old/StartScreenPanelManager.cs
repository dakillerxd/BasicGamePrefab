using UnityEngine;
using UnityEngine.UI;

public class StartScreenPanelManager : MonoBehaviour
{


    [Header("Panels")]
    [SerializeField] private GameObject panelTips;
    [SerializeField] private GameObject panelEasy;
    [SerializeField] private GameObject panelNormal;
    [SerializeField] private GameObject panelHard;

    [Header("Buttons")]
    [SerializeField] private Button buttonEasy;
    [SerializeField] private Button buttonNormal;
    [SerializeField] private Button buttonHard;
    [SerializeField] private Button buttonBack;


    private void Start()
    {
        SetupDifficultyButtons();

        panelTips.SetActive(true);
        panelEasy.SetActive(false);
        panelNormal.SetActive(false);
        panelHard.SetActive(false);
    }


    private void SetupDifficultyButtons()
    {
        if (buttonEasy != null)
        {
            // buttonEasy.onClick.RemoveAllListeners();
            buttonEasy.onClick.AddListener(() => SoundManager.Instance?.PlaySound("ButtonClick"));
            buttonEasy.onClick.AddListener(() => SetDifficulty(GameDifficulty.Easy));
        }

        if (buttonNormal != null)
        {
            // buttonNormal.onClick.RemoveAllListeners();
            buttonEasy.onClick.AddListener(() => SoundManager.Instance?.PlaySound("ButtonClick"));
            buttonNormal.onClick.AddListener(() => SetDifficulty(GameDifficulty.Normal));
        }

        if (buttonHard != null)
        {
            // buttonHard.onClick.RemoveAllListeners();
            buttonEasy.onClick.AddListener(() => SoundManager.Instance?.PlaySound("ButtonClick"));
            buttonHard.onClick.AddListener(() => SetDifficulty(GameDifficulty.Hard));
        }

        if (buttonBack != null)
        {
            // buttonBack.onClick.RemoveAllListeners();
            buttonEasy.onClick.AddListener(() => SoundManager.Instance?.PlaySound("ButtonClick"));
        }

    }



    public void OnEasyButtonEnter()
    {
        ShowPanel(panelEasy);
    }

    public void OnNormalButtonEnter()
    {
        ShowPanel(panelNormal);
    }

    public void OnHardButtonEnter()
    {
        ShowPanel(panelHard);
    }

    public void OnButtonExit()
    {
        ShowPanel(panelTips);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        panelTips.SetActive(false);
        panelEasy.SetActive(false);
        panelNormal.SetActive(false);
        panelHard.SetActive(false);

        panelToShow.SetActive(true);
    }


    private void SetDifficulty(GameDifficulty difficulty)
    {
        GameManager.Instance.chosenGameDifficulty = difficulty;
    }
}

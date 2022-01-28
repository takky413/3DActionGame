using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{
    //どのキャラクターを選択したかをSelectCharacter.csから参照するためのフラグ
    public static bool isSelectEkard = true; //とりあえずデフォルトはEkardにしておく
    public static bool isSelectAvelyn = false; //とりあえずデフォルトはEkardにしておく

    //キャラ選択画面で登場させるキャラクターモデル
    [SerializeField] GameObject Ekard = null;
    [SerializeField] GameObject Avelyn = null;

    //どのCanvasを表示しているか（Oculusからの入力を場合分けするために用いる）
    public static bool isDisplayTitleCnavas = true;
    public static bool isDisplaySelectModeCanvas = false;

    //Canvas
    [SerializeField] GameObject TitleCanvas = null;
    [SerializeField] GameObject SelectModeCanvas = null;

    //どのモードを選択したかをSelectCharacter.csから参照するためのフラグ
    public static bool isSelectPracticeMode = false; //とりあえずデフォルト
    public static bool isSelectBattleMode = true;
    public static bool isSelectCollaborateMode = false;

    //モード選択画面で表示する矢印
    [SerializeField] GameObject PracticeModeArrow = null;
    [SerializeField] GameObject BattleModeArrow = null;
    [SerializeField] GameObject CollaborateModeArrow = null;

    //ボタン選択音
    private AudioSource ButtonClickSE1;
    private AudioSource ButtonClickSE2;
    private AudioSource ButtonClickSE3;

    void Start()
    {
        //タイトルに戻るを押した時の初期化
        isSelectEkard = true;
        isSelectAvelyn = false;

        isDisplayTitleCnavas = true;
        isDisplaySelectModeCanvas = false;

        isSelectPracticeMode = false;
        isSelectBattleMode = true;
        isSelectCollaborateMode = false;

        AudioSource[] audioSources = GetComponents<AudioSource>();
        ButtonClickSE1 = audioSources[0];
        ButtonClickSE2 = audioSources[1];
        ButtonClickSE3 = audioSources[2];
    }


    private void Update()
    {
        //Oculusコントローラによる操作の処理
        if (isDisplayTitleCnavas == true) {

            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                //OnScreenTap();
                OnGoNextButtonClicked();
            }
            if (OVRInput.GetDown(OVRInput.Button.Three))
            {
                OnSelectEkardButtonClicked();
            }
            if (OVRInput.GetDown(OVRInput.Button.Four))
            {
                OnSelectAvelynButtonClicked();
            }
        }

        else if (isDisplaySelectModeCanvas == true)
        {

            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                OnScreenTap();
            }
            if (OVRInput.GetDown(OVRInput.Button.Two))
            {
                OnSelectPracticeButtonClicked();
            }
            if (OVRInput.GetDown(OVRInput.Button.Three))
            {
                OnSelectBattleButtonClicked();
            }
            if (OVRInput.GetDown(OVRInput.Button.Four))
            {
                OnSelectCollaborateButtonClicked();
            }
        }
    }

    // --------------------------------------------------------
    // 画面タップコールバック
    // --------------------------------------------------------
    public void OnScreenTap()
    {
        //ButtonClickSE3.PlayOneShot(ButtonClickSE3.clip);

        if (isSelectPracticeMode)
        {
            ButtonClickSE3.PlayOneShot(ButtonClickSE3.clip);
            SceneManager.LoadScene("PracticeScene");
        }
        if (isSelectBattleMode)
        {
            ButtonClickSE3.PlayOneShot(ButtonClickSE3.clip);
            SceneManager.LoadScene("BattleScene");
        }
        if (isSelectCollaborateMode)
        {
            ButtonClickSE3.PlayOneShot(ButtonClickSE3.clip);
            SceneManager.LoadScene("CollaborateScene");
        }
    }


    // --------------------------------------------------------
    // モード選択画面へ移行ボタンクリックコールバック
    // --------------------------------------------------------
    public void OnGoNextButtonClicked()
    {
        ButtonClickSE2.PlayOneShot(ButtonClickSE2.clip);

        TitleCanvas.gameObject.SetActive(false);
        SelectModeCanvas.gameObject.SetActive(true);

        isDisplayTitleCnavas = false;
        isDisplaySelectModeCanvas = true;

        Ekard.gameObject.SetActive(false);
        Avelyn.gameObject.SetActive(false);
    }


    // ---------------------------------------------------------------------
    // Ekard選択ボタンクリックコールバック
    // ---------------------------------------------------------------------
    public void OnSelectEkardButtonClicked()
    {
        ButtonClickSE1.PlayOneShot(ButtonClickSE1.clip);

        isSelectEkard = true;
        isSelectAvelyn = false;

        Ekard.gameObject.SetActive(true);
        Avelyn.gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------------
    // Avelyn選択ボタンクリックコールバック
    // ---------------------------------------------------------------------
    public void OnSelectAvelynButtonClicked()
    {
        ButtonClickSE1.PlayOneShot(ButtonClickSE1.clip);

        isSelectEkard = false;
        isSelectAvelyn = true;

        Ekard.gameObject.SetActive(false);
        Avelyn.gameObject.SetActive(true);
    }


    // ---------------------------------------------------------------------
    // 練習モード選択ボタンクリックコールバック
    // ---------------------------------------------------------------------
    public void OnSelectPracticeButtonClicked()
    {
        ButtonClickSE1.PlayOneShot(ButtonClickSE1.clip);

        isSelectPracticeMode = true;
        isSelectBattleMode = false;
        isSelectCollaborateMode = false;

        PracticeModeArrow.gameObject.SetActive(true);
        BattleModeArrow.gameObject.SetActive(false);
        CollaborateModeArrow.gameObject.SetActive(false);
        Debug.Log("PracticeButtonClicked");
    }


    // ---------------------------------------------------------------------
    // 対戦モード選択ボタンクリックコールバック
    // ---------------------------------------------------------------------
    public void OnSelectBattleButtonClicked()
    {
        ButtonClickSE1.PlayOneShot(ButtonClickSE1.clip);

        isSelectPracticeMode = false;
        isSelectBattleMode = true;
        isSelectCollaborateMode = false;

        PracticeModeArrow.gameObject.SetActive(false);
        BattleModeArrow.gameObject.SetActive(true);
        CollaborateModeArrow.gameObject.SetActive(false);
        Debug.Log("BattleButtonClicked");
    }


    // ---------------------------------------------------------------------
    // 協力モード選択ボタンクリックコールバック
    // ---------------------------------------------------------------------
    public void OnSelectCollaborateButtonClicked()
    {
        ButtonClickSE1.PlayOneShot(ButtonClickSE1.clip);

        isSelectPracticeMode = false;
        isSelectBattleMode = false;
        isSelectCollaborateMode = true;

        PracticeModeArrow.gameObject.SetActive(false);
        BattleModeArrow.gameObject.SetActive(false);
        CollaborateModeArrow.gameObject.SetActive(true);
        Debug.Log("CollaboreteButtonClicked");
    }

}
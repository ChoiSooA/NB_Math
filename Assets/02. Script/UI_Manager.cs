using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LeiaUnity;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance { get; private set; }

    LeiaDisplay display;
    public Button BT_Option;
    public Button BT_Exit;
    public Button BT_Exit_2;
    public Button BT_Restart;
    public Button BT_Close;
    public Toggle BT_3D;
    public Toggle BT_BGM_Sound;
    public Slider Slider_BGM_Sound;

    public DoTEffect Setting_Effect;
    Audio_Manager audioManager;

    public GameObject Option_Panel;

    bool is3D = false;
    bool isMute = false;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        audioManager = FindAnyObjectByType<Audio_Manager>();

        audioManager.Current_BGM_Volume = Slider_BGM_Sound.value;
        BT_Option.onClick.AddListener(() =>
        {
            Option_Panel.SetActive(true);
        });
        BT_Exit.onClick.AddListener(() =>
        {
            Application.Quit();
        });
        BT_Exit_2.onClick.AddListener(() =>
        {
            Application.Quit();
        });
        BT_Restart.onClick.AddListener(() =>
        {
            Restart();
        });
        BT_Close.onClick.AddListener(() =>
        {
            Setting_Effect.Close();
        });
        BT_3D.onValueChanged.AddListener((bool value) =>
        {
            OnOff3D();
        });
        BT_BGM_Sound.onValueChanged.AddListener((bool value) =>
        {
            OnOffBGM();
        });
        Slider_BGM_Sound.onValueChanged.AddListener((float value) =>
        {
            SetBgmVolume();
        });
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnOff3D()
    {
        is3D = !is3D;
#if UNITY_EDITOR
        Debug.Log("3D¸ðµå : " + is3D);
#endif
        display = FindAnyObjectByType<LeiaDisplay>();
        if (display != null)
        {
            display.Set3DMode(is3D);
        }
    }

    private void OnOffBGM()
    {
        isMute = !isMute;
        audioManager.SetMute(isMute);
        if (!isMute)
        {
            Slider_BGM_Sound.value = audioManager.Current_BGM_Volume;
        }
        else
        {
            Slider_BGM_Sound.value = 0;
        }
    }

    void SetBgmVolume()
    {
        Audio_Manager.Instance.SetBGMVolume(Slider_BGM_Sound.value);
        if (Slider_BGM_Sound.value != 0)
        {
            BT_BGM_Sound.isOn = false;
        }
        else
        {
            BT_BGM_Sound.isOn = true;
        }
    }
}

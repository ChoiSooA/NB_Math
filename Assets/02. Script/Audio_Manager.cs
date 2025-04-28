using UnityEngine;

public class Audio_Manager : MonoBehaviour
{
    public AudioSource BGM_audioSource;
    public AudioSource Effect_audioSource;
    public AudioSource Ment_audioSource;
    public AudioClip Bgm_Clip;
    public AudioClip[] Effect_Clip;     // 버튼 0, 맞음 1, 틀림 2, 박수 3, 성공 4

    public float Current_BGM_Volume;

    public static Audio_Manager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (BGM_audioSource == null)
        {
            BGM_audioSource = transform.GetChild(0).gameObject.AddComponent<AudioSource>();
        }
        else
        {
            BGM_audioSource = transform.GetChild(0).gameObject.GetComponent<AudioSource>();
        }

        if (Effect_audioSource == null)
        {
            Effect_audioSource = transform.GetChild(1).gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Effect_audioSource = transform.GetChild(1).gameObject.GetComponent<AudioSource>();
        }

        if (Ment_audioSource == null)
        {
            Ment_audioSource = transform.GetChild(2).gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Ment_audioSource = transform.GetChild(2).gameObject.GetComponent<AudioSource>();
        }

        if (Bgm_Clip != null&&BGM_audioSource!=null)
        {
            BGM_audioSource.clip = Bgm_Clip;
            BGM_audioSource.loop = true;
            BGM_audioSource.Play();
            BGM_audioSource.volume = Current_BGM_Volume;
        }
    }

    public void SetMute(bool isMute) // isMute이 true면 off, false면 on
    {
        if (isMute)
        {
            Current_BGM_Volume = BGM_audioSource.volume;
            if (BGM_audioSource.volume != 0)
            {
                BGM_audioSource.volume = 0;
            }
        }
        else
        {
            BGM_audioSource.volume = Current_BGM_Volume;
        }
    }
    public void SetBGMVolume(float volume)
    {
        BGM_audioSource.volume = volume;
    }

    /// <summary>
    /// 버튼 0, 맞음 1, 틀림 2, 박수 3, 성공 4
    /// </summary>
    public void PlayEffect(int index)
    {
        Effect_audioSource.PlayOneShot(Effect_Clip[index]);
    }

    public void PlayMent(AudioClip clip)
    {
        Ment_audioSource.Stop(); // 현재 재생 중인 사운드 정지
        Ment_audioSource.clip = clip;
        Ment_audioSource.Play();
    }

}

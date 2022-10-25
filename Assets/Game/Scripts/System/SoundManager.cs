using UnityEngine;
using System.Collections;

public class SoundManager : Singleton<SoundManager> 
{
	private static AudioSource musicAudio;
	private AudioSource soundFx;

	public static bool soundEnable
	{
		get
		{
			if (PlayerPrefs.GetInt("sound", 1) == 0) return false;
			else return true;
		}
		set
		{
			if (value) PlayerPrefs.SetInt("sound", 1);
			else PlayerPrefs.SetInt("sound", 0);
		}
	}

	public static bool musicEnable
	{
		get
		{
			if (PlayerPrefs.GetInt("music", 1) == 0) return false;
			else return true;
		}
		set
		{
			if (value)
			{
				PlayerPrefs.SetInt("music", 1);
				musicAudio.volume = 0.5f;
			}
			else 
			{
				PlayerPrefs.SetInt("music", 0);
				musicAudio.volume = 0;
			} 
		}
	}
    private void Start()
    {
		musicAudio = gameObject.AddComponent<AudioSource>();
		musicAudio.loop = true;
		if (musicEnable) musicAudio.volume = 0.5f;
		else musicAudio.volume = 0f;
		soundFx = gameObject.AddComponent<AudioSource>();
	}

    public static void PlaySfx(AudioClip clip)
    {
        if (!soundEnable)
            return;
        Instance.PlaySound(clip, Instance.soundFx);
    }

    public static void PlaySfx(AudioClip clip, float volume){
		if (!soundEnable)
			return;
		Instance.PlaySound(clip, Instance.soundFx, volume);
	}

	public static void PlayMusic(AudioClip clip){
		Instance.PlaySound (clip, musicAudio);
	}

	public static void PlayMusic(AudioClip clip, float volume){
		Instance.PlaySound (clip, musicAudio, volume);
	}

	private void PlaySound(AudioClip clip,AudioSource audioOut){
		if (!soundEnable)
			return;
		if (clip == null) {
			return;
		}
		if (audioOut == musicAudio) {
			audioOut.clip = clip;
			audioOut.Play ();
		} else
			audioOut.PlayOneShot (clip, 0.5f);
	}

	private void PlaySound(AudioClip clip,AudioSource audioOut, float volume){
		if (!soundEnable)
			return;
		if (clip == null) {
			return;
		}

		if (audioOut == musicAudio) {
			audioOut.clip = clip;
			audioOut.Play ();
		} else
			audioOut.PlayOneShot (clip, 0.5f * volume);
	}
}

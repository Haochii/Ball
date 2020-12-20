using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*	调用：AudioManager.Instance.playMusic(AudioManager.Instance.aim);*/

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip background;
    public AudioClip aim;
    public AudioClip breakWall;
    public AudioClip buffCal;
    public AudioClip chooseBall;
    public AudioClip collider1;
    public AudioClip collider2;
    public AudioClip collider3;
    public AudioClip collider4;
    public AudioClip die;
    public AudioClip hurt1;
    public AudioClip hurt2;
    public AudioClip hurt3;
    public AudioClip hurt4;
    public AudioClip qi;
    public AudioClip shoot;
    public AudioClip skill;
    public AudioClip start;
    public AudioClip wallRelive;
    public AudioClip lose;
    public AudioClip win;

    private AudioSource _audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        _audioSource = gameObject.GetComponent<AudioSource>();
        _audioSource.loop = true;
        _audioSource.volume = 1.0f;
        _audioSource.Play();
    }
    public void playMusic(AudioClip play)
    {
        _audioSource.PlayOneShot(play);
    }
}
using UnityEngine;

public class MusicJukebox : MonoBehaviour
{
    [Header("Components")]
    public AudioSource audioSource; // 소리가 날 오디오 소스 (CD 플레이어)

    [Header("Music Clips")]
    public AudioClip song1;         // 노래 1 파일
    public AudioClip song2;         // 노래 2 파일

    // Music 1 버튼이 누를 때 실행할 함수
    public void PlayTrack1()
    {
        if (audioSource.clip != song1 || !audioSource.isPlaying) // 이미 재생 중이면 무시
        {
            audioSource.clip = song1;
            audioSource.Play();
        }
    }

    // Music 2 버튼이 누를 때 실행할 함수
    public void PlayTrack2()
    {
        if (audioSource.clip != song2 || !audioSource.isPlaying)
        {
            audioSource.clip = song2;
            audioSource.Play();
        }
    }

    // No music 버튼이 누를 때 실행할 함수
    public void StopMusic()
    {
        audioSource.Stop();
    }
}
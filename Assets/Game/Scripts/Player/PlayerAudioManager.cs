using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource footstepSfx;
    [SerializeField] private AudioSource landingSfx;
    [SerializeField] private AudioSource glideSfx;
    [SerializeField] private AudioSource punchSfx;

    private void PlayFootstepSfx()
    {
        footstepSfx.volume = Random.Range(0.8f, 1f);
        footstepSfx.pitch = Random.Range(0.8f, 1.5f);
        footstepSfx.Play();
    }

    private void PlayLandingSfx()
    {
        landingSfx.Play();
    }

    public void PlayGlideSfx()
    {
        glideSfx.Play();
    }

    public void StopGlideSfx()
    {
        glideSfx.Stop();
    }

    private void PlayPunchSfx()
    {
        punchSfx.volume = Random.Range(0.8f, 1f);
        punchSfx.pitch = Random.Range(0.8f, 1.5f);
        punchSfx.Play();
    }
}

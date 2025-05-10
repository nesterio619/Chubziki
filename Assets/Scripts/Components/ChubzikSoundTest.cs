using UnityEngine;
using MelenitasDev.SoundsGood;

public class ChubzikSoundTest : MonoBehaviour
{
    private Sound _punchSound;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        _punchSound = new Sound(SFX.chubzikPunch);
        _punchSound.SetFollowTarget(transform);
    }


    public void PunchSoundPlay()
    {
        _punchSound.Play();
    }
}

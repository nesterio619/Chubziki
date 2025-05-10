using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BF_GroundAssetManager : MonoBehaviour
{
    public GameObject UIText;
    public int showcaseIndex = 0;
    [HideInInspector] public int subShowcaseIndex = 0;
    public List<GameObject> showcasesGO;
    public List<GameObject> cameras;
    public List<GameObject> lights;
    public List<Material> skyboxes;
    public List<GameObject> CursorTips;
    [Space]
    public GameObject specialCamera;
    public GameObject specialButton;
    public GameObject specialInfo;
    public Transform cameraFollowTr;
    public Transform cameraTr;
    public BF_DisplayFPS fpsDisplay;
    private int maxIndex = 4;
    [HideInInspector] public int maxSubIndex = 3;

    [HideInInspector] public UnityEvent m_ShowcaseChange = new UnityEvent();
    private bool isFollowing = false;
    private Vector3 oldPos;
    private Quaternion oldRot;
    public Image showcasePanel;
    public List<Button> showcaseButtons;
    public List<Color> showcaseColors;
    public GameObject camBis;

    // Start is called before the first frame update
    void Start()
    {
        maxIndex = showcasesGO.Count - 1;
        SwitchShowcase(0);
        SwitchSubShowcase(0);
        //RenderSettings.fog = true;
        //RenderSettings.fogDensity = 0.00f;
        UIText.SetActive(false);
        specialCamera.SetActive(false);
        specialButton.SetActive(true);
        specialInfo.SetActive(true);
        oldPos = cameraTr.position;
        oldRot = cameraTr.rotation;
    }

    public void SwitchShowcase(int addIndex)
    {
        for (int i = 0; i <= maxIndex; i++)
        {
            showcasesGO[i].SetActive(false);
            cameras[i].SetActive(false);
            lights[i].SetActive(false);
            CursorTips[i].SetActive(false);
        }
        showcaseIndex += addIndex;
        if (showcaseIndex <= -1)
        {
            showcaseIndex = maxIndex;
        }
        else if (showcaseIndex == maxIndex + 1)
        {
            showcaseIndex = 0;
        }
        showcasesGO[showcaseIndex].SetActive(true);
        cameras[showcaseIndex].SetActive(true);
        lights[showcaseIndex].SetActive(true);
        CursorTips[showcaseIndex].SetActive(true);
        RenderSettings.skybox = skyboxes[showcaseIndex];
        subShowcaseIndex = 0;
        m_ShowcaseChange.Invoke();

        
        if (showcaseIndex != 0)
        {
            specialCamera.SetActive(false);
            specialButton.SetActive(false);
            specialInfo.SetActive(false);
        }
        else
        {
            specialCamera.SetActive(false);
            specialButton.SetActive(true);
            specialInfo.SetActive(true);
        }
        if (showcaseIndex == 2 && (subShowcaseIndex == 1 || subShowcaseIndex == 2))
            fpsDisplay.enabled = true;
        else
            fpsDisplay.enabled = false;

        if(showcaseIndex == 3 || showcaseIndex == 4)
        {
            showcasePanel.color = showcaseColors[1];
            foreach (Button button in showcaseButtons)
            {
                ColorBlock colorBlock = button.colors;
                colorBlock.normalColor = showcaseColors[1];
                button.colors = colorBlock;
            }
        }
        else
        {
            showcasePanel.color = showcaseColors[0];
            foreach (Button button in showcaseButtons)
            {
                ColorBlock colorBlock = button.colors;
                colorBlock.normalColor = showcaseColors[0];
                button.colors = colorBlock;
            }
        }
    }

    public void SwitchSubShowcase(int addIndex)
    {
        subShowcaseIndex += addIndex;
        if (subShowcaseIndex <= -1)
        {
            subShowcaseIndex = maxSubIndex;
        }
        else if (subShowcaseIndex == maxSubIndex + 1)
        {
            subShowcaseIndex = 0;
        }

        if (showcaseIndex == 2 && (subShowcaseIndex == 1 || subShowcaseIndex == 2))
            fpsDisplay.enabled = true;
        else
            fpsDisplay.enabled = false;

        m_ShowcaseChange.Invoke();
    }

    public void ActivateSpecialCamera()
    {
        specialCamera.SetActive(!specialCamera.activeInHierarchy);
        cameras[0].SetActive(!cameras[0].activeInHierarchy);
    }
    public void ActivateBisCamera()
    {
        camBis.SetActive(true);
        cameras[1].SetActive(false);
    }

    private void SpecialFollowCamera()
    {
        if (!isFollowing)
            isFollowing = true;
        cameraTr.position = cameraFollowTr.position + Vector3.up * 3.8f + Vector3.forward * -5f;
        cameraTr.forward = (cameraFollowTr.position - cameraTr.position + Vector3.up * 1.25f).normalized ;
    }

    private void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchSubShowcase(-1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchSubShowcase(1);
        }
#else
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SwitchSubShowcase(-1);
        }
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SwitchSubShowcase(1);
        }
#endif

        if(showcaseIndex == 1 && subShowcaseIndex == 0)
            SpecialFollowCamera();
        else if (isFollowing)
        {
            isFollowing = false;
            cameraTr.position = oldPos;
            cameraTr.rotation = oldRot;
        }

        if (showcaseIndex == 1 && subShowcaseIndex == 2)
            ActivateBisCamera();
        else if(showcaseIndex == 1 && (subShowcaseIndex == 0 || subShowcaseIndex == 1))
        {
            camBis.SetActive(false);
            cameras[1].SetActive(true);
        }
        else if(showcaseIndex != 1)
        {
            camBis.SetActive(false);
        }

    }
}
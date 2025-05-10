using System;
using System.Collections;
using Core;
using Core.SaveSystem;
using UI.Canvas;
using UnityEngine;

public class SaveInfoCanvasScreen : CanvasScreen
{
    [SerializeField] private GameObject showSaveInfo;
    [SerializeField] private float showTime = 1.5f;
    private void Awake()
    {
        Initialize();
    }

    private void ShowSaveInfoCanvas()
    {
        showSaveInfo.SetActive(true);
        StartCoroutine(HideSaveInfoAfterDelay());
    }

    private IEnumerator HideSaveInfoAfterDelay()
    {
        yield return new WaitForSeconds(showTime);
        showSaveInfo.SetActive(false);
    }

    public override void Initialize()
    {
        AddListenersToCanvasScreenButtons();
    }

    protected override void AddListenersToCanvasScreenButtons()
    {
        SaveManager.OnSaveCompleted += ShowSaveInfoCanvas;
    }

    protected override void RemoveListenersFromCanvasScreenButtons()
    {
        SaveManager.OnSaveCompleted -= ShowSaveInfoCanvas;
    }

    private void OnDestroy()
    {
        RemoveListenersFromCanvasScreenButtons();
    }
}
//Очень Древняя писанина, но рабочая :D 
//реализация инвенторя с драг системой для мобильной игры

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System;

public class InventoryCard : MonoBehaviour
{
    [SerializeField] private Image visual;
    private int itemID = -1;

    public UnityEvent cancelUse = new UnityEvent();
    private RectTransform moveItem;

    private Coroutine controlCoroutine;
    private bool itemInHand = false;

    [Header("Реплики на случай отказа")]
    [SerializeField] private string[] answers = new string[0];
    [SerializeField] private AudioClip[] audioAnswer = new AudioClip[0];

    [Header("Картинка отказа")]
    [SerializeField] private Sprite cancelImage; 

    private void Start()
    {
        if (answers.Length == 0)
        {
            Array.Resize(ref answers, 1);
            answers[0] = "Не подходит...";
        }
    }

    public void SetData(int ID, RectTransform moveItemImage)
    {
        itemID = ID;
        visual.sprite = AllItemAsset.Instance.GetItemSprite(ID);
        moveItem = moveItemImage;
    }

    public int GetID()
    {
        return itemID;
    }

    public void ActiveInput()
    {
        if (moveItem.gameObject.activeInHierarchy) return;

        if (controlCoroutine != null) StopCoroutine(controlCoroutine);
        controlCoroutine = StartCoroutine(ControlTouchPos());
    }

    private IEnumerator ControlTouchPos()
    {
#if UNITY_EDITOR
        float borderMPos = Mouse.current.position.ReadValue().y + (GetComponent<RectTransform>().sizeDelta.y / 2);

        while (Mouse.current.position.ReadValue().y < borderMPos)
            yield return new WaitForFixedUpdate();

        itemInHand = true;
        visual.gameObject.SetActive(false);
        moveItem.gameObject.SetActive(true);
        moveItem.GetComponent<Image>().sprite = AllItemAsset.Instance.GetItemSprite(itemID);

        while (itemInHand)
        {
            moveItem.transform.position = Mouse.current.position.ReadValue();
            yield return new WaitForFixedUpdate();
        }

        control = null;

#elif UNITY_ANDROID
        float borderMPos = Touchscreen.current.position.ReadValue().y + (GetComponent<RectTransform>().sizeDelta.y / 2);

        while (Touchscreen.current.position.ReadValue().y < borderMPos)
            yield return new WaitForFixedUpdate();

        itemInHand = true;
        visual.gameObject.SetActive(false);
        moveItem.gameObject.SetActive(true);
        moveItem.GetComponent<Image>().sprite = AllItemAsset.Instance.GetItemSprite(itemID);

        while (itemInHand)
        {
            moveItem.transform.position = Touchscreen.current.position.ReadValue();
            yield return new WaitForFixedUpdate();
        }

        control = null;
#endif
    }

    public void DeactiveInput()
    {
        if (itemInHand)
        {
            APIntupController.Instance.InputDeactive(this);
            itemInHand = false;
        }
        else
        if (controlCoroutine != null) 
            StopCoroutine(controlCoroutine);
    }

    public void CancelItemUse()
    {
        cancelUse.Invoke();
        visual.gameObject.SetActive(true);
        StartCoroutine(CancelUse());
    }

    private IEnumerator CancelUse()
    {
        moveItem.GetComponent<Image>().sprite = cancelImage;

        int textR = UnityEngine.Random.Range(0, answers.Length);
        int audioR = UnityEngine.Random.Range(0, audioAnswer.Length);

        if (audioAnswer.Length != 0 && audioAnswer[audioR] != null)
            APIntupController.Instance.player.gameObject.GetComponent<APPlayerController>().AnswerConnector(answers[textR], audioAnswer[audioR]);
        else
            APIntupController.Instance.player.gameObject.GetComponent<APPlayerController>().AnswerConnector(answers[textR]);

        yield return new WaitForSeconds(0.2f);
        moveItem.gameObject.SetActive(false);
    }

    public void CorrectItemUse()
    {
        moveItem.gameObject.SetActive(false);
        if (controlCoroutine != null) StopCoroutine(controlCoroutine);
    }

    public void CompliteItemUse()
    {
        moveItem.gameObject.SetActive(false);
        if (AllItemAsset.Instance.AskDeleteItemAfterUse(itemID))
            Destroy(gameObject);
        else
            StartCoroutine(ReturnItemToInventory());
    }

    private IEnumerator ReturnItemToInventory()
    {
        cancelUse.Invoke();

        yield return new WaitForSeconds(1.5f);
        visual.gameObject.SetActive(true);
    }
}

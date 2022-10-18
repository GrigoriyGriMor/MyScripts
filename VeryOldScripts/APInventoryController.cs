//ќчень ƒревн€€ писанина, но рабоча€ :D 
//реализаци€ инвентор€ с драг системой дл€ мобильной игры

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class APInventoryController : MonoBehaviour
{
    private static APInventoryController instance;
    public static APInventoryController Instance => instance;

    [Header(" уда полетит подобранный итем")]
    [SerializeField] private RectTransform inventoryPointPos;
    [SerializeField] private RectTransform moveItemImage;
    private Vector2 startSizeMoveImage;

    private List<int> itemIDs = new List<int>();

    [Header("UI Inventory")]
    [SerializeField] private RectTransform rt_Content;
    [SerializeField] private Button B_OpenInventory;
    [SerializeField] private Button B_CloseInventory;
    [SerializeField] private GameObject InventoryCardPrefab;
    [SerializeField] private Animator anim; 

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        startSizeMoveImage = moveItemImage.sizeDelta;
        moveItemImage.gameObject.SetActive(false);

        B_OpenInventory.onClick.AddListener(() =>
        {
            anim.SetBool("Open", true);

            B_CloseInventory.gameObject.SetActive(true);
            B_OpenInventory.gameObject.SetActive(false);
        });


        B_CloseInventory.onClick.AddListener(() =>
        {
            anim.SetBool("Open", false);

            B_OpenInventory.gameObject.SetActive(true);
            B_CloseInventory.gameObject.SetActive(false);
        });

        B_CloseInventory.gameObject.SetActive(false);

        int[] newArray = SaveController.Instance.LoadInventoryItems();

        rt_Content.sizeDelta = new Vector2(0, rt_Content.sizeDelta.y);
        for (int i = 0; i < newArray.Length; i++)
            itemIDs.Add(newArray[i]);

        StartCoroutine(SetContentInventory());
    }

    private IEnumerator SetContentInventory()
    {
        yield return new WaitForFixedUpdate();

        for (int i = 0; i < itemIDs.Count; i++)
            SetNewCard(itemIDs[i]);
    }

    private void SetNewCard(int id)
    {
        GameObject go = Instantiate(InventoryCardPrefab, rt_Content);
        go.GetComponent<InventoryCard>().SetData(id, moveItemImage);

        float cardWidth = go.GetComponent<RectTransform>().sizeDelta.x;

        float spacePixels = rt_Content.GetComponent<HorizontalLayoutGroup>().spacing * rt_Content.childCount;
        float contentWidth = rt_Content.childCount * cardWidth + spacePixels;

        rt_Content.sizeDelta = new Vector2(contentWidth, rt_Content.sizeDelta.y);
    }

    public int[] GetInventoryItemIDs()
    {
        int[] newArray = new int[itemIDs.Count];

        for (int i = 0; i < newArray.Length; i++)
            newArray[i] = itemIDs[i];

        return newArray;
    }

    public void SetNewItemToInventory(int itemID, Sprite sprite, Vector2 startItemPos)
    {
        StartCoroutine(SetNewItem(itemID, sprite, startItemPos));
    }

    bool setItemCoroutine = false;
    private IEnumerator SetNewItem(int itemID, Sprite sprite, Vector2 startItemPos)
    {
        while (setItemCoroutine)
            yield return new WaitForFixedUpdate();

        setItemCoroutine = true;

        moveItemImage.gameObject.SetActive(true);
        moveItemImage.GetComponent<Image>().sprite = sprite;
        moveItemImage.sizeDelta = startSizeMoveImage;
        moveItemImage.transform.position = startItemPos;

        Vector2 centreScreenPos = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 centreSize = new Vector2(startSizeMoveImage.x * 2, startSizeMoveImage.y * 2);

        while (Vector2.Distance(moveItemImage.transform.position, centreScreenPos) > 5f)
        {
            moveItemImage.transform.position = Vector2.Lerp(moveItemImage.transform.position, centreScreenPos, 0.05f);
            moveItemImage.sizeDelta = Vector2.Lerp(moveItemImage.sizeDelta, centreSize, 0.05f);
            yield return new WaitForFixedUpdate();
        }

        while (Vector2.Distance(moveItemImage.transform.position, inventoryPointPos.transform.position) > 5f)
        {
            moveItemImage.transform.position = Vector2.Lerp(moveItemImage.transform.position, inventoryPointPos.transform.position, 0.1f);
            moveItemImage.sizeDelta = Vector2.Lerp(moveItemImage.sizeDelta, Vector2.zero, 0.1f);
            yield return new WaitForFixedUpdate();
        }

        moveItemImage.sizeDelta = startSizeMoveImage;
        moveItemImage.gameObject.SetActive(false);

        itemIDs.Add(itemID);
        SetNewCard(itemID);

        setItemCoroutine = false;
    }
}

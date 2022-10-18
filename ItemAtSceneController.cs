using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace inventory_system
{
    public class ItemAtSceneController : InteractbleBase
    {
        [Header("Visual Setting")]
        public Rigidbody _rb;
        [SerializeField] private GameObject visualTraker;

        [SerializeField] private ItemBaseParametrs itemRef;
        [SerializeField] private int count;

        private void Start()
        {
            SetName(itemRef.GetItemName());
        }

        public void SetItemCount(int itemCount)
        {
            count = itemCount;
        }

        //Если используется данная функция значит итем был поднят и перенесен в инвентарь
        public ItemTransaction GetItemInfo()
        {
            ItemTransaction newData = new ItemTransaction();
            newData.item = itemRef;
            newData.itemCount = count;

            Destroy(gameObject);

            return newData;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 31)
            {
                _rb.isKinematic = true;
                visualTraker.SetActive(true);
                visualTraker.transform.rotation = Quaternion.identity;
            }
        }
    }
}
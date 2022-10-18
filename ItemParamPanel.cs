using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace inventory_system
{
    public class ItemParamPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text t_name;
        [SerializeField] private TMP_Text t_value;

        public void SetParam(string _n, string _v)
        {
            t_name.text = _n;
            t_value.text = _v;
        }
    }
}
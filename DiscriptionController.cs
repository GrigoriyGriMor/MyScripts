using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

namespace nnk
{
    public class DiscriptionController : MonoBehaviour
    {
        [SerializeField] private Button b_close;

        [Header("Resources Data")]
        private Parametrs_Data resources_data;
        [SerializeField] private RectTransform contentResourceData;
        [SerializeField] private GameObject paramBlock;

        [Header("Web Data")]
        [SerializeField] private Vector2 realyWorld_coordinates;
        [SerializeField] private TMP_Text t_cityName;
        [SerializeField] private TMP_Text t_temp;
        [SerializeField] private TMP_Text t_feelsLike;
        [SerializeField] private TMP_Text t_url;

        Coroutine requestCoroutine;

        void Start()
        {
            b_close.onClick.AddListener(() =>
            {
                DeactivateWindows();
                gameObject.SetActive(false);
            });

            string json_data = Resources.Load<TextAsset>("database").text;
            resources_data = JsonUtility.FromJson<Parametrs_Data>(json_data);

            for (int i = 0; i < resources_data.data.Length; i++)
            {
                ParamBlockController block = Instantiate(paramBlock, Vector3.zero, Quaternion.identity, contentResourceData).GetComponent<ParamBlockController>();
                block.Init(resources_data.data[i]);
            }
        }

        public void ActivateWindows()
        {
            if (requestCoroutine != null)
                StopCoroutine(requestCoroutine);

            requestCoroutine = StartCoroutine(RequestWebData());
        }

        public IEnumerator RequestWebData()
        {
            string request = $"{WebData.Domain}lat={realyWorld_coordinates.x}&lon={realyWorld_coordinates.y}";

            using (UnityWebRequest www = UnityWebRequest.Get(request))
            {
                www.SetRequestHeader(WebData.Header, WebData.Ya_Key);
                yield return www.SendWebRequest();

                if (www.isHttpError || www.isNetworkError)
                {
                    Debug.LogError("Error " + www.error);
                    www.Dispose();
                    yield break;
                }

                Yandex_Meteo meteo_data = JsonUtility.FromJson<Yandex_Meteo>(www.downloadHandler.text);
                SetMeteoParametrs(meteo_data);

                www.Dispose();
            }

            requestCoroutine = null;
        }

        private void SetMeteoParametrs(Yandex_Meteo _meteo_data)
        {
            t_cityName.text = _meteo_data.info.tzinfo.name;
            t_temp.text = _meteo_data.fact.temp.ToString();
            t_feelsLike.text = _meteo_data.fact.feels_like.ToString();
            t_url.text = _meteo_data.info.url;
        }

        public void DeactivateWindows()
        {
            if (requestCoroutine != null)
                StopCoroutine(requestCoroutine);
        }
    }
}
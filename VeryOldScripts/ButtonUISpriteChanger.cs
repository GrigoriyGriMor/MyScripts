//Примерно 2021-й год, тоже давно писал, но все робит и сейчас
//реализация sound system для приложения с множеством разных игр. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace sound_system
{
    public class ButtonUISpriteChanger : MonoBehaviour
    {
        [Header("Если UI элемент")]
        [SerializeField] private Image buttonImage;

        [Header("Если Sprite объект на сцене")]
        [SerializeField] private SpriteRenderer buttonSprite;

        [SerializeField] private Sprite on_sound;
        [SerializeField] private Sprite off_sound;

        IEnumerator Start()
        {
            while (!SoundManagerAllControll.Instance)
                yield return new WaitForFixedUpdate();

            if (SoundManagerAllControll.Instance) SoundManagerAllControll.Instance.SoundButtonInit(this);
        }

        public void ChangeSprite(bool On)
        {
            if (buttonImage != null)
            {
                if (!On)
                    buttonImage.sprite = off_sound;
                else
                    buttonImage.sprite = on_sound;
            }
            else
            if (buttonSprite != null)
            {
                if (!On)
                    buttonSprite.sprite = off_sound;
                else
                    buttonSprite.sprite = on_sound;
            }
        }
    }
}
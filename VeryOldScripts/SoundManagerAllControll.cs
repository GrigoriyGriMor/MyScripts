//Примерно 2021-й год, тоже давно писал, но все робит и сейчас
//реализация sound system для приложения с множеством разных игр. 
//Инициализируется один раз, при запуске приложения у клиента, на отдельной сцене загрузки данных приложения, поэтому дубля быть не может в принципе. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace sound_system
{
    public class SoundManagerAllControll : MonoBehaviour
    {
        private static SoundManagerAllControll instance;
        public static SoundManagerAllControll Instance => instance;

        [SerializeField] private int current_souce = 5;
        [SerializeField] private List<AudioSource> mySorce = new List<AudioSource>();
        [SerializeField] private List<AudioSource> otherSorce = new List<AudioSource>();


        [SerializeField] private List<AudioSource> backgrounSorce = new List<AudioSource>();
        [SerializeField] [Range(0, 1)] private float backgroundVolue = 0.7f;

        [SerializeField] private ButtonUISpriteChanger muteButton;

        public enum SoundPriority { background, steps, avto, mehanicanism, fire, damage, UI, talking, system }
        [Header("Sound Priority Block")]
        [Range(0, 256)] [SerializeField] private int priority_background = 5;
        [Range(0, 256)] [SerializeField] private int priority_UI = 5;
        [Range(0, 256)] [SerializeField] private int priority_system = 5;
        [Range(0, 256)] [SerializeField] private int priority_steps = 5;
        [Range(0, 256)] [SerializeField] private int priority_avto = 5;
        [Range(0, 256)] [SerializeField] private int priority_mehanicanism = 5;
        [Range(0, 256)] [SerializeField] private int priority_talking = 5;
        [Range(0, 256)] [SerializeField] private int priority_fire = 5;
        [Range(0, 256)] [SerializeField] private int priority_damage = 5;

        Coroutine backgroundCoroutine;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            instance = this;
            Init();
        }

        private void Init()
        {
            AudioSource _backgrounSorce = gameObject.AddComponent<AudioSource>();//создаем отдельный сорс для бэкграунда
            _backgrounSorce.priority = priority_background;
            backgrounSorce.Add(_backgrounSorce);

            for (int i = 0; i < current_souce; i++)
                mySorce.Add(gameObject.AddComponent<AudioSource>());
        }

        public void SoundButtonInit(ButtonUISpriteChanger button)
        {
            muteButton = button;

            if (backgrounSorce[0].volume == 0)
                muteButton.ChangeSprite(false);
            else
                muteButton.ChangeSprite(true);

            muteButton.gameObject.GetComponent<Button>().onClick.AddListener(() => OnSoundSwitch());
        }

        public void BackgroundClipPlay(AudioClip clip)
        {
            AudioSource newClip = gameObject.AddComponent<AudioSource>();
            newClip.clip = clip;
            newClip.priority = priority_background;
            newClip.loop = true;

            backgrounSorce.Add(newClip);
            backgrounSorce[backgrounSorce.Count - 1].Play();

            if (backgroundCoroutine != null)
                StopCoroutine(backgroundCoroutine);

            backgroundCoroutine = StartCoroutine(SwapBackgroundSoundVolue());
        }

        private IEnumerator SwapBackgroundSoundVolue()
        {
            while (backgrounSorce[backgrounSorce.Count - 1].volume < (backgroundVolue - backgroundVolue * 0.05f))
            {
                for (int i = 0; i < backgrounSorce.Count; i++)
                {
                    if (i == backgrounSorce.Count - 1)
                        backgrounSorce[i].volume = Mathf.Lerp(backgrounSorce[i].volume, backgroundVolue, Time.deltaTime);
                    else
                        backgrounSorce[i].volume = Mathf.Lerp(backgrounSorce[i].volume, 0, Time.deltaTime);
                }
                yield return new WaitForFixedUpdate();
            }

            backgrounSorce[backgrounSorce.Count - 1].volume = backgroundVolue;

            while (backgrounSorce.Count > 1)
            {
                Destroy(backgrounSorce[0]);
                backgrounSorce.RemoveAt(0);
            }

            backgroundCoroutine = null;
        }

        public void LeaveScene()
        {
            while (backgrounSorce.Count > 1)
            {
                Destroy(backgrounSorce[0]);
                backgrounSorce.RemoveAt(0);
            }

            backgrounSorce[0].clip = null;

            if (muteButton != null) muteButton.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            muteButton = null;

            //== потребуется заменить ==
            for (int i = otherSorce.Count - 1; i >= 0; i--)
                if (otherSorce[i].loop)
                    Destroy(otherSorce[i]);
            //==                      ==

            otherSorce.RemoveRange(0, otherSorce.Count);
        }

        private void SetPriority(AudioSource sr, SoundPriority pr)
        {
            switch (pr)
            {
                case SoundPriority.background:
                    sr.priority = priority_background;
                    break;
                case SoundPriority.UI:
                    sr.priority = priority_UI;
                    break;
                case SoundPriority.system:
                    sr.priority = priority_system;
                    break;
                case SoundPriority.steps:
                    sr.priority = priority_steps;
                    break;
                case SoundPriority.avto:
                    sr.priority = priority_avto;
                    break;
                case SoundPriority.mehanicanism:
                    sr.priority = priority_mehanicanism;
                    break;
                case SoundPriority.talking:
                    sr.priority = priority_talking;
                    break;
                case SoundPriority.fire:
                    sr.priority = priority_fire;
                    break;
                case SoundPriority.damage:
                    sr.priority = priority_damage;
                    break;
            }
        }

        public void ClipPlay(AudioClip clip, SoundPriority pr = SoundPriority.background)
        {
            AudioSource sr = GetFree();
            if (sr != null) sr.PlayOneShot(clip);

            SetPriority(sr, pr);
        }

        public void ClipPlay(AudioClip clip, AudioSource _source, SoundPriority pr = SoundPriority.background)
        {
            if (otherSorce.Contains(_source))
            {
                _source.PlayOneShot(clip);
                SetPriority(_source, pr);
                return;
            }

            otherSorce.Add(_source);
            if (backgrounSorce[0].volume == 0)
                _source.volume = 0;
            else
                _source.PlayOneShot(clip);

            SetPriority(_source, pr);
        }

        public void ClipLoopAndPlay(AudioClip clip, SoundPriority pr = SoundPriority.background)
        {
            AudioSource sr = GetFree();
            sr.clip = clip;
            sr.loop = true;
            SetPriority(sr, pr);
            sr.Play();
        }

        public void ClipLoopAndPlay(AudioClip clip, AudioSource _source, SoundPriority pr = SoundPriority.background)
        {
            if (otherSorce.Contains(_source))
            {
                _source.clip = clip;
                _source.loop = true;
                SetPriority(_source, pr);
                _source.Play();
            }

            otherSorce.Add(_source);
            if (backgrounSorce[0].volume == 0)
            {
                _source.clip = clip;
                _source.loop = true;
                _source.Play();

                _source.volume = 0;
            }
            else
            {
                _source.clip = clip;
                _source.loop = true;
                _source.Play();
            }

            SetPriority(_source, pr);
        }

        private AudioSource GetFree()
        {
            for (int i = 0; i < mySorce.Count; i++)
            {
                if (mySorce[i].clip == null)
                    return mySorce[0];
            }

            AudioSource newClip = gameObject.AddComponent<AudioSource>();
            otherSorce.Add(newClip);

            return newClip;
        }

        public void ClearSound()
        {
            for (int i = 0; i < mySorce.Count; i++)
            {
                if (mySorce[i] != null)
                {
                    if (mySorce[i].clip != null)
                        mySorce[i].clip = null;
                }
            }
        }

        #region Controllers
        public void OnSoundSwitch()
        {
            if (backgrounSorce[0].volume == 0)
            {
                for (int i = 0; i < backgrounSorce.Count; i++)
                    backgrounSorce[i].volume = backgroundVolue;

                for (int i = 0; i < mySorce.Count; i++)
                    mySorce[i].volume = 1;

                for (int i = 0; i < otherSorce.Count; i++)
                    otherSorce[i].volume = 1;


                if (muteButton)
                    muteButton.ChangeSprite(true);
            }
            else
            {
                for (int i = 0; i < backgrounSorce.Count; i++)
                    backgrounSorce[i].volume = 0.0f;

                for (int i = 0; i < mySorce.Count; i++)
                    mySorce[i].volume = 0;

                for (int i = 0; i < otherSorce.Count; i++)
                    otherSorce[i].volume = 0;

                if (muteButton)
                    muteButton.ChangeSprite(false);
            }
        }

        public void OnBackgroundSound()
        {
            if (backgrounSorce[0].volume == 0)
                for (int i = 0; i < backgrounSorce.Count; i++)
                    backgrounSorce[i].volume = backgroundVolue;
            else
                for (int i = 0; i < backgrounSorce.Count; i++)
                    backgrounSorce[i].volume = 0.0f;
        }

        public void OnSound()
        {
            for (int i = 0; i < backgrounSorce.Count; i++)
                backgrounSorce[i].volume = backgroundVolue;

            for (int i = 0; i < mySorce.Count; i++)
                mySorce[i].volume = 1;

            for (int i = 0; i < otherSorce.Count; i++)
                otherSorce[i].volume = 1;


            if (muteButton)
                muteButton.ChangeSprite(true);
        }

        public void OffSound()
        {
            for (int i = 0; i < backgrounSorce.Count; i++)
                backgrounSorce[i].volume = 0.0f;

            for (int i = 0; i < mySorce.Count; i++)
                mySorce[i].volume = 0;

            for (int i = 0; i < otherSorce.Count; i++)
                otherSorce[i].volume = 0;

            if (muteButton)
                muteButton.ChangeSprite(false);
        }
        #endregion
    }
}
//Очень Древняя писанина, но рабочая :D 
//реализация вращения трех барабанов с числами и знаками, которые в итоге выдают результат. Калькулятор на барабанах с рандомным вращением.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NumericalAggregator : MonoBehaviour
{
    [SerializeField] private WheelInfo[] wheels = new WheelInfo[3];

    [Header("Диапозон скорости вращения барабана")]
    [SerializeField] private float minRotateSpeed = 10;
    [SerializeField] private float maxRotateSpeed = 50;
    //private float factRotateSpeed;
    private Vector3 wPos;
    private Quaternion wRotate;

    [SerializeField] private float maxBreakForce = 1000;
    private float breakForce = 0;

    [Header("Эффект при утверждении числа")]
    [SerializeField] private ParticleSystem[] particle = new ParticleSystem[1];

    [Header("Объект с результатом")]
    [SerializeField] private GameObject[] resultObj = new GameObject[2];
    [SerializeField] private TextMeshPro resultText; 
   // [SerializeField] private AnglePosValue[] nomber = new AnglePosValue[4];

    [Header("Кто ждет результат?")]
    [SerializeField] private CSPlayerController player;
    [SerializeField] private CSWarriorController warrior;

    private int fireObjCount = 15;

    private bool barabanUse = false;

    private bool endPos = false;

    private void Start()
    {
        if (player == null && warrior == null)
            Debug.LogError("В объекте подбора чисел ошибка! не указан класс, который ждет ответа");

        if (minRotateSpeed > maxRotateSpeed) 
            maxRotateSpeed = minRotateSpeed + 5;

        breakForce = 0;

        for (int i = 0; i < wheels.Length; i++)
        {
            for (int j = 0; j < wheels[i].number.Length; j++)
            {
                if (j < (wheels[i].number.Length - 1))
                {
                    if (j == 0) wheels[i].number[j].minAngle = 0;

                    wheels[i].number[j].maxAngle = wheels[i].number[j + 1].minAngle;
                }
                else
                    wheels[i].number[j].maxAngle = 360;
            }
        }
            StartCoroutine(StartSelection());
    }

    public IEnumerator StartSelection()
    {
        yield return new WaitForSeconds(1.15f);
        GetComponent<Animator>().enabled = false;

        for (int i = 0; i < wheels.Length; i++)
            if (i % 2 == 0)
                wheels[i].factRotateSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);
            else
                wheels[i].factRotateSpeed = Random.Range(minRotateSpeed, maxRotateSpeed) * -1;

        barabanUse = true;
    }

    public void StopSelection()
    {
        StartCoroutine(StopTime());
    }

    private IEnumerator StopTime()
    {
       barabanUse = false;

       while (breakForce < maxBreakForce)
        {
            breakForce += maxBreakForce / 25;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.2f);

        List<int> fireObjCount = new List<int>();
        int sings = 0;

        for (int i = 0; i < wheels.Length; i++)
        {
            float angle = wheels[i].visual.transform.eulerAngles.x;
            if (angle < 0)
                angle = 360.0f - angle;

            if (angle > 360)
                angle = angle - 360;

            if (!wheels[i].sing)
            {
                for (int j = 0; j < wheels[i].number.Length; j++)
                {
                    if (angle >= wheels[i].number[j].minAngle && angle <= wheels[i].number[j].maxAngle)
                    {
                        fireObjCount.Add(wheels[i].number[j].value);

                        wheels[i].wheelDisk.GetWorldPose(out wPos, out wRotate);
                        wheels[i].targetAngle = Quaternion.Euler(wheels[i].number[j].minAngle + ((wheels[i].number[j].maxAngle - wheels[i].number[j].minAngle) / 2), wRotate.y + (90/*75 + (i * 15)*/), 0);
                    }    
                }
            }
            else
            {
                for (int j = 0; j < wheels[i].number.Length; j++)
                {
                    if (angle >= wheels[i].number[j].minAngle && angle <= wheels[i].number[j].maxAngle)
                    {
                        sings = wheels[i].number[j].value;

                        wheels[i].wheelDisk.GetWorldPose(out wPos, out wRotate);
                        wheels[i].targetAngle = Quaternion.Euler(wheels[i].number[j].minAngle + ((wheels[i].number[j].maxAngle - wheels[i].number[j].minAngle) / 2), wRotate.y + (90/*75 + (i * 15)*/), 0);
                    }
                }
            }
        }

        endPos = true;

        int fireObjCountResult = 0;
        switch (sings)
        {
            case 1:
                fireObjCountResult = fireObjCount[0] + fireObjCount[1];
                break;
            case 2:
                fireObjCountResult = fireObjCount[0] * fireObjCount[1];
                break;
            case 3:
                fireObjCountResult = fireObjCount[0] - fireObjCount[1];
                break;
            case 4:
                fireObjCountResult = Mathf.FloorToInt(fireObjCount[0] / fireObjCount[1]);
                break;
            default:
                fireObjCountResult = fireObjCount[0] + fireObjCount[1];
                break;
        }

        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < particle.Length; i++)
        {
            yield return new WaitForSeconds(0.1f);
            particle[i].Play();
        }

        for (int i = 0; i < resultObj.Length; i++)
            resultObj[i].SetActive(true);
        resultText.text = fireObjCountResult.ToString();

        if (player != null)
            player.StopNumerical(fireObjCountResult, resultText);
        else
            if (warrior != null)
            warrior.StopNumerical();
    }

    private void FixedUpdate()
    {
        if (barabanUse)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].wheelDisk.brakeTorque = 0;
                if (wheels[i].factRotateSpeed >= 0)
                {
                    if (wheels[i].wheelDisk.rpm < wheels[i].factRotateSpeed)
                        wheels[i].wheelDisk.motorTorque = 1500 * Mathf.Clamp(wheels[i].factRotateSpeed, -1, 1);
                    else
                        wheels[i].wheelDisk.motorTorque = 0;
                }
                else
                {
                    if (wheels[i].wheelDisk.rpm > wheels[i].factRotateSpeed)
                        wheels[i].wheelDisk.motorTorque = 1500 * Mathf.Clamp(wheels[i].factRotateSpeed, -1, 1);
                    else
                        wheels[i].wheelDisk.motorTorque = 0;
                }
            }
        }
        else
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].wheelDisk.brakeTorque = breakForce;
                wheels[i].wheelDisk.motorTorque = 0;
            }
        }

        if (!endPos)
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].wheelDisk.GetWorldPose(out wPos, out wRotate);
                wheels[i].visual.rotation = wRotate;
            }
        else
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].visual.localRotation = Quaternion.Lerp(wheels[i].visual.localRotation, wheels[i].targetAngle, 5 * Time.deltaTime);
            }
        }
    }
}

[System.Serializable]
public class WheelInfo
{
    public WheelCollider wheelDisk;
    public Transform visual;

    [HideInInspector] public float factRotateSpeed;

    [Header("Если знак: +=1, *=2, -=3, /=4")]
    public bool sing = false;

    [HideInInspector] public Quaternion targetAngle;

    [Header("Сколько чисел, столько и диапазонов")]
    public AnglePosValue[] number = new AnglePosValue[4];
}

[System.Serializable]
public class AnglePosValue
{
    [Header("Диапазон угла активирующего значение")]
    public float minAngle;
     public float maxAngle;

    public int value;
}

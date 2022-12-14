using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace player_moduls
{
    public class MoveModule : CharacterBase
    {
        [Header("Move Setting")]
        [SerializeField] private float moveSpeed = 1;
        [SerializeField] private float sprintSpeed = 2;
        [SerializeField] private float rotationSpeed = 5f;

        [SerializeField] private float stopRunImpuls = 250;

        private bool isSprint = false;

        public void MoveAxis(Vector2 moveVector, SupportClass.PlayerStateMode playerMode = SupportClass.PlayerStateMode.Idle, bool isCrouch = false)
        {
            switch (playerMode)
            {
                case SupportClass.PlayerStateMode.Idle:
                    {
                        playerAnim.SetFloat("MoveRight", 0);

                        if (isSprint && new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) == Vector2.zero)
                        {
                            playerAnim.SetTrigger("BreakFastRunAnim");

                            _rb.AddForce(transform.forward * stopRunImpuls, ForceMode.Impulse);
                        }

                        if (moveVector != Vector2.zero)
                        {

                            Vector3 _vec = new Vector3(moveVector.y * moveSpeed, _rb.velocity.y, moveVector.x * moveSpeed);
                            float _angle = Mathf.Atan2(moveVector.y, moveVector.x) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
                            playerAnim.transform.rotation = Quaternion.Lerp(playerAnim.transform.rotation, Quaternion.Euler(0.0f, _angle, 0.0f), rotationSpeed * Time.deltaTime);

                            _rb.velocity = transform.TransformVector(_vec);

                            if (!isCrouch)
                                playerAnim.SetFloat("Speed", Mathf.LerpUnclamped(playerAnim.GetFloat("Speed"), Mathf.Abs(Mathf.Clamp(new Vector2(moveVector.x, moveVector.y).sqrMagnitude, -1, 1)), 10 * Time.deltaTime));
                            else
                                playerAnim.SetFloat("Speed", Mathf.LerpUnclamped(playerAnim.GetFloat("Speed"), Mathf.Abs(Mathf.Clamp(new Vector2(moveVector.x, moveVector.y).sqrMagnitude, -0.5f, 0.5f)), 10 * Time.deltaTime));
                        }
                        else
                            playerAnim.SetFloat("Speed", Mathf.LerpUnclamped(playerAnim.GetFloat("Speed"), 0, 15 * Time.deltaTime));

                        isSprint = false;
                    }
                    break;
                case SupportClass.PlayerStateMode.Combat:
                    {
                        if (moveVector == Vector2.zero && isSprint)
                        {
                            isSprint = false;
                            playerAnim.SetTrigger("BreakFastRunAnim");

                            _rb.AddForce(transform.forward * moveSpeed, ForceMode.Impulse);
                        }

                        Vector3 vec = new Vector3(moveVector.y * moveSpeed, _rb.velocity.y, moveVector.x * moveSpeed);
                        _rb.velocity = transform.TransformVector(vec);

                        playerAnim.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

                        playerAnim.SetFloat("Speed", moveVector.x);
                        playerAnim.SetFloat("MoveRight", moveVector.y);
                    }
                    break;
                case SupportClass.PlayerStateMode.Sprint:
                    {
                        isSprint = true;

                        playerAnim.SetFloat("MoveRight", 0);

                        Vector3 _vec = new Vector3(moveVector.y * sprintSpeed, _rb.velocity.y, moveVector.x * sprintSpeed);
                        float _angle = Mathf.Atan2(moveVector.y, moveVector.x) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
                        playerAnim.transform.rotation = Quaternion.Euler(0.0f, _angle, 0.0f);

                        _rb.velocity = transform.TransformVector(_vec);

                        if (moveVector != Vector2.zero)
                            playerAnim.SetFloat("Speed", Mathf.LerpUnclamped(playerAnim.GetFloat("Speed"), 1.5f, 10 * Time.deltaTime));
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
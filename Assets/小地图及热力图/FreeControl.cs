using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class FreeControl : MonoBehaviour, IMiniItem
{
    float moveSpeed = 100;
    bool AddSpeed;
    public CinemachineVirtualCamera FreeCam;

    public MiniType miniType { get; set; }
    public Transform Transform { get; set; }

    private void Start()
    {
        miniType = MiniType.Look;
        Transform = this.transform;

        GameMgr.Instance.ChangeFree += () => 
        {
            this.transform.position = Camera.main.transform.position;
            this.transform.rotation = Camera.main.transform.rotation;

            FreeCam.transform.position = Camera.main.transform.position;
            FreeCam.transform.rotation = Camera.main.transform.rotation;
        };
    }


    void Update()
    {
        switch (GameMgr.Instance.camFollowType)
        {
            case CamFollowType.First:
                this.transform.parent = GameMgr.Instance.FirstCam.transform;
                this.transform.localPosition = Vector3.zero;
                this.transform.localRotation = Quaternion.identity;
                break;
            case CamFollowType.Second:
                this.transform.parent = GameMgr.Instance.SecondCam.transform;
                this.transform.localPosition = Vector3.zero;
                this.transform.localRotation = Camera.main.transform.rotation;
                break;
            case CamFollowType.Free:
                this.transform.parent = null;
                break;
            default:
                break;
        }


        if (GameMgr.Instance.camFollowType == CamFollowType.Free)
        {
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            if (Input.GetKey(KeyCode.Q))
                input.y = 1;
            else if (Input.GetKey(KeyCode.E))
                input.y = -1;
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                AddSpeed = !AddSpeed;
                moveSpeed = AddSpeed ? 200 : 100;
            }
            if (Input.GetMouseButton(1))
            {
                FreeCam.transform.DORotate(new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * 10, 0.5f)
                    .SetRelative()
                    .SetEase(Ease.Linear)
                    .OnUpdate(() =>
                    {
                        this.transform.rotation = FreeCam.transform.rotation;
                    });
            }
            input = Quaternion.Euler(new Vector3(0, FreeCam.transform.eulerAngles.y, 0)) * input;
            this.transform.position += input * Time.deltaTime * moveSpeed;
        }
    }

    private void OnDisable()
    {
        MiniMapManager.Instance.MiniItems.Remove(this);
    }

    private void OnEnable()
    {
        MiniMapManager.Instance.MiniItems.Add(this);
    }
}

using System.Collections.Generic;
using Cinemachine;
using System.Linq;
using DG.Tweening;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Dropdown = UnityEngine.UI.Dropdown;


public class GameMgr : MonoBehaviour
{
    public Dropdown SelectCar;

    public Dropdown CamType;

    public Dropdown MiniMapType;

    public CinemachineVirtualCameraBase FirstCam;
    public CinemachineVirtualCameraBase SecondCam;
    public CinemachineVirtualCameraBase FreeCam;

    public CamFollowType camFollowType = CamFollowType.First;

    public static GameMgr Instance;

    public UnityAction ChangeFree;

    private void Awake()
    {
        Instance = this;
    }

    private MiniMapManager _miniMapManager;
    public void Init(MiniMapManager miniMapManager)
    {
        _miniMapManager = miniMapManager;

        SelectCarBind();
        
        CamTypeBind();
        
        MiniMapTypeBind();
    }


    private void SelectCarBind()
    {
        SelectCar.options.Clear();
                
        var tankeList = _miniMapManager.MiniItems.Where(_ => _.miniType == MiniType.Red || _.miniType == MiniType.Blue)
            .ToList();
        
        HeatMapMgr.Instance.Init(tankeList.Count);

        Dropdown.OptionData[] optionData = new Dropdown.OptionData[tankeList.Count];
        for (int i = 0; i < tankeList.Count; i++)
        {
            optionData[i] = new Dropdown.OptionData() { text = "Item_" + i };
        }

        SelectCar.options.AddRange(optionData);
        SelectCar.onValueChanged.AddListener((_) =>
        {
            FirstCam.Follow = tankeList[_].Transform;
            FirstCam.LookAt = tankeList[_].Transform;

            SecondCam.Follow = tankeList[_].Transform;
            SecondCam.LookAt = tankeList[_].Transform;
        });
        SelectCar.RefreshShownValue();
    }

    private void CamTypeBind()
    {
        Dropdown.OptionData[] optionDataB = new Dropdown.OptionData[3];
        optionDataB[0] = new Dropdown.OptionData() { text = "第一视角" };
        optionDataB[1] = new Dropdown.OptionData() { text = "第三视角" };
        optionDataB[2] = new Dropdown.OptionData() { text = "自由视角" };
        CamType.options.Clear();
        CamType.options.AddRange(optionDataB);
        CamType.onValueChanged.AddListener((_) =>
        {
            switch (_)
            {
                case 0:
                    FirstCam.Priority = 10;
                    SecondCam.Priority = 0;
                    FreeCam.Priority = 0;

                    camFollowType = CamFollowType.First;
                    break;
                case 1:
                    FirstCam.Priority = 0;
                    SecondCam.Priority = 10;
                    FreeCam.Priority = 0;

                    camFollowType = CamFollowType.Second;
                    break;
                case 2:
                    FirstCam.Priority = 0;
                    SecondCam.Priority = 0;
                    FreeCam.Priority = 10;

                    camFollowType = CamFollowType.Free;
                    ChangeFree?.Invoke();
                    break;
            }
        });
        
        CamType.RefreshShownValue();
    }

    private void MiniMapTypeBind()
    {
        Dropdown.OptionData[] optionDataB = new Dropdown.OptionData[3];
        optionDataB[0] = new Dropdown.OptionData() { text = "扫描模式" };
        optionDataB[1] = new Dropdown.OptionData() { text = "实时模式" };
        optionDataB[2] = new Dropdown.OptionData() { text = "热力图模式" };

        MiniMapType.options.Clear();
        MiniMapType.options.AddRange(optionDataB);
        MiniMapType.onValueChanged.AddListener((_) =>
        {
            switch (_)
            {
                case 0:
                    _miniMapManager.enableScann = true;
                    HeatMapMgr.Instance.Disable();
                    foreach (var VARIABLE in _miniMapManager.uiTraces)
                    {
                        VARIABLE.StopAllChange();
                        VARIABLE.BiggerImage.transform.localScale = Vector3.zero;
                        VARIABLE.State.transform.localScale = Vector3.zero;
                        
                        VARIABLE.BiggerImage.DOFade(0f, 0);
                        VARIABLE.State.DOFade(0f, 0);
                    }
                    _miniMapManager.scanningLine.SetActive(true);
                    _miniMapManager.transform.Find("MiniMapOther").gameObject.SetActive(true);
                    _miniMapManager.transform.Find("MiniMapMask").GetComponent<Mask>().enabled = true;
                    break;
                case 1:
                    _miniMapManager.enableScann = false;
                    HeatMapMgr.Instance.Disable();
                    foreach (var VARIABLE in _miniMapManager.uiTraces)
                    {
                        VARIABLE.StopAllChange();
                        VARIABLE.BiggerImage.transform.localScale = Vector3.zero;
                        VARIABLE.State.transform.localScale = Vector3.one;
                        
                        VARIABLE.BiggerImage.DOFade(0f, 0);
                        VARIABLE.State.DOFade(1f, 0);
                    }
                    
                    _miniMapManager.scanningLine.SetActive(true);
                    _miniMapManager.transform.Find("MiniMapOther").gameObject.SetActive(true);
                    _miniMapManager.transform.Find("MiniMapMask").GetComponent<Mask>().enabled = true;
                    break;
                case 2:
                    _miniMapManager.enableScann = false;
                    HeatMapMgr.Instance.Enable();
                    foreach (var VARIABLE in _miniMapManager.uiTraces)
                    {
                        VARIABLE.StopAllChange();
                        VARIABLE.BiggerImage.transform.localScale = Vector3.zero;
                        VARIABLE.State.transform.localScale = Vector3.one;

                        VARIABLE.BiggerImage.DOFade(0, 0);
                        VARIABLE.State.DOFade(0.39f, 0);
                    }

                    _miniMapManager.scanningLine.SetActive(false);
                    _miniMapManager.transform.Find("MiniMapOther").gameObject.SetActive(false);
                    _miniMapManager.transform.Find("MiniMapMask").GetComponent<Mask>().enabled = false;
                    break;
            }
        });
        MiniMapType.RefreshShownValue();
    }

    private void Update()
    {
        if (camFollowType == CamFollowType.Second && Input.GetMouseButton(1))
        {
            (SecondCam as CinemachineFreeLook).m_XAxis.Value += Input.GetAxis("Mouse X") * 2;
            (SecondCam as CinemachineFreeLook).m_YAxis.Value += Input.GetAxis("Mouse Y") * 0.01f;
        }
    }
}

public enum CamFollowType
{
    First,
    Second,
    Free
}
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapManager : MonoBehaviour
{
    public bool enableScann;

    public GameObject scanningLine;

    public Transform center;
    public bool followCenter;

    public Camera miniCam;

    public Transform miniMap;

    public RectTransform miniImage;

    public GameObject demoImage;

    public GameObject JumpTag;

    public float rotateSpeed;

    public List<UITrace> uiTraces;

    public int renderTextureSize;

    public List<IMiniItem> MiniItems = new List<IMiniItem>();

    public Dictionary<IMiniItem, UITrace> Items_UIs = new Dictionary<IMiniItem, UITrace>();
        
    public static MiniMapManager Instance;

    public Sprite[] uISprite;

    public Camera LookCam;

    public Transform direction1;
    public Transform direction2;

    private void Awake()
    {
        Instance = this;
        Invoke(nameof(Init), 2f);
    }

    private void Init()
    {
        RenderTexture renderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 0);
        miniCam.targetTexture = renderTexture;
        miniImage.GetComponent<RawImage>().texture = renderTexture;
        var transform1 = miniCam.transform;
        var position = center.position;
        transform1.position = new Vector3(position.x, transform1.position.y, position.z);
        
        foreach (var t in MiniItems)
        {
            UITrace uITrace = Instantiate(Resources.Load<UITrace>("UITrace"), miniImage);
            uITrace.Init(GetUITrace(t.miniType, out Color color), color, t.Transform);
            uiTraces.Add(uITrace);
            if (enableScann)
            {
                uITrace.State.transform.localScale = Vector3.zero;
            }
            else
            {
                uITrace.State.transform.localScale = Vector3.one;
            }
            Items_UIs.Add(t, uITrace);
        }
        GameMgr.Instance.Init(this);
        
    }

    private Sprite GetUITrace(MiniType type, out Color color)
    {
        switch (type)
        {
            case MiniType.None:
                color = Color.white;
                return null;
            case MiniType.Red:
                color = Color.red;
                return uISprite[0];
            case MiniType.Blue:
                color = Color.blue;
                return uISprite[0];
            case MiniType.Look:
                color = Color.yellow;
                return uISprite[1];
            default:
                Debug.LogError($"未处理的类型{type}");
                break;
        }
        color = Color.white;
        return null;
    }

    private void Update()
    {
        if (followCenter)
            miniCam.transform.position = center.transform.position;
        if (enableScann)
        {
            scanningLine.gameObject.SetActive(true);
            scanningLine.transform.Rotate(-Vector3.forward * (Time.deltaTime * rotateSpeed));
        }
        else
        {
            scanningLine.gameObject.SetActive(false);
        }

        if (GameMgr.Instance.camFollowType == CamFollowType.Free)
        {
            direction1.transform.DORotate(new Vector3(0, 0, LookCam.transform.eulerAngles.y), 0.8f).SetEase(Ease.Linear);
            direction2.transform.DORotate(new Vector3(0, 0, LookCam.transform.eulerAngles.y), 0.8f);
        }
        else
        {
            direction1.transform.DORotate(new Vector3(0, 0, miniCam.transform.eulerAngles.y), 0.8f).SetEase(Ease.Linear);
            direction2.transform.DORotate(new Vector3(0, 0, miniCam.transform.eulerAngles.y), 0.8f);
        }


        Trace();
        Detection();
        JumpMap();
    }

    /// <summary>
    /// UI跟踪目标
    /// </summary>
    private void Trace()
    {
        if (!enableScann)
        {
            foreach (var t in uiTraces)
            {
                Vector3 Tag = RectTransformUtility.WorldToScreenPoint(miniCam, t.Tag.position);
                Tag -= new Vector3(renderTextureSize / 2f, Screen.height / 2f, 0);
                Tag *= (miniMap.localScale.x * miniImage.localScale.x * miniImage.sizeDelta.x / renderTextureSize);
                Tag += miniMap.position;
                t.transform.position = Tag;
                t.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -t.Tag.rotation.eulerAngles.y));
            }
        }
    }


    /// <summary>
    /// 获取两物体角度
    /// </summary>
    /// <param name="posA"></param>
    /// <param name="posB"></param>
    /// <returns></returns>
    private float Two_ObjectAngle(Vector3 posA, Vector3 posB)
    {
        Vector3 dir = new Vector2(posB.x, posB.z) - new Vector2(posA.x, posA.z);
        float angle = Vector2.Angle(Vector2.up, dir); //求出两向量之间的夹角 
        angle *= Mathf.Sign(Vector2.Dot(dir, Vector2.left)); //求法线向量与物体上方向向量点乘，结果为1或-1，修正旋转方向 
        return angle;
    }

    private float _angle;

    /// <summary>
    /// 检测发现
    /// </summary>
    private void Detection()
    {
        _angle = scanningLine.transform.eulerAngles.z;
        _angle = _angle >= 180 ? _angle - 360 : _angle;
        _angle -= miniCam.transform.eulerAngles.y > 180
            ? miniCam.transform.eulerAngles.y - 360
            : miniCam.transform.eulerAngles.y;

        if (enableScann)
        {
            foreach (var item in uiTraces)
            {
                item.Angle = Two_ObjectAngle(center.transform.position, item.Tag.position);

                if (Mathf.Abs(item.Angle - _angle) < 5f)
                {
                    item.Show();
                    Vector3 Tag = RectTransformUtility.WorldToScreenPoint(miniCam, item.Tag.position);
                    Tag -= new Vector3(renderTextureSize / 2f, Screen.height / 2f, 0);
                    Tag *= (miniMap.localScale.x * miniImage.localScale.x * miniImage.sizeDelta.x / renderTextureSize);
                    Tag += miniMap.position;
                    item.transform.position = Tag;
                    item.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -item.Tag.rotation.eulerAngles.y));

                    // Vector2 heatpos = new Vector2(item.transform.localPosition.x, item.transform.localPosition.y) /
                    //                   miniImage.sizeDelta.x / 2;
                    // HeatMapMgr.Instance.AddHitPoint(heatpos);
                }
                else
                {
                    item.Hiden();
                }
            }
        }
    }

    [Header("ClickMap")] public float rayDistance = 200;
    public LayerMask raymask = -1;
    private RaycastHit _hitInfo;

    /// <summary>
    /// 点击地图 返回世界坐标
    /// </summary>
    private void JumpMap()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //demoImage.transform.position = Input.mousePosition;
            Ray ray = RectTransformUtility.ScreenPointToRay(miniCam,
                (Input.mousePosition - miniMap.position) / miniMap.localScale.x / miniImage.localScale.x /
                (miniImage.sizeDelta.x / renderTextureSize));
            var orthographicSize = miniCam.orthographicSize;
            ray.origin += new Vector3(orthographicSize, 0, orthographicSize); //UI摄像机的Size
            if (Physics.Raycast(ray.origin, ray.direction, out _hitInfo, rayDistance, raymask))
            {
                JumpTag.transform.position = _hitInfo.point + new Vector3(0, JumpTag.transform.position.y, 0);
            }
        }
    }

    public Transform NeatItem(Vector3 pos)
    {
        float minDistance = float.MaxValue;
        Transform back = null;
        foreach (var item in MiniItems)
        {
            var tempdistance = Vector3.Distance(item.Transform.position, pos);
            if (tempdistance == 0) continue;
            if (minDistance > tempdistance)
            {
                minDistance = tempdistance;
                back = item.Transform;
            }
        }
        return back;
    }
}

public struct RecordTrans
{
    public Vector3 Pos;
    public Quaternion Rot;

}

public enum MiniType
{
    None,
    Red, //玩家
    Blue, //敌人
    Look, //道具
}


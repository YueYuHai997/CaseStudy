using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HeatMapMgr : MonoBehaviour
{
    private Material _material;

    private float[] _points;
    private int _hitCount;
    
    private static readonly int Hits = Shader.PropertyToID("_Hits");
    private static readonly int HitCount = Shader.PropertyToID("_HitCount");

    public static HeatMapMgr Instance;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _material = this.GetComponent<Image>().material;
        //_material = this.GetComponent<MeshRenderer>().material;

    }

    private List<UITrace> tankeList = new List<UITrace>();
    public void Init(int num)
    {
        _points = new float[num * 3]; // 32 point;
        Width = MiniMapManager.Instance.miniImage.sizeDelta.x / 2;

        foreach (var VARIABLE in MiniMapManager.Instance.Items_UIs)
        {
            if (VARIABLE.Key.miniType == MiniType.Red || VARIABLE.Key.miniType == MiniType.Blue)
            {
                tankeList.Add(VARIABLE.Value);
            }
        }
    }
    
    private float Width;
    
    private void Update()
    {
        if (tankeList.Count > 1)
        {
            UpdateHitPoint(tankeList.Select((_) =>
            {
                return new Vector2(_.transform.localPosition.x, _.transform.localPosition.y);
            }).ToArray());
        }
    }
    
    
    public void UpdateHitPoint(Vector2[] pos)
    {
        for (int i = 0; i < pos.Length; i++)
        {
            _points[i * 3] = pos[i].x / Width * 0.5f + 0.5f;
            _points[i * 3 + 1] = pos[i].y / Width * 0.5f + 0.5f;
            _points[i * 3 + 2] = 1.5f;
        }

        _material.SetFloatArray(Hits, _points);
        _material.SetInt(HitCount, pos.Length);
    }

    public void Enable()
    {
        this.transform.localScale = Vector3.one;
    }

    public void Disable()
    {
        this.transform.localScale = Vector3.zero;
    }

    // private void OnCollisionEnter(Collision collision)
    // {
    //     foreach (ContactPoint cp in collision.contacts)
    //     {
    //         Vector3 startRay = cp.point - cp.normal;
    //         Vector3 rayDir = cp.normal;
    //
    //         //从世界坐标转换到UI坐标 (0,0)=>(1,1)
    //         Ray ray = new Ray(startRay, rayDir);
    //         if (Physics.Raycast(ray, out RaycastHit hit, 10f, LayerMask.GetMask("HeatMapLayer")))
    //         {
    //             Debug.Log("Hit Texture coordinates = " + hit.textureCoord.x + ',' + hit.textureCoord.y);
    //             AddHitPoint(hit.textureCoord.x, hit.textureCoord.y);
    //         }
    //         
    //         Destroy(cp.otherCollider.gameObject);
    //     }
    // }


}
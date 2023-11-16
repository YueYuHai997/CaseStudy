using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;

public class DemoItem : MonoBehaviour, IMiniItem
{
    public MiniType miniType { get; set; }
    public Transform Transform { get; set; }

    public Material RedMaterial;
    public Material BlueMaterial;
    public Canvas TankeUI;
    private void Start()
    {
        miniType = Random.Range(0, 1f) > 0.5f ? MiniType.Red : MiniType.Blue;
        if (miniType == MiniType.Red)
        {
            this.transform.Find("TankeModel").GetComponent<Renderer>().material = RedMaterial;
            TankeUI.transform.Find("Container/INfo2").GetComponent<Text>().text = "所属方：红";
        }
        else if (miniType == MiniType.Blue)
        {
            this.transform.Find("TankeModel").GetComponent<Renderer>().material = BlueMaterial;
            TankeUI.transform.Find("Container/INfo2").GetComponent<Text>().text = "所属方：蓝";
        }
        Transform = this.transform;

        MiniMapManager.Instance.MiniItems.Add(this);
        Rot();
    }

    private float MoveSpeed;

    private CancellationTokenSource cts = new CancellationTokenSource();
    async void Rot()
    {
        while (!cts.IsCancellationRequested)
        {
            await Task.Delay(3000);
            MoveSpeed = Random.Range(20f, 30f);
            this.transform.DORotate(new Vector2(0, Random.Range(-90f, 90f)), 2);
        }
    }

    private void OnDestroy()
    {
        cts.Cancel();
    }



    void Update()
    {
        this.transform.Translate(Vector3.forward * Time.deltaTime * MoveSpeed);

        TankeUI.transform.DOLookAt(Camera.main.transform.position, 1);
    }

}
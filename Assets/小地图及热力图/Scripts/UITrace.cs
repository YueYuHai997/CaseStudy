using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class UITrace : MonoBehaviour
{
    public float Angle;
    public Image BiggerImage;
    public Image State;
    public Transform Tag;

    public UnityAction Show;
    public UnityAction Hiden;

    private Sequence sequence;

    private void Awake()
    {
        sequence = DOTween.Sequence();
        sequence.SetAutoKill(false);
        sequence.Join(BiggerImage.DOFade(1, 0));
        sequence.Join(BiggerImage.transform.DOScale(Vector3.zero, 0));
        sequence.Join(BiggerImage.DOFade(0, 2));
        sequence.Join(BiggerImage.DOFade(0, 2));
        sequence.Join(State.DOFade(1, 0));
        sequence.Join(BiggerImage.transform.DOScale(Vector3.one * 5, 2));
        sequence.AppendInterval(180 / MiniMapManager.Instance.rotateSpeed);
        sequence.Append(State.DOFade(0, 1));
    }


    public void Init(Sprite sprite, Color color, Transform transform)
    {

        BiggerImage.sprite = sprite;
        State.sprite = sprite;
        Tag = transform;
        Show += ChangeColor;
        Hiden += BackColor;

        BiggerImage.color = color;
        State.color = color;

        BiggerImage.transform.localScale = Vector3.zero;
    }


    private bool _canChange;

    private void ChangeColor()
    {
        if (!_canChange)
            _canChange = false;
        
        State.transform.localScale = Vector3.one;
        
        sequence.Restart();
    }


    public void StopAllChange()
    {
        sequence.Pause();
    }

    private void BackColor()
    {
        _canChange = true;
    }
}



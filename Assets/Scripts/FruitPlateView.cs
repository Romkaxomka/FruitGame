using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FruitPlateView : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public RectTransform rectTransform { get; private set; }
    public Image image { get; private set; }
    public CanvasGroup canvasGroup { get; private set; }

    public UnityEvent<PointerEventData> OnBeginDragEvent;
    public UnityEvent<PointerEventData> OnDragEvent;
    public UnityEvent<PointerEventData> OnEndDragEvent;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public async UniTask MoveTo(Vector2 pos, float duration)
    {
        rectTransform.DOKill();
        await rectTransform.DOAnchorPos(pos, duration);
    }

    public async UniTask Collect()
    {
        await canvasGroup.DOFade(0f, .05f);
    }

    //public void SetDebugView(bool debugEnabled)
    //{
    //    if(debugEnabled)
    //    {
    //        var texture = Resources.Load<Texture2D>("Sprites/Square");
    //        image.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(16, 16));
    //    }
    //    else
    //    {
    //        SetView(fruitType, plateState);
    //    }
    //}

    public void SetView(Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnBeginDragEvent?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnDragEvent?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnEndDragEvent?.Invoke(eventData);
    }
}
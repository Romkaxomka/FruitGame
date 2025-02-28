using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : Button
{
    public static event Action<CustomButton> OnClick;

    private bool _pressed = false;
    private float longPressDuration = 0.5f;
    private float timeToLongPressDuration = 0;
    public bool HaveLongPressAction = false;
    private Tweener _currentTweener;

    public Action OnLongPress;

    public bool IsScalable = true;

    public void OnPointerDownLogic()
    {
        if (!IsScalable) return;

        _currentTweener = transform.DOScale(new Vector3(0.95f, 0.95f, 1), 0.1f);
    }

    public void OnPointerUpLogic()
    {
        if (!IsScalable) return;

        _currentTweener = gameObject.transform.DOScale(Vector3.one, 0.1f);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (!IsInteractable()) return;

        _pressed = true;
        _currentTweener?.Kill();
        OnPointerDownLogic();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (!IsInteractable()) return;

        _currentTweener?.Kill();
        _pressed = false;
        OnPointerUpLogic();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        _currentTweener?.Kill();
        OnPointerUpLogic();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (_pressed)
        {
            _currentTweener?.Kill();
            OnPointerDownLogic();
        }
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        _currentTweener?.Kill();
        OnPointerUpLogic();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        if (!IsInteractable()) return;
        OnClick?.Invoke(this);
    }

    private void Update()
    {
        if (!HaveLongPressAction) return;
        if (!_pressed) return;
        timeToLongPressDuration += Time.deltaTime;
        if (!(timeToLongPressDuration >= longPressDuration)) return;
        _pressed = false;
        timeToLongPressDuration = 0;
        OnLongPress?.Invoke();
    }
}
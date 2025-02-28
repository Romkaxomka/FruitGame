using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FruitPlate
{
    public int x { get; private set; }
    public int y { get; private set; }

    public EFruitType fruitType { get; private set; }
    public EPlateState plateState { get; private set; }

    public int FruitHash { get; set; } = -1;
    public Vector2 currentPos { get; private set; }

    public FruitPlateView View { get; private set; }

    public FruitPlate(int x, int y, FruitPlateView view, Vector2 pos)
    {
        this.x = x;
        this.y = y;

        View = view;
        View.OnBeginDragEvent.AddListener(a => GamePanel.instance.OnBeginDrag(this, a));
        View.OnDragEvent.AddListener(a => GamePanel.instance.OnDrag(this, a));
        View.OnEndDragEvent.AddListener(a => GamePanel.instance.OnEndDrag(this, a));

        currentPos = pos;
        View.rectTransform.anchoredPosition = pos;
    }

    public async UniTask MoveTo(Vector2 pos, float duration)
    {
        await View.MoveTo(pos, duration);
    }

    public async UniTask ReturnPos(float duration)
    {
        await View.MoveTo(currentPos, duration);
    }

    public void SetFruits(EFruitType fruitType, EPlateState plateState)
    {
        this.fruitType = fruitType;
        this.plateState = plateState;

        FruitHash = (int)fruitType;

        View.SetView(GamePanel.instance.fruitSpriteManager.GetSprite(fruitType, plateState));
    }

    public void SetNullHash()
    {
        FruitHash = -1;
    }

    public async UniTask Collect()
    {
        if (FruitHash >= 0)
        {
            SetNullHash();
            await View.Collect();
        }
    }
}

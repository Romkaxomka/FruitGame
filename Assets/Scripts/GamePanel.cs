using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class GamePanel : MonoBehaviour
{
    public static GamePanel instance;

    public FruitManagerScriptableObject fruitSpriteManager;
    public FruitPlateView prefabFruitPlate;
    public GameObject gridLayoutObject;

    private int rowsCount = 6;
    private int columnsCount = 5;

    Canvas canvas;
    FruitPlateView forDrag;

    MergeLogic mergeLogic;

    private void Awake()
    {
        instance = this;

        fruitSpriteManager.Init();
        canvas = GetComponentInParent<Canvas>();
        mergeLogic = new MergeLogic(columnsCount, rowsCount);
    }

    private void Start()
    {
        forDrag = Instantiate(prefabFruitPlate, transform);
        forDrag.transform.SetAsLastSibling();
        forDrag.canvasGroup.alpha = .6f;
        forDrag.canvasGroup.blocksRaycasts = false;
        forDrag.gameObject.SetActive(false);

        mergeLogic.InitGame();
        mergeLogic.GenGame(Random.Range(0, int.MaxValue)).Forget();
    }

    public FruitPlateView InstantiateFruitPlate()
    {
        return Instantiate(prefabFruitPlate, gridLayoutObject.transform);
    }

    FruitPlate draggedOn = null;

    public const float LeftRightOffset = 50;

    public const float UpMinOffset = 35;
    public const float UpMaxOffset = 175;

    void SetDraggedOn(FruitPlate value)
    {
        if (draggedOn != null)
        {
            draggedOn.ReturnPos(.25f).Forget();
        }

        draggedOn = value;

        if (draggedOn != null)
        {
            draggedOn.MoveTo(draggedFruit.View.rectTransform.anchoredPosition, .25f).Forget();
        }

        if (draggedOn != null)
        {
            Debug.Log($"draggedOn: {draggedOn?.x} {draggedOn?.y}");
        }
        else
        {
            Debug.Log($"draggedOn: NULL");
        }
    }

    void Update()
    {
        if(isDraging)
        {
            var dragOffset = forDrag.rectTransform.anchoredPosition - startDragPos;

            FruitPlate draggedOnCurrent = null;

            if (dragOffset.x is < LeftRightOffset and > -LeftRightOffset && dragOffset.y is < UpMaxOffset and > UpMinOffset && draggedFruit.y + 1 < rowsCount)
            {
                draggedOnCurrent = mergeLogic.FruitPlates[draggedFruit.x, draggedFruit.y + 1];
            }
            else if (dragOffset.x is < LeftRightOffset and > -LeftRightOffset && dragOffset.y is < -UpMinOffset and > -UpMaxOffset && draggedFruit.y - 1 >= 0)
            {
                draggedOnCurrent = mergeLogic.FruitPlates[draggedFruit.x, draggedFruit.y - 1];
            }
            else if (dragOffset.y is < LeftRightOffset and > -LeftRightOffset && dragOffset.x is < UpMaxOffset and > UpMinOffset && draggedFruit.x + 1 < columnsCount)
            {
                draggedOnCurrent = mergeLogic.FruitPlates[draggedFruit.x + 1, draggedFruit.y];
            }
            else if (dragOffset.y is < LeftRightOffset and > -LeftRightOffset && dragOffset.x is < -UpMinOffset and > -UpMaxOffset && draggedFruit.x - 1 >= 0)
            {
                draggedOnCurrent = mergeLogic.FruitPlates[draggedFruit.x - 1, draggedFruit.y];
            }

            if (draggedOn != draggedOnCurrent)
            {
                SetDraggedOn(draggedOnCurrent);
            }
            
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            mergeLogic.GenGame(Random.Range(0, int.MaxValue)).Forget();
        }
    }

    bool isDraging = false;
    int pointerId = -1;
    FruitPlate draggedFruit;
    Vector2 startDragPos;

    public void OnBeginDrag(FruitPlate fruitPlate, PointerEventData eventData)
    {
        if (!mergeLogic.CanPlay) return;
        if (isDraging) return;
        isDraging = true;
        pointerId = eventData.pointerId;

        forDrag.rectTransform.position = fruitPlate.View.rectTransform.position;
        startDragPos = forDrag.rectTransform.anchoredPosition;
        forDrag.SetView(fruitSpriteManager.GetSprite(fruitPlate.fruitType, fruitPlate.plateState));
        forDrag.gameObject.SetActive(true);

        draggedFruit = fruitPlate;
        draggedFruit.View.canvasGroup.alpha = 0f;
        SetDraggedOn(null);

        Debug.Log($"draggedFruit: {draggedFruit.x} {draggedFruit.y}");
    }

    public void OnDrag(FruitPlate fruitPlate, PointerEventData eventData)
    {
        if (!mergeLogic.CanPlay) return;
        if (!isDraging) return;
        if(eventData.pointerId != pointerId) return;

        forDrag.rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    async UniTask TryMerge(FruitPlate fruitPlateA, FruitPlate fruitPlateB)
    {
        draggedFruit.View.canvasGroup.alpha = 1f;

        if (await mergeLogic.TryMerge(draggedOn, draggedFruit))
        {

        }
        else
        {
            draggedOn.ReturnPos(0).Forget();
        }
    }

    public void OnEndDrag(FruitPlate fruitPlate, PointerEventData eventData)
    {
        if (!mergeLogic.CanPlay) return;
        if (!isDraging) return;
        if (eventData.pointerId != pointerId) return;

        forDrag.gameObject.SetActive(false);

        if (draggedOn != null)
        {
            TryMerge(draggedOn, draggedFruit).Forget();
            SetDraggedOn(null);
        }
        else
        {
            draggedFruit.View.canvasGroup.alpha = 1f;
        }

        isDraging = false;
    }
}
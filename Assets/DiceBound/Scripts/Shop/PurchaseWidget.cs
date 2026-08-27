using System;
using DG.Tweening;
using KCoreKit;
using UnityEngine;
using Random = System.Random;

namespace DiceBound.Shop
{
    public class PurchaseWidget : WidgetBase
    {
        [SerializeField] private ImageWidget preview;
        [SerializeField] private BoosterPackWidget boosterPack;
        [SerializeField] private TextWidget costText;
        private int _cost;
        private BoosterPackDataTableRow _data;
        public Action<PurchaseWidget> onDragBeginAction;
        private Vector2 _initialPosition;
        private float _spiralOffset;

        public void Awake()
        {
            boosterPack.onPointerDownAction += _ => onDragBeginAction.Invoke(this);
            _initialPosition = transform.position;
            _spiralOffset = UnityEngine.Random.Range(-100f,100f);
        }

        public void Update()
        {
            var delta = Time.time + _spiralOffset;
            var rad = 0.2f;
            transform.position = _initialPosition + new Vector2(Mathf.Cos(delta) * rad, Mathf.Sin(delta) * rad);
        }

        public void Setup(BoosterPackDataTableRow data)
        {
            _data = data;
            _cost = data.cost;
            boosterPack.Setup(data);
            costText.SetText(_cost.ToString());
            preview.SetSprite(data.texture.ToSprite());
        }

        public int GetCost()
        {
            return _cost;
        }

        public BoosterPackDataTableRow GetData()
        {
            return _data;
        }

        public BoosterPackWidget GetBoosterPack()
        {
            return boosterPack;
        }
    }
}
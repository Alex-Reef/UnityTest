using System;
using LevelsSystem.SO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LevelsSystem.Behaviors
{
    [RequireComponent(typeof(Button)), DisallowMultipleComponent]
    public class LevelItem : MonoBehaviour
    {
        [SerializeField] private Image symbol;

        private int _levelId;
        public event Action<int> OnLevelSelected;
    
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => OnLevelSelected?.Invoke(_levelId));
        }

        public void Init(int levelId, LevelConfig levelConfig, Color color)
        {
            _levelId = levelId;
            symbol.overrideSprite = levelConfig.FullImage;
            symbol.SetNativeSize();
            symbol.color = color;
        }
    }
}

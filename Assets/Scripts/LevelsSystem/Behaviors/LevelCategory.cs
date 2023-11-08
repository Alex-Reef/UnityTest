using System;
using System.Collections;
using LevelsSystem.SO;
using TMPro;
using UnityEngine;

namespace LevelsSystem.Behaviors
{
    public class LevelCategory : MonoBehaviour
    {
        [SerializeField] private TMP_Text categoryName;
        [SerializeField] private RectTransform scrollTransform;
        [SerializeField] private LevelItem levelItemPrefab;
        
        private Color[] _itemsColors;
        private int _categoryId;
        public event Action<int, int> OnLevelSelected; 
    
        public void Init(int categoryId, LevelsListConfig levelsConfig, Color[] itemsColors)
        {
            _itemsColors = itemsColors;
            _categoryId = categoryId;
            StartCoroutine(LoadLevels(levelsConfig));
        }

        private IEnumerator LoadLevels(LevelsListConfig levelsConfig)
        {
            categoryName.text = levelsConfig.CategoryName;
        
            int levelsCount = levelsConfig.Levels.Length;
            for (int i = 0; i < levelsCount; i++)
            {
                var level = Instantiate(levelItemPrefab, scrollTransform);
                level.Init(i, levelsConfig.Levels[i], _itemsColors[i]);
                level.OnLevelSelected += OnLevelFromCategorySelected;
                yield return null;
            }
        }

        private void OnLevelFromCategorySelected(int levelId)
        {
            OnLevelSelected?.Invoke(_categoryId, levelId);
        }
    }
}

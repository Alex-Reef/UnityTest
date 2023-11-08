using System.Collections;
using LevelsSystem.SO;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace LevelsSystem.Behaviors
{
    public class LevelsLoader : MonoBehaviour
    {
        [SerializeField] private RectTransform categoriesTransform;
        [SerializeField] private LevelCategory levelCategoryPrefab;
        [SerializeField] private LevelsListConfig[] levelsConfigs;
        [SerializeField] private Color[] itemsColors;
        [SerializeField] private GameObject loadingBlocker;
        
        private void Awake()
        {
            loadingBlocker.SetActive(true);
            Observable.FromMicroCoroutine(Load).Subscribe();
        }

        private IEnumerator Load()
        {
            int categoryCount = levelsConfigs.Length;
            for (int i = 0; i < categoryCount; i++)
            {
                var category = Instantiate(levelCategoryPrefab, categoriesTransform);
                category.Init(i, levelsConfigs[i], itemsColors);
                category.OnLevelSelected += OnLevelSelected;
                yield return null;
            }

            yield return new WaitForEndOfFrame();
            loadingBlocker.SetActive(false);
        }

        private void OnLevelSelected(int categoryId, int levelId)
        {
            PlayerPrefs.SetInt(Constants.CategorySaveKey, categoryId);
            PlayerPrefs.SetInt(Constants.LevelSaveKey, levelId);
            SceneManager.LoadSceneAsync(1);
        }
    }
}

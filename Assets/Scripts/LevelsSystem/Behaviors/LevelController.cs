using System;
using System.Collections;
using System.Collections.Generic;
using AlexDevTools.Models;
using AlexDevTools.SoundSystem;
using DG.Tweening;
using Drawing;
using LevelsSystem.SO;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace LevelsSystem.Behaviors
{
    [DisallowMultipleComponent]
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private LevelsListConfig[] levelsListConfigs;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image symbolImage;
        [SerializeField] private DrawingController drawingController;
        [SerializeField] private Color[] fillColors;

        private LevelConfig _levelConfig;

        private int _currentParts;
        private List<Image> _parts;
        private List<Image> _tips;

        private void Awake()
        {
            symbolImage.gameObject.SetActive(false);
            _parts = new List<Image>();
            _tips = new List<Image>();
            InitListening();
            Init();
        }

        private void Init()
        {
            InitConfig();
            
            DisablePoll(_parts);
            DisablePoll(_tips);

            Load();
        }
        
        private void InitListening()
        {
            drawingController.PlaySound += () => SoundController.Instance.Play(_levelConfig.TaskAudio);
            drawingController.ImageFilled += () =>
            {
                if (_currentParts < _parts.Count - 1)
                {
                    _currentParts++;
                    drawingController.Init(_parts[_currentParts], _levelConfig.DrawRoads[_currentParts], _tips[_currentParts]);
                }
                else
                {
                    SoundController.Instance.Play(SoundType.Win);
                    LoadNextLevel();
                    Init();
                }
            };
        }

        private void InitConfig()
        {
            var levelInfo = GetCurrentLevel();
            int categoryId = levelInfo.Item1;
            int levelId = levelInfo.Item2;
            
            if (categoryId >= levelsListConfigs.Length) categoryId = 0;
            if (levelId >= levelsListConfigs[categoryId].Levels.Length) levelId = 0;
            
            _levelConfig = levelsListConfigs[categoryId].Levels[levelId];
        }

        private IEnumerator InitParts()
        {
            var partsCount = _levelConfig.Parts.Length;
            for (int i = 0; i < partsCount; i++)
            {
                var part = InstantiateImage(i, _levelConfig.Parts[i], _parts);
                part.fillAmount = 0;
                part.color = fillColors[GetCurrentLevel().Item2];
                part.gameObject.SetActive(true);
                yield return null;
            }
        }

        private IEnumerator InitHints()
        {
            var hintsCount = _levelConfig.Hints.Length;
            for (int i = 0; i < hintsCount; i++)
            {
                var tips = InstantiateImage(i, _levelConfig.Hints[i], _tips);
                tips.gameObject.SetActive(false);
                yield return null;
            }
        }

        private IEnumerator InitDrawSymbol()
        {
            var color = Color.white;
            color.a = 0;
            symbolImage.color = color;
            
            symbolImage.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(1);
            
            symbolImage.color = color;
            symbolImage.overrideSprite = _levelConfig.FullImage;
            symbolImage.SetNativeSize();
            color.a = 0.15f;
            symbolImage.DOColor(color, 1);
        }

        private Image InstantiateImage(int id, KeyValue<Sprite, Image> content, List<Image> pool)
        {
            Image image;
            if (id >= pool.Count)
            {
                image = Instantiate(content.value, canvas.transform);
                pool.Add(image);
            }
            else
            {
                image = pool[id];
            }

            image.overrideSprite = content.key;
            image.SetNativeSize();

            return image;
        }

        private void DisablePoll(List<Image> imagesPool)
        {
            for(int i = 0; i < imagesPool.Count; i++)
                imagesPool[i].gameObject.SetActive(false);
        }

        private Tuple<int, int> GetCurrentLevel()
        {
            var category = PlayerPrefs.GetInt(Constants.CategorySaveKey);
            var level = PlayerPrefs.GetInt(Constants.LevelSaveKey);

            return new Tuple<int, int>(category, level);
        }

        private void LoadNextLevel()
        {
            var levelInfo = GetCurrentLevel();
            if (levelInfo.Item2 < levelsListConfigs[levelInfo.Item1].Levels.Length)
                PlayerPrefs.SetInt(Constants.LevelSaveKey, levelInfo.Item2 + 1);
            else 
                PlayerPrefs.SetInt(Constants.LevelSaveKey, 0);
        }
    
        private void Load()
        {
            _currentParts = 0;

            Observable.FromMicroCoroutine(InitDrawSymbol)
                .SelectMany(InitParts)
                .SelectMany(InitHints)
                .SelectMany(InitDrawing)
                .Subscribe();
        }

        private IEnumerator InitDrawing()
        {
            SoundController.Instance.Play(_levelConfig.TaskAudio);
            yield return new WaitForSeconds(2);
            
            drawingController.Init(_parts[0], _levelConfig.DrawRoads[0], _tips[0]);
        }
    }
}

using System;
using System.Collections;
using AlexDevTools.TimerSystem;
using DG.Tweening;
using LevelsSystem.Models;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Drawing
{
    public class DrawingController : MonoBehaviour
    {
        [SerializeField] private RectTransform pointer;
        [FormerlySerializedAs("tipsPointer")] [SerializeField] private RectTransform hintPointer;

        #region Main
        private bool _loaded;
        private Camera _camera;

        #endregion
        
        #region Actions
        public event Action ImageFilled;
        public event Action PlaySound;
        #endregion

        #region Hints
        private Image _hintImage;
        private Color _hintColor;
        private int _showHintTimerId;
        private int _hintLevel;
        private Tweener _hintTween;
        private const int HintsTimerDelay = 7;
        #endregion

        #region Drawing
        private Image _currentImage;
        private bool _canDraw;
        private Vector3[] _drawPath;
        private int _nextPointId;
        
        private const float DistanceToPoint = 0.2f;
        private const float StartPointDistance = 0.4f;

        private float _distanceToPoint;
        private float _fullDistance;
        private float _elapsedDistance;
        #endregion
        
        
        private void Awake()
        {
            _hintColor = new Color(256, 256, 256, 0);
            _camera = Camera.main;
        }

        private IEnumerator Initializing(Image image, DrawPath drawPath, Image tips)
        {
            _distanceToPoint = Vector3.Distance(drawPath.Points[0], drawPath.Points[1]);
            
            _drawPath = GetPathPoints(drawPath);
            pointer.transform.position = _drawPath[0];
            _currentImage = image;
            
            hintPointer.gameObject.SetActive(false);
            
            if (_hintImage != null)
            {
                _hintColor.a = 0;
                _hintImage.color = _hintColor;
                _hintImage.gameObject.SetActive(false);
            }

            _hintImage = tips;
            _hintImage.color = _hintColor;
            _hintColor.a = 0.9f;
            _hintImage.gameObject.SetActive(true);
            
            pointer.GetComponent<Image>().DOColor(Color.white, 1f);
            _hintImage.DOColor(_hintColor, 1f).OnComplete(() =>
            {
                _showHintTimerId = TimersController.Instance.StartTimer(HintsTimerDelay, true, TimerElapsed);
            
                _nextPointId = 0;
                _elapsedDistance = 0;
                GetFullDistance();
                _loaded = true;
            });

            yield return null;
        }

        public void Init(Image image, DrawPath drawPath, Image tips)
        {
            _loaded = false;
            StartCoroutine(Initializing(image, drawPath, tips));
        }

        private Vector3[] GetPathPoints(DrawPath drawPath)
        {
            var drawPathLength = drawPath.Points.Length;
            _drawPath = new Vector3[drawPathLength];
            for (int i = 0; i < drawPathLength; i++)
                _drawPath[i] = drawPath.Points[i];

            return _drawPath;
        }

        private void TimerElapsed()
        {
            _hintLevel++;
            if(_hintLevel == 1) PlaySound?.Invoke();
            if(_hintLevel == 2) EnableHints(true);
        }

        private void EnableHints(bool enable)
        {
            hintPointer.gameObject.SetActive(enable);
            
            if(_hintTween != null)
                _hintTween.Kill();
            
            if (enable)
            {
                var startPos = _drawPath[0];
                hintPointer.transform.position = startPos;
                _hintTween = hintPointer.DOPath(_drawPath, _drawPath.Length * 1.4f).OnComplete(
                    () => hintPointer.gameObject.SetActive(false));
            }
        }

        private void GetFullDistance()
        {
            _fullDistance = 0;
            for (int i = 1; i < _drawPath.Length; i++)
            {
                _fullDistance += Vector3.Distance(_drawPath[i], _drawPath[i - 1]);
            }
        }

        private void Update()
        {
            if (Input.touchCount == 0) return;
            
            TimersController.Instance.ResetTimer(_showHintTimerId);
            EnableHints(false);
            _hintLevel = 0;

            if (!_loaded) return;
            
            if (_drawPath == null) return;

            if (_nextPointId == -1) return;
        
            if (!_canDraw)
            {
                if (AroundPoint(_drawPath[0], GetMousePosition(), StartPointDistance))
                {
                    _canDraw = true;
                    CanSetNextPoint();
                }

                return;
            }
        
            if (AroundPoint(_drawPath[_nextPointId], GetMousePosition(), DistanceToPoint))
            {
                if (!CanSetNextPoint())
                {
                    FillImage();
                    return;
                }
            }
            
            UpdateFill();
            UpdatePointerPosition();
        }

        private void FillImage()
        {
            TimersController.Instance.StopTimer(_showHintTimerId);
            var color = Color.white;
            color.a = 0;
            pointer.GetComponent<Image>().DOColor(color, 0.5f);
            _currentImage.fillAmount = 1;
            _canDraw = false;
            _nextPointId = -1;
            _hintLevel = 0;
            ImageFilled?.Invoke();
        }

        private bool OnRightWay()
        {
            var currentPosition = GetMousePosition();

            if (_nextPointId <= _drawPath.Length - 1)
            {
                var nextPointPosition = _drawPath[_nextPointId];
                var distanceToPoint = Vector3.Distance(currentPosition, nextPointPosition);
                if (distanceToPoint <= _distanceToPoint)
                {
                    _distanceToPoint = distanceToPoint;
                    return true;
                }
            }

            return false;
        }

        private void UpdatePointerPosition()
        {
            var distanceToPoint = Vector3.Distance(GetMousePosition(), _drawPath[_nextPointId - 1]);
            var pointA = _drawPath[_nextPointId - 1];
            var pointB = _drawPath[_nextPointId];
            float t = Mathf.Clamp01(distanceToPoint / Vector3.Distance(pointA, pointB));
            
            pointer.position = Vector3.Lerp(pointA, pointB, t);
        }

        private void UpdateFill()
        {
            if(OnRightWay())
            {
                _currentImage.fillAmount = GetFillPercent();
            }
        }

        private float GetFillPercent()
        {
            var distanceToPoint = Vector3.Distance(GetMousePosition(), _drawPath[_nextPointId - 1]);
            var distance = _elapsedDistance + distanceToPoint;
            float normalizedDistance = distance  / _fullDistance;
            normalizedDistance = Mathf.Clamp01(normalizedDistance);
            return normalizedDistance;
        }

        private bool CanSetNextPoint()
        {
            if (_nextPointId < _drawPath.Length - 1)
            {
                var distance = Vector3.Distance(_drawPath[_nextPointId], _drawPath[_nextPointId + 1]);
                if (_nextPointId > 0)
                {
                    _elapsedDistance += distance;
                }
                
                _distanceToPoint = distance;
                _nextPointId++;
                
                return true;
            }
            
            return false;
        }

        private bool AroundPoint(Vector3 position1, Vector3 position2, float distance)
        {
            return Vector3.Distance(position1, position2) < distance;
        }

        private Vector3 GetMousePosition()
        {
            var position = _camera.ScreenToWorldPoint(Input.mousePosition);
            position.z = 0;
            return position;
        }
    }
}

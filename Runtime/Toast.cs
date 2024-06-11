using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DarkNaku.Toast
{
    public interface IToast
    {
        void OnClickToastView(ToastView toastView);
    }
    
    [RequireComponent(typeof(Canvas))]
    public class Toast : MonoBehaviour, IToast
    {
        [SerializeField] private ToastView _toastView;
        [SerializeField] private bool _dismissByClick;
        [SerializeField] private bool _useYieldSeat = true;
        [SerializeField, Min(1)] private int _maxVisibleCount = 1;
        [SerializeField] private RectTransform _exposureLocation;
        
        public static Toast Instance
        {
            get
            {
                if (_isQuitting) return null;

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        var instances = FindObjectsByType<Toast>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                        if (instances.Length > 0)
                        {
                            _instance = instances[0];

                            for (int i = 1; i < instances.Length; i++)
                            {
                                Debug.LogWarningFormat("[Toast] Instance Duplicated - {0}", instances[i].name);
                                Destroy(instances[i]);
                            }
                        }
                        else
                        {
                            _instance = new GameObject($"[Toast]").AddComponent<Toast>();
                        }
                    }

                    return _instance;
                }
            }
        }
        
        private static readonly object _lock = new();
        private static Toast _instance;
        private static bool _isQuitting;

        public static UnityEvent<ToastView> OnClickToast => Instance._onClickToast;
        
        private Canvas ToastCanvas => _toastCanvas ??= GetComponent<Canvas>();
        private UnityEvent<ToastView> _onClickToast = new UnityEvent<ToastView>();
        
        private Canvas _toastCanvas;
        private HashSet<ToastView> _toastViews = new();
        private Queue<ToastView> _toastQueue = new();
        private List<ToastView> _toastViewsInUse = new();
        
        public static T Show<T>() where T : ToastView => Instance._Show<T>();

        public void OnClickToastView(ToastView toastView)
        {
            if (_dismissByClick)
            {
                toastView.Dismiss();
            }
            
            _onClickToast.Invoke(toastView);
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Debug.LogWarningFormat("[Toast] Duplicated - {0}", name);
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            _isQuitting = true;
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void LateUpdate()
        {
            if (_toastQueue.Count == 0) return;
            if (_toastViewsInUse.Count >= _maxVisibleCount) return;

            if (_useYieldSeat)
            {
                for (int i = 0; i < _toastViewsInUse.Count; i++)
                {
                    _toastViewsInUse[i].Move();
                }
            }
            
            var toastView = _toastQueue.Dequeue();
            
            _toastViewsInUse.Add(toastView);
            
            StartCoroutine(toastView.Show(() => _toastViewsInUse.Remove(toastView)));
        }
            
        private T _Show<T>() where T : ToastView
        {
            var toastView = GetToastView();
            
            _toastQueue.Enqueue(toastView);
            
            if (_exposureLocation != null)
            {
                toastView.RT.anchoredPosition = _exposureLocation.anchoredPosition;
            }
            else
            {
                toastView.transform.position = transform.position;
            }
            
            return toastView as T;
        }

        private ToastView GetToastView()
        {
            ToastView toastView = null;
            
            foreach (var item in _toastViews)
            {
                if (item.IsUsable && _toastQueue.Contains(item) == false)
                {
                    toastView = item;
                    break;
                }
            }

            if (toastView == null)
            {
                toastView = Instantiate(_toastView, transform);
                
                toastView.Initialize(this);
                
                _toastViews.Add(toastView);
            }

            return toastView;
        }
    }
}
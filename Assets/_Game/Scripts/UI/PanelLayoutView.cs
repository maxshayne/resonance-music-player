using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.UI
{
    public class PanelLayoutView : MonoBehaviour
    {
        private const float SlideDuration = 0.45f;
        private const float FadeDuration = 0.3f;
        
        private const float FadeDrop = 40f;

        [SerializeField] private RectTransform _dialogue;
        [SerializeField] private CanvasGroup _dialogueGroup;
        [SerializeField] private RectTransform _player;
        [SerializeField] private CanvasGroup _playerGroup;
        [SerializeField] private Button _hidePlayer;
        [SerializeField] private float _gap = 18f;
        [SerializeField] private float _sideMargin = 60f;
        [SerializeField] private float _maxHeightShare = 0.36f;

        private CanvasStateController _state;
        private RectTransform _self;
        private RectTransform _canvas;
        private DisposableBag _subscriptions;
        private Tween _dialogueSlide;
        private Tween _dialogueFade;
        private Tween _playerSlide;
        private Tween _tabSlide;
        private float _width;
        private float _height;
        private bool _animate;
        private bool _dirty;

        [Inject]
        public void Construct(CanvasStateController state) => _state = state;

        private void Awake()
        {
            _self = (RectTransform)transform;
            _canvas = (RectTransform)_self.parent;
        }

        private void Start()
        {
            _state.PlayerVisible.Subscribe(_ => Place(_animate)).AddTo(ref _subscriptions);
            _state.DialogueVisible.Subscribe(visible => Fade(visible, _animate)).AddTo(ref _subscriptions);

            _animate = true;
        }

        private void OnEnable()
        {
            _hidePlayer.onClick.AddListener(HidePlayer);
        }

        private void OnDisable()
        {
            _hidePlayer.onClick.RemoveListener(HidePlayer);
        }

        private void OnDestroy()
        {
            _subscriptions.Dispose();

            _dialogueSlide?.Kill();
            _dialogueFade?.Kill();
            _playerSlide?.Kill();
            _tabSlide?.Kill();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.D))
                _state.ToggleDialogue();

            if (Input.GetKeyDown(KeyCode.M))
                _state.TogglePlayer();
        }
        
        private void OnRectTransformDimensionsChange() => _dirty = true;

        private void LateUpdate()
        {
            if (!_dirty)
                return;

            _dirty = false;
            if (Mathf.Approximately(_self.rect.width, _width) && Mathf.Approximately(_canvas.rect.height, _height))
                return;

            Place(animated: false);
        }

        private void HidePlayer() => _state.PlayerVisible.Value = false;

        private void ShowPlayer() => _state.PlayerVisible.Value = true;

        private void Place(bool animated)
        {
            _width = _self.rect.width;
            _height = _canvas.rect.height;

            float span = _dialogue.rect.width + _gap + _player.rect.width;
            float widthFit = (_width - _sideMargin * 2f) / span;
            float heightFit = _height * _maxHeightShare / _dialogue.rect.height;
            float fit = Mathf.Clamp(Mathf.Min(widthFit, heightFit), 0.1f, 1f);
            _self.localScale = new Vector3(fit, fit, 1f);
            
            float edge = _width * 0.5f / fit;
            bool visible = _state.PlayerVisible.CurrentValue;
            
            Slide(ref _dialogueSlide, _dialogue, visible ? (_dialogue.rect.width - span) * 0.5f : 0f, Ease.InOutCubic, animated);
            Slide(ref _playerSlide, _player, visible ? (span - _player.rect.width) * 0.5f : edge + _player.rect.width * 0.5f, visible ? Ease.OutCubic : Ease.InCubic, animated);

            _playerGroup.blocksRaycasts = visible;
        }

        private void Fade(bool visible, bool animated)
        {
            _dialogueFade?.Kill();
            _dialogueFade = null;

            _dialogueGroup.blocksRaycasts = visible;

            float alpha = visible ? 1f : 0f;
            float y = visible ? 0f : -FadeDrop;

            if (!animated)
            {
                _dialogueGroup.alpha = alpha;
                _dialogue.anchoredPosition = new Vector2(_dialogue.anchoredPosition.x, y);
                return;
            }

            _dialogueFade = DOTween.Sequence()
                .Join(_dialogueGroup.DOFade(alpha, FadeDuration).SetEase(Ease.OutQuad))
                .Join(_dialogue.DOAnchorPosY(y, FadeDuration).SetEase(Ease.OutQuad))
                .SetUpdate(true);
        }
        
        private static void Slide(ref Tween tween, RectTransform panel, float x, Ease ease, bool animated)
        {
            tween?.Kill();
            tween = null;

            if (!animated)
            {
                panel.anchoredPosition = new Vector2(x, panel.anchoredPosition.y);
                return;
            }

            tween = panel.DOAnchorPosX(x, SlideDuration).SetEase(ease).SetUpdate(true);
        }
    }
}

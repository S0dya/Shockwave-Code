using Gameplay.Shockwave2048.Board;
using PT.Logic.Ads;
using PT.Logic.Dependency.Signals;
using PT.Tools.Helper;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gameplay.Shockwave2048.Undo
{
    public class UndoController : MonoBehaviour
    {
        [SerializeField] private Button undoButton;
        [SerializeField] private CanvasGroup undoCanvasGroup;
        [SerializeField] private TextMeshProUGUI amountText;
        [Space]        
        [SerializeField] private GameObject[] adObjects;
        [SerializeField] private Button adButton;
        
        [Inject] private GameplayController _gameplay;
        [Inject] private AdsManager _ads;
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardState _boardState;

        private readonly UndoModel _model = new();

        private readonly CompositeDisposable _cd = new();

        private void Awake()
        {
            _signalBus.Subscribe<PlayerTurnSignal>(OnPlayerTurn);
            _signalBus.Subscribe<GameTurnSignal>(OnGameTurn);
            
            undoButton.onClick.AddListener(TryUndo);
            adButton.onClick.AddListener(WatchAd);

            _model.Init();
            _model.Amount
                .Subscribe(UpdateView)
                .AddTo(_cd);
            
            foreach (var obj in adObjects) obj.SetActive(false);
        }

        private void UpdateView(int amount)
        {
            amountText.text = amount.ToString();

            bool hasAmounts = amount > 0;
            
            undoButton.interactable = hasAmounts;
            foreach (var obj in adObjects) obj.SetActive(!hasAmounts);
        }

        private void TryUndo()
        {
            if (_boardState.GameStep <= 1 || !_model.TryUse()) return;
            _gameplay.TryUndoTurn();
        }

        private void WatchAd()
        {
            _ads.ShowRewardAd(() =>
            {
                _model.AddFromAd();
            });
        }
        
        private void OnPlayerTurn()
        {
            undoCanvasGroup.interactable = true;
        }
        private void OnGameTurn()
        {
            undoCanvasGroup.interactable = false;
        }

        private void OnDestroy()
        {
            _cd.Dispose();
        }
    }
}
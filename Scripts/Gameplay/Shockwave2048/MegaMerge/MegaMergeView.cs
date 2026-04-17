using PT.Logic.Dependency.Signals;
using PT.Tools.Helper;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gameplay.Shockwave2048.MegaMerge
{
    public class MegaMergeView : MonoBehaviour
    {
        [SerializeField] private Image barFill;
        [SerializeField] private SwapPointerHint swapPointerHint;
        [SerializeField] private GameObject[] readyObjs;
        [Space]
        [SerializeField] private Color chargingColor = Color.white;
        [SerializeField] private Color readyColor = Color.gold;
    
        [Inject] private MegaMergeModel _model;
        [Inject] private SignalBus _signalBus;
        [Inject] private GameEffectsController _gameEffectsController;

        private bool _isReadyPlaying = false;
        private bool _shownPointer = false; //rewrite into saved value, if analytics show its needed

        private void Awake()
        {
            ToggleReady(false);
        }
        
        private void Start()
        {
            _model.Charge
                .Subscribe(SetCharge)
                .AddTo(this);
            
            barFill.color = chargingColor;
        }

        private void SetCharge(float val)
        {
            barFill.fillAmount = val;

            if (barFill.fillAmount >= 0.99f)
            {
                ShowReady();
            }
            else
            {
                HideReady();
            }
        }

        private void ShowReady()
        {
            if (_isReadyPlaying) return;
                
            _gameEffectsController.PlayMegaMergeReady();
             ToggleReady(true);
            
            if (!_shownPointer) _shownPointer = true;
        }
        private void HideReady()
        {
            if (!_isReadyPlaying) return;
             
             _gameEffectsController.StopMegaMergeReady();
             ToggleReady(false);
        }

        private void ToggleReady(bool toggle)
        {
            barFill.color = toggle ? readyColor : chargingColor;
            
            _isReadyPlaying = toggle;
            swapPointerHint.SetActive(!_shownPointer && toggle);
            readyObjs.SetActive(toggle);
        }
    }
}
using PT.UI.Effects;
using UnityEngine;

namespace Gameplay.Shockwave2048.Board
{
    public class MergeTextPopUp : TextPopUp
    {
        [Header("Merge Power")]
        [SerializeField] private int baseFontSize = 36;
        [SerializeField] private int fontStep = 6;

        [SerializeField] private float scaleStep = 0.06f;
        [SerializeField] private float moveStep = 0.08f;

        [SerializeField] private Color lowColor = Color.white;
        [SerializeField] private Color midColor = Color.yellow;
        [SerializeField] private Color highColor = new Color(1f, 0.55f, 0f);
        [SerializeField] private int midStep = 3;
        [SerializeField] private int highStep = 6;

        private int _mergeStep;

        private float _initialScaleUp;

        private void Start()
        {
            _initialScaleUp = scaleUp;
        }
        
        public void SetMergeStep(int mergeStep)
        {
            _mergeStep = mergeStep;
        }

        protected override void ApplyStyle()
        {
            var power = Mathf.Max(_mergeStep - 1, 0);

            text.fontSize = baseFontSize + power * fontStep;
            text.color = EvaluateColor(_mergeStep);

            scaleUp = _initialScaleUp * (1f + power * scaleStep);
        }

        private Color EvaluateColor(int step)
        {
            if (step <= midStep)
                return Color.Lerp(lowColor, midColor, (float)(step - 1) / midStep);

            if (step <= highStep)
                return Color.Lerp(
                    midColor,
                    highColor,
                    (float)(step - midStep) / (highStep - midStep)
                );

            return highColor;
        }
    }
}
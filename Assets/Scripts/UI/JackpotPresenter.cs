using System.Collections;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// The jackpot celebration: the jackpot title and amount flash, the marquee lamps chase and the coin
    /// fountains keep throwing coins until the celebration is cancelled.
    /// </summary>
    public class JackpotPresenter : MonoBehaviour
    {
        [SerializeField] private TextMesh title;
        [SerializeField] private TextMesh amount;
        [SerializeField] private Lamps[] lamps;
        [SerializeField] private CoinFountain[] fountains;
        [SerializeField] private float fountainInterval = 0.5f;
        [SerializeField] private float fountainPause = 2f;

        private Coroutine flash;
        private Coroutine fountain;
        private Color titleColor;
        private Color amountColor;
        private bool active;

        public void Setup(TextMesh jackpotTitle, TextMesh jackpotAmount, Lamps[] marquee, CoinFountain[] coinFountains)
        {
            title = jackpotTitle;
            amount = jackpotAmount;
            lamps = marquee;
            fountains = coinFountains;
        }

        private void Awake()
        {
            titleColor = title.color;
            amountColor = amount.color;
        }

        public void Show()
        {
            if (active) return;
            active = true;
            foreach (Lamps l in lamps) l.Mode = LampsMode.Sequence;
            flash = StartCoroutine(FlashRoutine());
            fountain = StartCoroutine(FountainRoutine());
        }

        public void Cancel()
        {
            if (!active) return;
            active = false;
            if (flash != null) StopCoroutine(flash);
            if (fountain != null) StopCoroutine(fountain);
            title.color = titleColor;
            amount.color = amountColor;
            foreach (Lamps l in lamps) l.Mode = LampsMode.Random;
        }

        private IEnumerator FlashRoutine()
        {
            float time = 0f;
            while (true)
            {
                time += Time.deltaTime;
                float k = 0.5f * (Mathf.Cos(time * Mathf.PI * 2f) + 1f);
                title.color = new Color(titleColor.r, titleColor.g, titleColor.b, titleColor.a * k);
                amount.color = new Color(amountColor.r, amountColor.g, amountColor.b, amountColor.a * k);
                yield return null;
            }
        }

        private IEnumerator FountainRoutine()
        {
            while (true)
            {
                foreach (CoinFountain f in fountains)
                {
                    f.Jump();
                    GameServices.Sound.PlayWinCoins();
                    yield return new WaitForSeconds(fountainInterval);
                }
                yield return new WaitForSeconds(fountainPause);
            }
        }
    }
}

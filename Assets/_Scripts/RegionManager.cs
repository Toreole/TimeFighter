using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Game
{
    public class RegionManager : MonoBehaviour
    {
        [SerializeField]
        protected TextMeshProUGUI regionName, regionSubtext;
        [SerializeField]
        protected CanvasGroup canvasGroup;
        [SerializeField]
        protected float fadeIn = 0.6f, stayTime = 3f, fadeOut = 1.5f;

        //IEnumerator Start()
        //{
        //    yield return new WaitForSeconds(1);
        //    yield return DoRegionText();
        //}

        public void TriggerRegion(string rName, string rSubText)
        {
            StopAllCoroutines();
            regionName.text = rName;
            regionSubtext.text = rSubText;
            StartCoroutine(DoRegionText());
        }

        IEnumerator DoRegionText()
        {
            for (float t = 0; t <= fadeIn; t += Time.deltaTime)
            {
                canvasGroup.alpha = t / fadeIn;
                yield return null;
            }
            yield return new WaitForSeconds(stayTime);
            for (float t = fadeOut; t > 0; t -= Time.deltaTime)
            {
                canvasGroup.alpha = t / fadeOut;
                yield return null;
            }
        }
    }
}
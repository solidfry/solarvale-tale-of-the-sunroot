using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gaia
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string m_tooltipHeader;
        [Multiline]
        public string m_tooltipContent;

        //UI
        public void OnPointerEnter(PointerEventData eventData)
        {
            StopAllCoroutines();
            if (TooltipsManagerPresent())
            {
                if (TooltipManager.Instance.m_interactionMode == InteractionMode.UI || TooltipManager.Instance.m_interactionMode == InteractionMode.Both)
                {
                    StartCoroutine(TooltipDelay());
                }
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            StopAllCoroutines();
            if (TooltipsManagerPresent())
            {
                if (TooltipManager.Instance.m_interactionMode == InteractionMode.UI || TooltipManager.Instance.m_interactionMode == InteractionMode.Both)
                {
                    TooltipManager.HideTooltip();
                }
            }
        }
        //Game
        public void OnMouseEnter()
        {
            StopAllCoroutines();
            if (TooltipsManagerPresent())
            {
                if (TooltipManager.Instance.m_interactionMode == InteractionMode.SceneObjects || TooltipManager.Instance.m_interactionMode == InteractionMode.Both)
                {
                    StartCoroutine(TooltipDelay());
                }
            }
        }
        public void OnMouseExit()
        {
            StopAllCoroutines();
            if (TooltipsManagerPresent())
            {
                if (TooltipManager.Instance.m_interactionMode == InteractionMode.SceneObjects || TooltipManager.Instance.m_interactionMode == InteractionMode.Both)
                {
                    TooltipManager.HideTooltip();
                }
            }
        }
        /// <summary>
        /// Validates in the tooltips manager is present
        /// </summary>
        /// <returns></returns>
        private bool TooltipsManagerPresent()
        {
            return TooltipManager.Instance != null;
        }
        /// <summary>
        /// Processes the tooltip showing
        /// </summary>
        /// <returns></returns>
        private IEnumerator TooltipDelay()
        {
            yield return new WaitForSeconds(TooltipManager.Instance.m_tooltipDelay);
            TooltipManager.ShowTooltip(m_tooltipContent, m_tooltipHeader);
            yield return new WaitForEndOfFrame();
            StopAllCoroutines();
        }
    }
}
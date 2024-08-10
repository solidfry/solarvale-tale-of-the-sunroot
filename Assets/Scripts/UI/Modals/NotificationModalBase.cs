using TMPro;
using UI.Utilities;
using UnityEngine;

namespace UI.Modals
{
    public abstract class NotificationModalBase<T> : MonoBehaviour where T : ScriptableObject
    {
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text description;
        [SerializeField] T data;
        [SerializeField] DoTweenAnimationHandler doTweenAnimationHandler;
        [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
    
        protected virtual void Awake()
        {
            doTweenAnimationHandler ??= GetComponent<DoTweenAnimationHandler>();
            CanvasGroup ??= GetComponent<CanvasGroup>();
            gameObject.SetActive(false);
        }

        protected virtual void Start()
        {
            SetData(data);
        }
    
        protected virtual void OnDisable()
        {
            ClearDescription();
        }
        
        protected virtual void SetTitle()
        {
            title.text = data.name;
        }

        protected virtual void SetDescription()
        {
            description.text = data.name;
        }
    
        protected virtual void ClearDescription()
        {
            description.text = "";
        }
    
        public virtual void SetData(T data)
        {
            if (data is null) return;
            this.data = data;
            SetDescription();
        }
    
        public virtual void SetActive()
        {
            gameObject.SetActive(true);
        }
    }
}
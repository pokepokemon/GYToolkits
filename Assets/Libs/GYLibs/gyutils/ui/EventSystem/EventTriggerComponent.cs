using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GYLib.UI
{
    /// <summary>
    /// 让UGUI可以像NGUI一样快速添加UI事件
    /// 使用方法:
    /// GetComponent<EventTriggerComponent>().onClick=
    /// delegate void FunGameObject(GameObject target)
    /// {
    ///   //DoSomethings
    /// };
    /// </summary>
    public class EventTriggerComponent : UnityEngine.EventSystems.EventTrigger
    {
        public static BaseEventData eventData;
        public CallBack.FunGameObject onClick;
        public CallBack.FunGameObject onDown;
        public CallBack.FunGameObject onEnter;
        public CallBack.FunGameObject onExit;
        public CallBack.FunGameObject onUp;
        public CallBack.FunGameObject onSelect;
        public CallBack.FunGameObject onUpdateSelect;
        public CallBack.FunGameObject onDrag;



        protected virtual void Awake()
        {
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            EventTriggerComponent.eventData = eventData;
            if (onClick != null) 
                onClick(gameObject);

            EventTriggerComponent.eventData = null;
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            EventTriggerComponent.eventData = eventData;
            if (onDown != null)
                onDown(gameObject);

            EventTriggerComponent.eventData = null;
        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            EventTriggerComponent.eventData = eventData;
            if (onEnter != null)
                onEnter(gameObject);
    
            EventTriggerComponent.eventData = null;
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            EventTriggerComponent.eventData = eventData;
            if (onExit != null)
                onExit(gameObject);

            EventTriggerComponent.eventData = null;
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            EventTriggerComponent.eventData = eventData;
            if (onUp != null)
                onUp(gameObject);
       
            EventTriggerComponent.eventData = null;
        }
        public override void OnSelect(BaseEventData eventData)
        {
            EventTriggerComponent.eventData = eventData;
            if (onSelect != null)
                onSelect(gameObject);
         
            EventTriggerComponent.eventData = null;
        }
        public override void OnUpdateSelected(BaseEventData eventData)
        {
            EventTriggerComponent.eventData = eventData;
            if (onUpdateSelect != null)
                onUpdateSelect(gameObject);
 
            EventTriggerComponent.eventData = null;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            EventTriggerComponent.eventData = eventData;
            if (onDrag != null)
                onDrag(gameObject);
   
            EventTriggerComponent.eventData = null;
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GYLib.Utils {
    public class LayerManager
    {
        public static readonly LayerManager instance = new LayerManager();

        private GameObject _background;
        private GameObject _container;
        private GameObject _map;
        private UIContainer _uiContainer;
        private DepthSort _sortManager;
        private List<SpriteRenderer> _tranforms = null;

        public void Init()
        {
            GameObject __bg = GameObject.Find("__Background");
            GameObject __map = GameObject.Find("__Map");
            GameObject __contaienr = GameObject.Find("__Container");
            GameObject __uiContainer = GameObject.Find("__UIContainer");
            _background = __bg;
            _container = __contaienr;
            _map = __map;
            
            _tranforms = new List<SpriteRenderer>();
            _sortManager = new DepthSort(_tranforms);
            UIContainer tmpContainer = new UIContainer(
                DisplayUtils.GetChildByName(__uiContainer, "World"),
                DisplayUtils.GetChildByName(__uiContainer, "Menu"), 
                DisplayUtils.GetChildByName(__uiContainer, "PopUp"),
                DisplayUtils.GetChildByName(__uiContainer, "Loading"));
            tmpContainer.canvas = __uiContainer.GetComponent<Canvas>();
            _uiContainer = tmpContainer;

            _sortManager.Start();
        }

        public GameObject background
        {
            get { return _background; }
        }

        public GameObject container
        {
            get { return _container; }
        }

        public GameObject map
        {
            get { return _map; }
        }

        public UIContainer uiContainer
        {
            get { return _uiContainer; }
        }

        public void addToContainer(SpriteRenderer child, bool addToLayer = true)
        {
            _tranforms.Add(child);
            if (addToLayer)
            {
                child.transform.SetParent(_container.transform);
            }
            child.sortingLayerName = _container.GetComponent<SpriteRenderer>().sortingLayerName;
        }

        public void removeFromContainer(SpriteRenderer child, bool removeFromLayer = true)
        {
            if (_tranforms.Contains(child))
            {
                _tranforms.Remove(child);
                if (removeFromLayer)
                {
                    child.transform.SetParent(null);
                }
            }
        }
    }
}
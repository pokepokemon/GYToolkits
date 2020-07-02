﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UIContainer
{
    public Canvas canvas;

    private List<GameObject> _worldList = new List<GameObject>();
    private List<GameObject> _popUpList = new List<GameObject>();
    private List<GameObject> _menuList = new List<GameObject>();
    private List<GameObject> _loadingList = new List<GameObject>();

    public Transform popUpLayer;
    public Transform menuLayer;
    public Transform loadingLayer;
    public Transform worldLayer;

    public UIContainer(GameObject world, GameObject menu, GameObject popUp, GameObject loading)
    {
        worldLayer = world.transform;
        popUpLayer = popUp.transform;
        menuLayer = menu.transform;
        loadingLayer = loading.transform;
    }

    public void AddPopUp(GameObject go)
    {
        if (_popUpList.Contains(go))
        {
            _popUpList.Remove(go);
        }
        _popUpList.Add(go);
        if (go.GetComponent<DestroyCallback>() == null)
        {
            go.AddComponent<DestroyCallback>().callback += delegate ()
            {
                RemovePopUp(go, false);
            };
        }
        go.transform.SetParent(popUpLayer, false);
    }

    public void RemovePopUp(GameObject go, bool needDestroy = true)
    {
        if (_popUpList.Contains(go))
        {
            _popUpList.Remove(go);
            if (needDestroy)
                GameObject.Destroy(go);
        }
    }

    public void AddMenu(GameObject go)
    {
        if (_menuList.Contains(go))
        {
            _menuList.Remove(go);
        }
        _menuList.Add(go);
        if (go.GetComponent<DestroyCallback>() == null)
        {
            go.AddComponent<DestroyCallback>().callback += delegate ()
            {
                RemoveMenu(go);
            };
        }
        go.transform.SetParent(menuLayer, false);
    }

    public void RemoveMenu(GameObject go)
    {
        if (_menuList.Contains(go))
        {
            _menuList.Remove(go);
            GameObject.Destroy(go);
        }
    }

    public void AddLoading(GameObject go)
    {
        if (_loadingList.Contains(go))
        {
            _loadingList.Remove(go);
        }
        _loadingList.Add(go);

        if (go.GetComponent<DestroyCallback>() == null)
        {
            go.AddComponent<DestroyCallback>().callback += delegate ()
            {
                RemoveLoading(go);
            };
        }
        go.transform.SetParent(loadingLayer, false);
    }

    public void RemoveLoading(GameObject go)
    {
        if (_loadingList.Contains(go))
        {
            _loadingList.Remove(go);
            GameObject.Destroy(go);
        }
    }

    private List<UIModelBlind> _blindList = new List<UIModelBlind>();
    public void AddWorld(GameObject go)
    {
        if (_worldList.Contains(go))
        {
            _worldList.Remove(go);
        }
        _worldList.Add(go);

        if (go.GetComponent<DestroyCallback>() == null)
        {
            go.AddComponent<DestroyCallback>().callback += delegate ()
            {
                RemoveWorld(go);
            };
        }
        go.transform.SetParent(worldLayer, false);

        UIModelBlind blind = go.GetComponent<UIModelBlind>();
        InsertToWorldSlibingIndex(blind);
    }

    private void InsertToWorldSlibingIndex(UIModelBlind blind)
    {
        if (blind != null)
        {
            if (_blindList.IndexOf(blind) != - 1)
            {
                _blindList.Remove(blind);
            }
            _blindList.Add(blind);
            List<GameObject> noBlindList = _worldList.FindAll((ui) => ui.GetComponent<UIModelBlind>() == null);
            int index = 0;
            for (int i = 0; i < noBlindList.Count; i++, index++)
            {
                noBlindList[i].transform.SetSiblingIndex(index);
            }
            for (int i = 0; i < _blindList.Count; i++, index++)
            {
                _blindList[i].transform.SetSiblingIndex(index);
            }
        }
    }

    public void RemoveWorld(GameObject go)
    {
        UIModelBlind blind = go.GetComponent<UIModelBlind>();
        if (blind != null)
        {
            _blindList.Remove(blind);
        }
        if (_worldList.Contains(go))
        {
            _worldList.Remove(go);
            GameObject.Destroy(go);
        }
    }

    public GameObject GetPopUpLayer()
    {
        return popUpLayer.gameObject;
    }

    public GameObject GetMenuLayer()
    {
        return menuLayer.gameObject;
    }

    public GameObject GetWorldLayer()
    {
        return worldLayer.gameObject;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using GYLib.Utils;

public class UISortingContainer : UIContainer
{
    public UISortingTree sortingTree { private set; get; }

    public UISortingContainer(GameObject world, GameObject menu, GameObject popUp, GameObject loading, GameObject cursor,
        UISortingTree sort) : base(world, menu, popUp, loading, cursor)
    {
        sortingTree = sort;
    }

    public override void AddLoading(GameObject go)
    {
        base.AddLoading(go);
        sortingTree.Rebuild();
    }

    public override void AddMenu(GameObject go)
    {
        base.AddMenu(go);
        sortingTree.Rebuild();
    }

    public override void AddPopUp(GameObject go)
    {
        base.AddPopUp(go);
        sortingTree.Rebuild();
    }

    public override void AddWorld(GameObject go)
    {
        base.AddWorld(go);
        sortingTree.Rebuild();
    }
}

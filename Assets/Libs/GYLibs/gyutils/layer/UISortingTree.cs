using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GYLib.Utils
{
    public class UISortingTree : MonoBehaviour
    {
        private Queue<UISortingTreeNode> _pool = new Queue<UISortingTreeNode>();
        private List<UISortingTreeNode> _usingNode = new List<UISortingTreeNode>();

        public GameObject goRoot;
        public int renderStep = 20;
        public bool updateRebuild = false;

        public void Start()
        {
            Rebuild();
        }

        public void Rebuild()
        {
            RecycleToPool();
            _cacheMap.Clear();
            PickAllNode();
            BuildTree();
            SortingTree();
        }

        private bool _nextTimeRebuild = false;
        public void RebuildDirty()
        {
            _nextTimeRebuild = true;
        }

        void Update()
        {
            if (updateRebuild || _nextTimeRebuild)
            {
                Rebuild();
                _nextTimeRebuild = false;
            }
        }

        Dictionary<int, UISortingTreeNode> _cacheMap = new Dictionary<int, UISortingTreeNode>();
        private void RecycleToPool()
        {
            foreach (var node in _usingNode)
            {
                node.Reset();
                _pool.Enqueue(node);
            }
            _usingNode.Clear();
        }

        /// <summary>
        /// Parse node to usingList
        /// </summary>
        /// <param name="cacheDict"></param>
        private void PickAllNode()
        {
            Canvas[] canvasList = goRoot.GetComponentsInChildren<Canvas>(true);
            foreach (Canvas canvas in canvasList)
            {
                UISortingTreeNode node = CheckInCache(canvas.transform);
                node.canvas = canvas;
            }

            Renderer[] rendererList = goRoot.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in rendererList)
            {
                UISortingTreeNode node = CheckInCache(renderer.transform);
                node.renderers = node.renderers ?? new List<Renderer>();
                node.renderers.Add(renderer);
            }

            UISortingTreeNode root = CheckInCache(goRoot.transform);
        }

        /// <summary>
        /// in cache map or pool create
        /// </summary>
        /// <param name="cacheDict"></param>
        /// <param name="tf"></param>
        /// <returns></returns>
        private UISortingTreeNode CheckInCache(Transform tf)
        {
            int instanceID = tf.GetInstanceID();
            UISortingTreeNode node;
            if (!_cacheMap.TryGetValue(instanceID, out node))
            {
                node = GetEmptyNode();
                node.instanceID = instanceID;
                node.tf = tf;
                _cacheMap[instanceID] = node;
                _usingNode.Add(node);
            }

            return node;
        }

        /// <summary>
        /// Get node in pool or create
        /// </summary>
        /// <returns></returns>
        private UISortingTreeNode GetEmptyNode()
        {
            if (_pool.Count > 0)
            {
                return _pool.Dequeue();
            }
            else
            {
                return new UISortingTreeNode();
            }
        }

        private HashSet<int> _cacheHashMap = new HashSet<int>();
        /// <summary>
        /// Create tree relation
        /// </summary>
        private void BuildTree()
        {
            _cacheHashMap.Clear();
            foreach (var node in _usingNode)
            {
                UISortingTreeNode curNode = node;
                Transform tmpTf = curNode.tf;
                int curInstanceId = tmpTf.GetInstanceID();
                if (!_cacheHashMap.Contains(curInstanceId))
                {
                    float order = 0;
                    while (tmpTf.parent != null)
                    {
                        //Already set
                        if (!_cacheHashMap.Contains(curInstanceId))
                        {
                            int parentId = tmpTf.parent.GetInstanceID();
                            
                            int tfIndex = tmpTf.GetSiblingIndex();
                            int tfTotalCount = tmpTf.parent.childCount;
                            order += (float)tfIndex / tfTotalCount;

                            UISortingTreeNode cacheNode;
                            if (_cacheMap.TryGetValue(parentId, out cacheNode))
                            {
                                curNode.parent = cacheNode;
                                cacheNode.children.Add(curNode);
                                //mask as done
                                _cacheHashMap.Add(curNode.instanceID);
                                curNode.treeOrder = order;

                                //replace
                                curNode = cacheNode;
                            }
                        }
                        else
                        {
                            break;
                        }

                        order *= 0.1f;
                        tmpTf = tmpTf.parent;
                        //not sync with curNode
                        curInstanceId = tmpTf.GetInstanceID();
                    }
                }
            }
        }

        private Stack<UISortingTreeNode> _cacheStack = new Stack<UISortingTreeNode>();
        /// <summary>
        /// Tree sort node and set render order
        /// </summary>
        private void SortingTree()
        {
            int renderOrder = 0;
            UISortingTreeNode root = null;
            foreach (var node in _usingNode)
            {
                node.SortChildren();
                if (node.parent == null)
                {
                    root = node;
                }
            }

            if (root != null)
            {
                _cacheStack.Clear();
                _cacheStack.Push(root);
                while (_cacheStack.Count > 0)
                {
                    UISortingTreeNode tmpNode = _cacheStack.Pop();
                    tmpNode.renderOrder = renderOrder;
                    renderOrder += renderStep;
                    tmpNode.SetRenderOrder();
                    for (int i = tmpNode.children.Count - 1; i >= 0; i--)
                    {
                        var childNode = tmpNode.children[i];
                        _cacheStack.Push(childNode);
                    }
                }
            }
        }

        internal class UISortingTreeNode
        {
            //root transform instanceID
            public int instanceID;

            public Transform tf;
            public Canvas canvas;
            public List<Renderer> renderers;

            public List<UISortingTreeNode> children;
            public UISortingTreeNode parent;

            //order in same tree parent (not unity transform's parent)
            public float treeOrder;
            public int renderOrder;

            public UISortingTreeNode()
            {
                renderers = null;
                children = new List<UISortingTreeNode>();
                parent = null;
                instanceID = -1;
                canvas = null;
                treeOrder = 0;
                tf = null;
                renderOrder = 0;
            }

            public void Reset()
            {
                renderers?.Clear();
                children.Clear();
                instanceID = -1;
                canvas = null;
                parent = null;
                treeOrder = 0;
                tf = null;
                renderOrder = 0;
            }

            public void SetRenderOrder()
            {
                if (canvas != null)
                {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = renderOrder;
                }
                if (renderers != null && renderers.Count > 0)
                {
                    foreach (var render in renderers)
                    {
                        render.sortingOrder = renderOrder;
                    }
                }
            }

            public void SortChildren()
            {
                children.Sort(SortFunction);
            }

            private int SortFunction(UISortingTreeNode a, UISortingTreeNode b)
            {
                float result =  a.treeOrder - b.treeOrder;
                if (result > 0)
                {
                    return 1;
                }
                else if (result == 0)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace Lemon
{
    public class DragMgr : MonoBehaviour
    {
        public List<DragItem> dragItems = new List<DragItem>();

        public List<Transform> optionalPosition = new List<Transform>();

        [SerializeField] private Material selectMaterial;
        private Action _updateAction;
        private new Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;

            foreach (var item in dragItems)
            {
                item.Init(BeginDrag, EndDrag);

                item.successAction = DragSucess;
                item.failedAction = DragFailed;
            }

            dragItems[0].range = 1;
            dragItems[1].range = 1;
        }

        private void Update()
        {
            //TODO Test
            if (Input.GetMouseButtonDown(0)) dragItems[0].beginDrag?.Invoke(dragItems[1]);
            if (Input.GetMouseButtonUp(0)) dragItems[0].endDrag?.Invoke(dragItems[1]);

            if (Input.GetMouseButtonDown(1)) dragItems[1].beginDrag?.Invoke(dragItems[0]);
            if (Input.GetMouseButtonUp(1)) dragItems[1].endDrag?.Invoke(dragItems[0]);

            _updateAction?.Invoke();
        }


        #region 回调方法

        /// <summary>
        /// 拖拽成功回调
        /// </summary>
        /// <param name="trans"></param>
        private void DragSucess(Transform trans, GameObject game)
        {
            Debug.Log("YYYY");
        }

        /// <summary>
        /// 拖拽失败回调
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="game"></param>
        private void DragFailed(Transform trans, GameObject game)
        {
            Debug.Log("NNNNN");

            optionalPosition.Add(trans);
            Destroy(game);
        }

        #endregion

        #region 拖拽方法

        private Vector3 oldPostion;

        /// <summary>
        /// 跟随鼠标
        /// </summary>
        /// <param name="tag"></param>
        private void FollowMouse(GameObject tag)
        {
            var index = Nearest(Input.mousePosition,
                optionalPosition.Select(pos => pos.transform.position).ToArray());

            // var offset = optionalPosition[index].position.z - camera.transform.position.z;
            // var pos = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, offset));
            // pos.z = optionalPosition[index].position.z;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(_camera.transform.position - optionalPosition[index].position,
                optionalPosition[index].position);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);

                tag.transform.position = hitPoint;
            }
        }

        /// <summary>
        /// 开始拖拽
        /// </summary>
        /// <param name="_"></param>
        private void BeginDrag(DragItem _)
        {
            foreach (var item in optionalPosition)
            {
                GameObject temp = Instantiate(_.prefab);
                AlignToPoint(temp.transform, item);
                foreach (var renderer in temp.GetComponentsInChildren<Renderer>())
                {
                    renderer.material = selectMaterial;
                }

                _.tempList.Add(temp);
            }

            GameObject current = Instantiate(_.prefab);
            _.currentPrefab = current;

            _updateAction += () => FollowMouse(_.currentPrefab);
        }

        /// <summary>
        /// 结束拖拽
        /// </summary>
        /// <param name="_"></param>
        private void EndDrag(DragItem _)
        {
            _updateAction = null; // -= () => FollowMouse(_.currentPrefab,_);

            bool find = false;
            foreach (var VARIABLE in optionalPosition)
            {
                if (Vector3.Distance(_.currentPrefab.transform.position, VARIABLE.transform.position) <= _.range)
                {
                    var point = AlignToPointV3(_.currentPrefab.transform, VARIABLE.transform);
                    _.currentPrefab.transform.DOMove(point, 0.25f);
                    _.currentPrefab.transform.DORotate(VARIABLE.transform.eulerAngles, 0.25f);

                    if (_.correctPosition.ToList().Exists(_ => _ == VARIABLE))
                    {
                        _.successAction?.Invoke(VARIABLE, _.currentPrefab);
                        optionalPosition.Remove(VARIABLE);
                    }
                    else
                    {
                        _.failedAction?.Invoke(VARIABLE, _.currentPrefab);
                        optionalPosition.Remove(VARIABLE);
                    }

                    find = true;
                    break;
                }
            }

            if (!find) Destroy(_.currentPrefab);

            foreach (var VARIABLE in _.tempList)
            {
                Destroy(VARIABLE);
            }
        }

        #endregion

        #region 辅助方法

        private void AlignToPoint(Transform alignItem, Transform point)
        {
            // 获取物体的碰撞器
            var collider = alignItem.GetComponent<Collider>();
            if (collider == null)
            {
                alignItem.forward = point.forward;
                // 获取模型的包围盒
                var bounds = alignItem.GetComponent<Renderer>().bounds;
                // 获取模型的底部中心点
                var bottomCenter = (bounds.center - new Vector3(0, bounds.extents.y, 0));
                // 计算物体需要移动的距离
                var distanceToGround = (bottomCenter - point.position);
                // 移动物体
                alignItem.transform.position -= distanceToGround;
            }
            else
            {
                alignItem.forward = point.forward;
                // 获取物体的底部中心点
                var bounds = collider.bounds;
                var bottomCenter =
                    (bounds.center - new Vector3(0, bounds.extents.y, 0));
                // 计算物体需要移动的距离
                var distanceToGround = (bottomCenter - point.position);
                // 移动物体
                alignItem.transform.position -= distanceToGround;
            }
            //transform.rotation = point.rotation;
        }

        private Vector3 AlignToPointV3(Transform alignItem, Transform point)
        {
            // 获取物体的碰撞器
            Collider collider = alignItem.GetComponent<Collider>();
            if (collider == null)
            {
                alignItem.forward = point.forward;
                // 获取模型的包围盒
                Bounds bounds = alignItem.GetComponent<Renderer>().bounds;
                // 获取模型的底部中心点
                Vector3 bottomCenter = (bounds.center - new Vector3(0, bounds.extents.y, 0));
                // 计算物体需要移动的距离
                Vector3 distanceToGround = (bottomCenter - point.position);
                // 移动物体
                return alignItem.transform.position - distanceToGround;
            }
            else
            {
                alignItem.forward = point.forward;
                // 获取物体的底部中心点
                var bounds = collider.bounds;
                var bottomCenter =
                    (bounds.center - new Vector3(0, bounds.extents.y, 0));
                // 计算物体需要移动的距离
                var distanceToGround = (bottomCenter - point.position);
                // 移动物体
                return alignItem.transform.position - distanceToGround;
            }
        }

        private int Nearest(Vector3 tagpos, Vector3[] list)
        {
            var minDistance = float.MaxValue;
            var minIndex = 0;

            for (int i = 0; i < list.Length; i++)
            {
                var scenePos = _camera.WorldToScreenPoint(list[i]);
                var temp = Vector3.Distance(tagpos, scenePos);
                if (temp < minDistance)
                {
                    minDistance = temp;
                    minIndex = i;
                }
            }

            return minIndex;
        }

        #endregion
    }


    [Serializable]
    public class DragItem
    {
        public GameObject prefab;

        //public Transform[] optionalPosition;
        public Transform[] correctPosition;
        public List<GameObject> tempList;
        public float range = 1.0f;
        public GameObject currentPrefab;

        public UnityAction<Transform, GameObject> successAction;
        public UnityAction<Transform, GameObject> failedAction;

        public UnityAction<DragItem> beginDrag;
        public UnityAction<DragItem> endDrag;

        public void Init(UnityAction<DragItem> _beginDrag, UnityAction<DragItem> _endDrag)
        {
            this.beginDrag += _beginDrag;
            this.endDrag += _endDrag;
        }
    }
}
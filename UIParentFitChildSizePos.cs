using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace globals.ui
{
	[CustomEditor(typeof(UIParentFitChildSizePos))]
	public class UIParentFitChildSizePosEditor:Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			UIParentFitChildSizePos p = (UIParentFitChildSizePos)target;
			if (GUILayout.Button("Fit Child Size And Position"))
			{
				p.CheckForChanges();
			}
		}
	}

	[ExecuteInEditMode]
    public class UIParentFitChildSizePos : MonoBehaviour
    {
		public void CheckForChanges()
        {
#if UNITY_EDITOR
            const string undoRecordName = "Parent Fit Children";

            RectTransform children = transform.GetComponentInChildren<RectTransform>();

            float min_x, max_x, min_y, max_y;
            min_x = float.MaxValue; max_x = 0;
            min_y = float.MaxValue; max_y = 0;

			Dictionary<RectTransform, Vector3> child_Positions = new Dictionary<RectTransform, Vector3>();
			Vector3[] corner_positions_inScreenPixel = new Vector3[4];
			Vector3 leftBottom, leftTop, rightTop, rightBottom;

			foreach (RectTransform child in children)
            {
				// child.offsetMin 是未被scale之前的数值，如果scale不为1，则这个值和实际值就不一样
				// RectTransformUtility.CalculateRelativeRectTransformBounds 会计算所有子对象就算是被rect mask遮蔽的部分也会被算进来

				child.GetWorldCorners(corner_positions_inScreenPixel);

				leftBottom = corner_positions_inScreenPixel[0];
				leftTop = corner_positions_inScreenPixel[1];
				rightTop = corner_positions_inScreenPixel[2];
				rightBottom = corner_positions_inScreenPixel[3];

				//Debug.Log("child \"" + child.gameObject.name + "\":" + child.offsetMin + "/" + leftBottom + "/"
				//	+ leftTop + "/" + rightTop + "/" + rightBottom + "/");

				if (min_x > leftBottom.x) min_x = leftBottom.x;
				if (max_x < rightTop.x) max_x = rightTop.x;

				if (min_y > leftBottom.y) min_y = leftBottom.y;
				if (max_y < rightTop.y) max_y = rightTop.y;

				child_Positions.Add(child, child.position);
			}

			var prt = GetComponent<RectTransform>();
			prt.sizeDelta = new Vector2(max_x - min_x, max_y - min_y);

			Vector3 parent_new_position = Vector3.zero;

			parent_new_position.x = prt.pivot.x * prt.sizeDelta.x + min_x;
			parent_new_position.y = prt.pivot.y * prt.sizeDelta.y + min_y;

			{
                Vector3 diffpos = prt.position - parent_new_position;

                Undo.RecordObject(prt, undoRecordName);
				prt.position = parent_new_position;

				// child position changed , because parent has moved; reset to original position
				foreach (var child in child_Positions)
                {
                    Undo.RecordObject(child.Key, undoRecordName);
                    child.Key.position = child.Value;
                }
            }
#endif
            return;
        }
    }
}
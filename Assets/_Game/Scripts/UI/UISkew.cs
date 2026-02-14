using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace IOChef.UI
{
    // Adds a shear transformation to the UI element
    public class UISkew : BaseMeshEffect
    {
        [Tooltip("Horizontal skew amount (shear X based on Y)")]
        public float skewX = 0.2f;

        [Tooltip("Vertical skew amount (shear Y based on X)")]
        public float skewY = 0f;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;

            var list = new List<UIVertex>();
            vh.GetUIVertexStream(list);

            for (int i = 0; i < list.Count; i++)
            {
                var v = list[i];
                float xOffset = v.position.y * skewX;
                float yOffset = v.position.x * skewY;
                
                v.position.x += xOffset;
                v.position.y += yOffset;
                
                list[i] = v;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(list);
        }
    }
}

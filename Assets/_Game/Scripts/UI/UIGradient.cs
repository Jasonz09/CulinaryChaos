using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace IOChef.UI
{
    /// <summary>
    /// Applies a smooth vertex-color gradient to any UI Graphic (Image, Text, etc.).
    /// Supports vertical (top→bottom) and horizontal (left→right) directions.
    /// Attach alongside an Image component for gradient button faces, panels, etc.
    /// </summary>
    public class UIGradient : BaseMeshEffect
    {
        /// <summary>Color at the top (vertical) or right (horizontal) edge.</summary>
        public Color topColor = Color.white;

        /// <summary>Color at the bottom (vertical) or left (horizontal) edge.</summary>
        public Color bottomColor = Color.white;

        /// <summary>When true, gradient flows left→right instead of bottom→top.</summary>
        [Tooltip("When true, gradient goes left to right instead of bottom to top.")]
        public bool horizontal = false;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0) return;

            var vertices = new List<UIVertex>();
            vh.GetUIVertexStream(vertices);

            // Determine mesh bounds for normalization
            float minY = float.MaxValue, maxY = float.MinValue;
            float minX = float.MaxValue, maxX = float.MinValue;
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 pos = vertices[i].position;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
            }

            float rangeY = maxY - minY;
            float rangeX = maxX - minX;

            for (int i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                float t;
                if (horizontal)
                    t = rangeX > 0.001f ? (v.position.x - minX) / rangeX : 0f;
                else
                    t = rangeY > 0.001f ? (v.position.y - minY) / rangeY : 0f;

                // Lerp between bottom (t=0) and top (t=1), multiply with existing vertex color
                Color gradColor = Color.Lerp(bottomColor, topColor, t);
                v.color = v.color * gradColor;
                vertices[i] = v;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(vertices);
        }
    }
}

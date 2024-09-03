using UnityEngine;
using UnityEngine.UI;

namespace GYLibs.UI
{
    /// <summary>
    /// No Render UI Button
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class EmptyUIRaycast : MaskableGraphic
	{
		protected EmptyUIRaycast()
		{
			useLegacyMeshGeneration = false;
		}

		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			toFill.Clear();
		}
	}
}
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Singularity")]
public class SgtSingularity : MonoBehaviour
{
	public enum EdgeFadeType
	{
		None,
		Center,
		Fragment
	}

	public static List<SgtSingularity> AllSingularities = new List<SgtSingularity>();

	public List<Mesh> Meshes = new List<Mesh>();
	
	public SgtRenderQueue RenderQueue = SgtRenderQueue.Transparent;

	public int RenderQueueOffset;

	[Tooltip("How much the singulaity distorts the screen")]
	public float PinchPower = 10.0f;
	
	[SgtRange(0.0f, 0.5f)]
	[Tooltip("How large the pinch start point is")]
	public float PinchOffset = 0.02f;

	[Tooltip("To prevent rendering issues the singularity can be faded out as it approaches the edges of the screen. This allows you to set how the fading is calculated")]
	public EdgeFadeType EdgeFade = EdgeFadeType.Fragment;

	[Tooltip("How sharp the fading effect is")]
	public float EdgeFadePower = 2.0f;
	
	[Tooltip("The color of the pinched hole")]
	public Color HoleColor = Color.black;

	[Tooltip("How sharp the hole color gradient is")]
	public float HolePower = 2.0f;

	[Tooltip("Enable this if you want the singulairty to tint nearby space")]
	public bool Tint;

	[Tooltip("The color of the tint")]
	public Color TintColor = Color.red;

	[Tooltip("How sharp the tint color gradient is")]
	public float TintPower = 2.0f;

	[System.NonSerialized]
	private Material material;

	[SerializeField]
	private List<SgtSingularityModel> models = new List<SgtSingularityModel>();

	private static List<string> keywords = new List<string>();

	public void UpdateState()
	{
		UpdateMaterial();
		UpdateModels();
	}

	public static SgtSingularity CreateSingularity(int layer = 0, Transform parent = null)
	{
		return CreateSingularity(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
	}

	public static SgtSingularity CreateSingularity(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
	{
		var gameObject  = SgtHelper.CreateGameObject("Singularity", layer, parent, localPosition, localRotation, localScale);
		var singularity = gameObject.AddComponent<SgtSingularity>();

		return singularity;
	}

#if UNITY_EDITOR
	[UnityEditor.MenuItem(SgtHelper.GameObjectMenuPrefix + "Singularity", false, 10)]
	public static void CreateSingularityMenuItem()
	{
		var parent      = SgtHelper.GetSelectedParent();
		var singularity = CreateSingularity(parent != null ? parent.gameObject.layer : 0, parent);

		SgtHelper.SelectAndPing(singularity);
	}
#endif

	protected virtual void OnEnable()
	{
#if UNITY_EDITOR
		if (AllSingularities.Count == 0)
		{
			SgtHelper.RepaintAll();
		}
#endif
		AllSingularities.Add(this);

		for (var i = models.Count - 1; i >= 0; i--)
		{
			var model = models[i];

			if (model != null)
			{
				model.gameObject.SetActive(true);
			}
		}

		UpdateState();
	}

	protected virtual void OnDisable()
	{
		AllSingularities.Remove(this);

		for (var i = models.Count - 1; i >= 0; i--)
		{
			var model = models[i];

			if (model != null)
			{
				model.gameObject.SetActive(false);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		SgtHelper.Destroy(material);

		for (var i = models.Count - 1; i >= 0; i--)
		{
			SgtSingularityModel.MarkForDestruction(models[i]);
		}

		models.Clear();
	}

	protected virtual void Update()
	{
		UpdateState();
	}

	private void UpdateMaterial()
	{
		if (material == null) material = SgtHelper.CreateTempMaterial(SgtHelper.ShaderNamePrefix + "Singularity");
		
		var renderQueue = (int)RenderQueue + RenderQueueOffset;

		material.renderQueue = renderQueue;
		material.SetVector("_Center", SgtHelper.NewVector4(transform.position, 1.0f));
		
		material.SetFloat("_PinchPower", PinchPower);
		material.SetFloat("_PinchScale", SgtHelper.Reciprocal(1.0f - PinchOffset));
		material.SetFloat("_PinchOffset", PinchOffset);

		material.SetFloat("_HolePower", HolePower);
		material.SetColor("_HoleColor", HoleColor);

		if (Tint == true)
		{
			keywords.Add("SGT_A");

			material.SetFloat("_TintPower", TintPower);
			material.SetColor("_TintColor", TintColor);
		}

		switch (EdgeFade)
		{
			case EdgeFadeType.Center:
			{
				keywords.Add("SGT_B");

				material.SetFloat("_EdgeFadePower", EdgeFadePower);
			}
			break;

			case EdgeFadeType.Fragment:
			{
				keywords.Add("SGT_C");

				material.SetFloat("_EdgeFadePower", EdgeFadePower);
			}
			break;
		}

		SgtHelper.SetKeywords(material, keywords); keywords.Clear();
	}

	private void UpdateModels()
	{
		models.RemoveAll(m => m == null);

		if (Meshes.Count != models.Count)
		{
			SgtHelper.ResizeArrayTo(ref models, Meshes.Count, i => SgtSingularityModel.Create(this), m => SgtSingularityModel.Pool(m));
		}

		for (var i = Meshes.Count - 1; i >= 0; i--)
		{
			models[i].ManualUpdate(Meshes[i], material);
		}
	}
}

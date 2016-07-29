using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Spacetime")]
public class SgtSpacetime : MonoBehaviour
{
	public enum DisplacementType
	{
		Pinch,
		Offset
	}
	
	// All currently active and enabled spacetimes in the scene
	public static List<SgtSpacetime> AllSpacetimes = new List<SgtSpacetime>();
	
	public Color Color = Color.white;
	
	public float Brightness = 1.0f;
	
	public SgtRenderQueue RenderQueue = SgtRenderQueue.Transparent;
	
	public int RenderQueueOffset;
	
	[Tooltip("The main texture applied to the spacetime")]
	public Texture2D MainTex;

	[Tooltip("How many times should the spacetime texture be tiled?")]
	public int Tile = 1;

	[Tooltip("The ambient color")]
	public Color AmbientColor = Color.white;

	[Tooltip("The ambient brightness")]
	public float AmbientBrightness = 0.25f;

	[Tooltip("The displacement color")]
	public Color DisplacementColor = Color.white;

	[Tooltip("The displacement brightness")]
	public float DisplacementBrightness = 1.0f;
	
	[Tooltip("The color of the highlight")]
	public Color HighlightColor = Color.white;

	[Tooltip("The brightness of the highlight")]
	public float HighlightBrightness = 0.1f;

	[Tooltip("The sharpness of the highlight")]
	public float HighlightPower = 1.0f;

	[Tooltip("The scale of the highlight")]
	public float HighlightScale = 3.0f;

	[Tooltip("How should the vertices in the spacetime get displaced when a well is nearby?")]
	public DisplacementType Displacement = DisplacementType.Pinch;
	
	[Tooltip("Should the displacement effect additively stack if wells overlap?")]
	public bool Accumulate;
	
	[Tooltip("The pinch power")]
	public float Power = 3.0f;

	[Tooltip("How strong the fading is")]
	public float FadeScale = 1.0f;

	[Tooltip("The offset direction/vector for vertices within range of a well")]
	public Vector3 Offset = new Vector3(0.0f, -1.0f, 0.0f);
	
	[Tooltip("Automatically use all active and enabled wells in the scene?")]
	public bool UseAllWells = true;
	
	[Tooltip("Filter all the wells to require the same layer at this GameObject")]
	public bool RequireSameLayer;
	
	[Tooltip("Filter all the wells to require the same tag at this GameObject")]
	public bool RequireSameTag;
	
	[Tooltip("Filter all the wells to require a name that contains this")]
	public string RequireNameContains;
	
	[Tooltip("The wells currently being checked by the spacetime")]
	public List<SgtSpacetimeWell> Wells = new List<SgtSpacetimeWell>();
	
	[Tooltip("The renderers this spacetime is being applied to")]
	public List<MeshRenderer> Renderers = new List<MeshRenderer>();
	
	[System.NonSerialized]
	protected Material material;
	
	protected static List<string> keywords = new List<string>();
	
	public void UpdateState()
	{
		UpdateMaterial();
		UpdateRenderers();
	}
	
	[ContextMenu("Add Well")]
	public SgtSpacetimeWell AddWell()
	{
		var well = SgtSpacetimeWell.Create(this);
#if UNITY_EDITOR
		SgtHelper.SelectAndPing(well);
#endif
		return well;
	}
	
#if UNITY_EDITOR
	protected virtual void OnDrawGizmos()
	{
		UpdateState();
	}
#endif
	
	protected virtual void Reset()
	{
		var meshRenderer = GetComponent<MeshRenderer>();
		
		if (meshRenderer != null)
		{
			Renderers.Clear();
			Renderers.Add(meshRenderer);
		}
	}
	
	protected virtual void OnEnable()
	{
		AllSpacetimes.Add(this);

		UpdateState();
	}
	
	protected virtual void OnDisable()
	{
		AllSpacetimes.Remove(this);
		
		for (var i = Renderers.Count - 1; i >= 0; i--)
		{
			var renderer = Renderers[i];
			
			SgtHelper.RemoveMaterial(renderer, material);
		}

		UpdateState();
	}
	
	protected virtual void OnDestroy()
	{
		for (var i = Renderers.Count - 1; i >= 0; i--)
		{
			var renderer = Renderers[i];
			
			SgtHelper.RemoveMaterial(renderer, material);
		}
		
		SgtHelper.Destroy(material);
	}
	
	protected virtual void Update()
	{
		UpdateState();
	}
	
	protected virtual void UpdateMaterial()
	{
		if (material == null) material = SgtHelper.CreateTempMaterial(SgtHelper.ShaderNamePrefix + "Spacetime");
		
		var color             = SgtHelper.Brighten(Color, Brightness);
		var ambientColor      = SgtHelper.Brighten(AmbientColor, AmbientBrightness);
		var displacementColor = SgtHelper.Brighten(DisplacementColor, DisplacementBrightness);
		var higlightColor     = SgtHelper.Brighten(HighlightColor, HighlightBrightness);
		var renderQueue       = (int)RenderQueue + RenderQueueOffset;
		var gaussianCount     = 0;
		var rippleCount       = 0;
		var twistCount        = 0;

		WriteWells(12, 1, 1, ref gaussianCount, ref rippleCount, ref twistCount); // 12 is the shader instruction limit
		
		material.renderQueue = renderQueue;
		material.SetTexture("_MainTex", MainTex);
		material.SetColor("_Color", color);
		material.SetColor("_AmbientColor", ambientColor);
		material.SetColor("_DisplacementColor", displacementColor);
		material.SetColor("_HighlightColor", higlightColor);
		material.SetFloat("_HighlightPower", HighlightPower);
		material.SetFloat("_HighlightScale", HighlightScale);
		material.SetFloat("_Tile", Tile);
		
		switch (Displacement)
		{
			case DisplacementType.Pinch:
			{
				material.SetFloat("_Power", Power);
			}
			break;
			
			case DisplacementType.Offset:
			{
				keywords.Add("SGT_A");
				material.SetVector("_Offset", Offset);
			}
			break;
		}
		
		if (Accumulate == true)
		{
			keywords.Add("SGT_B");
		}
		
		if ((gaussianCount & 1 << 0) != 0)
		{
			keywords.Add("SGT_C");
		}
		
		if ((gaussianCount & 1 << 1) != 0)
		{
			keywords.Add("SGT_D");
		}
		
		if ((gaussianCount & 1 << 2) != 0)
		{
			keywords.Add("SGT_E");
		}
		
		if ((gaussianCount & 1 << 3) != 0)
		{
			keywords.Add("LIGHT_0");
		}

		if ((rippleCount & 1 << 0) != 0)
		{
			keywords.Add("LIGHT_1");
		}

		if ((twistCount & 1 << 0) != 0)
		{
			keywords.Add("SHADOW_1");
		}
		
		SgtHelper.SetKeywords(material, keywords); keywords.Clear();
	}
	
	private void UpdateRenderers()
	{
		for (var i = Renderers.Count - 1; i >= 0; i--)
		{
			var renderer = Renderers[i];
			
			if (renderer != null && renderer.sharedMaterial != material)
			{
				SgtHelper.BeginStealthSet(renderer);
				{
					renderer.sharedMaterial = material;
				}
				SgtHelper.EndStealthSet();
			}
		}
	}
	
	private void WriteWells(int gaussianMax, int rippleMax, int twistMax, ref int gaussianCount, ref int rippleCount, ref int twistCount)
	{
		var wells = UseAllWells == true ? SgtSpacetimeWell.AllWells : Wells;
		
		for (var i = wells.Count - 1; i >= 0; i--)
		{
			var well = wells[i];
			
			if (SgtHelper.Enabled(well) == true && well.Radius > 0.0f)
			{
				if (well.Distribution == SgtSpacetimeWell.DistributionType.Gaussian && gaussianCount >= gaussianMax)
				{
					continue;
				}

				if (well.Distribution == SgtSpacetimeWell.DistributionType.Ripple && rippleCount >= rippleMax)
				{
					continue;
				}

				if (well.Distribution == SgtSpacetimeWell.DistributionType.Twist && twistCount >= twistMax)
				{
					continue;
				}

				// If the well list is atuo generated, allow well filtering
				if (UseAllWells == true)
				{
					if (RequireSameLayer == true && gameObject.layer != well.gameObject.layer)
					{
						continue;
					}
					
					if (RequireSameTag == true && tag != well.tag)
					{
						continue;
					}
					
					if (string.IsNullOrEmpty(RequireNameContains) == false && well.name.Contains(RequireNameContains) == false)
					{
						continue;
					}
				}
				
				var wellPos = well.transform.position;

				switch (well.Distribution)
				{
					case SgtSpacetimeWell.DistributionType.Gaussian:
					{
						var index = gaussianCount++;

						material.SetVector("_GauPos" + index, new Vector4(wellPos.x, wellPos.y, wellPos.z, well.Radius));
						material.SetVector("_GauDat" + index, new Vector4(well.Strength, 0.0f, 0.0f, 0.0f));
					}
					break;

					case SgtSpacetimeWell.DistributionType.Ripple:
					{
						var index = rippleCount++;

						material.SetVector("_RipPos" + index, new Vector4(wellPos.x, wellPos.y, wellPos.z, well.Radius));
						material.SetVector("_RipDat" + index, new Vector4(well.Strength, well.Frequency, well.Offset, 0.0f));
					}
					break;

					case SgtSpacetimeWell.DistributionType.Twist:
					{
						var index = twistCount++;

						material.SetVector("_TwiPos" + index, new Vector4(wellPos.x, wellPos.y, wellPos.z, well.Radius));
						material.SetVector("_TwiDat" + index, new Vector4(well.Strength, well.Frequency, well.HoleSize, well.HolePower));
						material.SetMatrix("_TwiMat" + index, well.transform.worldToLocalMatrix);
					}
					break;
				}
			}
		}
	}
}
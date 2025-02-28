using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Manages the render passes for overlays
public class DownscaleRenderFeature : ScriptableRendererFeature
{
    internal DownscalePixelRenderPass pass;

    [SerializeField] DownscalePixelRenderPass.ShaderSettings DownscaleShaderSettings;
    public override void Create()
    {
        pass = new(DownscaleShaderSettings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_EDITOR
        if (renderingData.cameraData.isSceneViewCamera) return;
#endif
        if (pass != null) renderer.EnqueuePass(pass);
    }
}
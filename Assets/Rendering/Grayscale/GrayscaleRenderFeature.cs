using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Manages the render passes for overlays
public class GrayscaleRenderFeature : ScriptableRendererFeature
{
    internal GrayscaleRenderPass pass;
    public bool UseGrayscale = false;

    [SerializeField] GrayscaleRenderPass.ShaderSettings ShaderSettings;
    public override void Create()
    {
        pass = new(ShaderSettings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!UseGrayscale) return;
#if UNITY_EDITOR
        if (renderingData.cameraData.isSceneViewCamera) return;
#endif
        if (pass != null) renderer.EnqueuePass(pass);
    }
}
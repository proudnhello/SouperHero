using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class GrayscaleRenderPass : ScriptableRenderPass
{

    // code from https://discussions.unity.com/t/replacing-onrenderimage-graphics-blit-in-urp/783177
    // https://github.com/whateep/unity-simple-URP-pixelation/blob/main/Scripts/PixelizePass.cs
    private string k_RenderTag = "Downscale";
    private int targetID = Shader.PropertyToID("_Downscale");

    RenderTargetIdentifier source, currentTarget;
    ShaderSettings settings;
    private Material pixelateMaterial;

    public GrayscaleRenderPass(ShaderSettings settings)
    {
        pixelateMaterial = CoreUtils.CreateEngineMaterial(settings.GrayscaleShader);
        this.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        this.settings = settings;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        if (!renderingData.cameraData.postProcessEnabled) return;
        source = renderingData.cameraData.renderer.cameraColorTargetHandle;
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

        descriptor.width = Screen.width;
        descriptor.height = Screen.height;

        cmd.GetTemporaryRT(targetID, descriptor, FilterMode.Point);
        currentTarget = new RenderTargetIdentifier(targetID);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);
        cmd.Blit(source, currentTarget, pixelateMaterial, 0);
        cmd.Blit(currentTarget, source);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }


    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        if (cmd == null) throw new System.ArgumentNullException("cmd");
        cmd.ReleaseTemporaryRT(targetID);
    }


    [Serializable]
    public class ShaderSettings
    {
        public Shader GrayscaleShader;
    }
}
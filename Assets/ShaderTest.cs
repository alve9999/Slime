using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Threading;
struct float2{
    public float x;
    public float y;
}

struct Agent{
    public float angle;
    public float2 position;
};

public class ShaderTest : MonoBehaviour
{

    private int state = 0;
    private const int n = 1000000;
    public ComputeShader computeShader;
    public RenderTexture renderTexture;
    public ComputeBuffer agent_buffer;
    public int height=320;
    public int width=180;
    public float dt;
    public float evaporation_speed=0.013f;
    public float diffusion_speed = 1f;
    public float agent_speed = 1;
    public float sense_angle = 0.1f;
    public int sense_size = 1;
    public float sense_distance=3f;
    public float turn_speed=0.2f;
    public float wander_strength = 0.2f;
    private Agent[] agents= new Agent[n];
    void set_kernel_val(){
        computeShader.SetInt("state",state);
        state++;
        computeShader.SetFloat("dt",dt);
        computeShader.SetFloat("diffusion_speed",diffusion_speed);
        computeShader.SetFloat("evaporation_speed",evaporation_speed);
        computeShader.SetFloat("agent_speed",agent_speed);
        computeShader.SetFloat("sense_angle",sense_angle);
        computeShader.SetFloat("sense_distance",sense_distance);
        computeShader.SetFloat("turn_speed",turn_speed);
        computeShader.SetFloat("wander_strength",wander_strength);
        
        computeShader.SetInt("sense_size",sense_size);
        computeShader.SetInt("height",height);
        computeShader.SetInt("width",width);
    }

    void Start()
    {
        //dt=Time.fixedDeltaTime;
        renderTexture = new RenderTexture(width,height,32,RenderTextureFormat.ARGBFloat);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();


        int kernelIndex = computeShader.FindKernel("update");
        agent_buffer = new ComputeBuffer(n, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Agent)));


        computeShader.SetBuffer(kernelIndex,"agents",agent_buffer);
        set_kernel_val();
        System.Random random = new System.Random();
        
        for (int i = 0; i < n; i++)
        {
            float2 pos;
            float angle =(float)random.NextDouble()*2*3.1415f;
            float radius = (float)random.NextDouble()*500;
            pos.x = (float)(width/2+Math.Cos(angle)*radius);
            pos.y = (float)(height/2+Math.Sin(angle)*radius);
            agents[i].position=pos;
            agents[i].angle = (float)(angle-3.1415);
        }/*
        for (int i = 0; i < n; i++)
        {
            float2 pos;
            pos.x = (float)(width/2);
            pos.y = (float)(height/2);
            agents[i].position=pos;
            agents[i].angle = (float)random.NextDouble()*2*3.1415f;
        }*/
        
        agent_buffer.SetData(agents);
        
        computeShader.SetTexture(kernelIndex,"Result",renderTexture);
        computeShader.Dispatch(kernelIndex,n/768,1,1);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        set_kernel_val();
        state++;
        int kernelIndex = computeShader.FindKernel("update");
        computeShader.SetTexture(kernelIndex,"Result",renderTexture);
        computeShader.Dispatch(kernelIndex,n/768,1,1);
        kernelIndex = computeShader.FindKernel("blur");
        computeShader.SetTexture(kernelIndex,"bluring",renderTexture);
        computeShader.Dispatch(kernelIndex,renderTexture.width/32,renderTexture.height/32,1);
        Graphics.Blit(renderTexture,dest);

    }

}

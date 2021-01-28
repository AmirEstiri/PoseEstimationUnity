using UnityEngine;
using Unity.Barracuda;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.UI;

public class OnnxModel : MonoBehaviour
{

    int inputHeight = 256;
    int inputWidth = 256;
    int inputChannel = 3;
    int outputHeight = 64;
    int outputWidth = 64;
    int jointSize = 16;
    int depth = 24;

    Tensor input;
    Tensor output;
    Joint[] joints;
    VideoPlayer videoPlayer;
    Texture2D frame;
    RenderTexture renderTexture;
    Model model;
    IWorker engine;
    Rect rect;
    Color c;
    Color pen;
    RawImage rawImage;

    void Start()
    {
        rawImage = GetComponent<RawImage>();

        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.Play();

        frame = new Texture2D(inputWidth, inputHeight, TextureFormat.RGBA32, false);
        renderTexture = new RenderTexture(inputWidth, inputHeight, depth);

        input = new Tensor(1, inputHeight, inputWidth, inputChannel);
        model = ModelLoader.Load("Assets/Models/simplepose.nn");
        engine = WorkerFactory.CreateWorker(model, WorkerFactory.Device.GPU);

        joints = new Joint[jointSize];
        for (int k = 0; k < jointSize; k++) { joints[k] = new Joint(); }

        rect = new Rect(0, 0, inputWidth, inputHeight);
        pen = new Color(1, 0, 0, 1);
    }

    void Update()
    {
        StartCoroutine(RunPoseEstimation());
    }

    IEnumerator RunPoseEstimation()
    {
        ReadFrame();
        ExecuteModel();
        CreateJoints();
        ShowJoints();
        yield return null;
    }

    void ReadFrame()
    {
        Texture videoTexture = videoPlayer.texture;
        Graphics.Blit(videoTexture, renderTexture);
        RenderTexture.active = renderTexture;
        frame.ReadPixels(rect, 0, 0);
        frame.Apply();
    }

    void ExecuteModel()
    {
        for (int i = 0; i < inputWidth; i++)
        {
            for (int j = 0; j < inputHeight; j++)
            {
                c = frame.GetPixel(i, inputHeight - j - 1);
                input[0, j, i, 0] = c.r;
                input[0, j, i, 1] = c.g;
                input[0, j, i, 2] = c.b;
            }
        }
        engine.Execute(input);
        output = engine.PeekOutput();
    }

    void CreateJoints()
    {
        for (int k = 0; k < jointSize; k++)
        {
            joints[k].value = 0.0F;
            joints[k].X = 0;
            joints[k].Y = 0;
        }
        double val;
        for (int i = 0; i < outputWidth; i++)
        {
            for (int j = 0; j < outputHeight; j++)
            {
                for (int k = 0; k < jointSize; k++)
                {
                    val = output[0, j, i, k];
                    if (val > joints[k].value)
                    {
                        joints[k].value = val;
                        joints[k].X = i;
                        joints[k].Y = j;
                    }
                }
            }
        }
        output.Dispose();
        for (int k = 0; k < jointSize; k++)
        {
            joints[k].X = (int)(joints[k].X * inputWidth / outputWidth);
            joints[k].Y = (int)(joints[k].Y * inputHeight / outputHeight);
            joints[k].Y = inputHeight - joints[k].Y - 1;
        }
    }

    void ShowJoints()
    {
        for (int k = 0; k < jointSize; k++)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    frame.SetPixel(i + joints[k].X, j + joints[k].Y, pen);
                }
            }
        }

        //RenderTexture rt = new RenderTexture(1280, 720, 0);
        //RenderTexture.active = rt;
        //Graphics.Blit(frame, rt);

        rawImage.material.mainTexture = frame;
        frame.Apply();

        //byte[] _bytes = frame.EncodeToPNG();
        //File.WriteAllBytes("Assets/Images/out3.png", _bytes);
    }

    void WriteImage()
    {
        for (int k = 0; k < jointSize; k++)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    frame.SetPixel(i + joints[k].X, j + joints[k].Y, pen);
                }
            }
        }
        //videoPlayer.renderMode = 
        //byte[] _bytes = frame.EncodeToPNG();
        //File.WriteAllBytes("Assets/Images/out2.png", _bytes);
    }

}

using UnityEngine;
using Unity.Barracuda;
using System.IO;
using Unity.Barracuda.ONNX;
using System.Collections;

public class Joint
{
    public double value;
    public int X;
    public int Y;
}

public class Person
{
    Joint[] joints = new Joint[19];
}

public class OpenPose : MonoBehaviour
{
    int camHeight = 720;
    int camWidth = 1280;
    int inputHeight = 368;
    int inputWidth = 656;
    int inputChannel = 3;
    int outputHeight = 46;
    int outputWidth = 82;
    int jointSize = 19;
    int depth = 24;
    double threshold = 0.9;

    Tensor input;
    Tensor out_paf;
    Tensor out_conf;
    Texture2D frame;
    Model model;
    IWorker engine;
    Color[] pen;
    Color c;
    WebCamTexture webcamTexture;

    void Start()
    {
        /*Texture2D camRenderer = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
        if (webcamTexture == null)
            webcamTexture = new WebCamTexture();
        //renderer = GetComponent<Renderer>();
        webcamTexture.deviceName = WebCamTexture.devices[1].name;
        //renderer.material.mainTexture = webcamTexture;
        if (!webcamTexture.isPlaying)
            webcamTexture.Play();*/

        frame = new Texture2D(inputWidth, inputHeight, TextureFormat.RGBA32, false);
        byte[] bytes = File.ReadAllBytes("Assets/Images/image1.png");
        frame.LoadImage(bytes);

        input = new Tensor(1, inputHeight, inputWidth, inputChannel);
        model = (new ONNXModelConverter(true)).Convert("Assets/Models/openpose-coco.onnx");
        engine = WorkerFactory.CreateWorker(model, WorkerFactory.Device.GPU);

        pen = new Color[]
        {
            new Color(1, 0, 0, 1),
            new Color(0, 1, 0, 1),
            new Color(0, 0, 1, 1),
            new Color(0, 0, 0, 1),
            new Color(1, 1, 0, 1),
            new Color(1, 0, 1, 1),
            new Color(0, 1, 1, 1),
            new Color(1, 1, 1, 1),
            new Color(0, 0, 0, 1),
            new Color(0, 0, 0, 1),
            new Color(1, 0.5f, 1, 1),
            new Color(1, 1, 0.5f, 1),
            new Color(0.5f, 1, 1, 1),
            new Color(0.5f, 0.5f, 1, 1),
            new Color(0.5f, 1, 0.5f, 1),
            new Color(1, 0.5f, 0.5f, 1),
            new Color(0, 0, 0, 1),
            new Color(0, 0, 0, 1),
            new Color(0, 0, 0, 1)
        };

        ExecuteModel();
        CreateJoints();
        WriteImage();
    }

    void Update()
    {
        //StartCoroutine(RunPoseEstimation());
    }

    IEnumerator RunPoseEstimation()
    {
        ExecuteModel();
        CreateJoints();
        yield return null;
    }

    void ExecuteModel()
    {
        for (int i = 0; i < inputWidth; i++)
        {
            for (int j = 0; j < inputHeight; j++)
            {
                c = frame.GetPixel(i, inputHeight - 1 - j);
                input[0, j, i, 0] = c.r;
                input[0, j, i, 1] = c.g;
                input[0, j, i, 2] = c.b;
            }
        }
        engine.Execute(input);
        out_paf = engine.PeekOutput("output_paf");
        out_conf = engine.PeekOutput("output_conf");
    }

    void CreateJoints()
    {
        double val;
        for (int i = 0; i < outputWidth; i++)
        {
            for (int j = 0; j < outputHeight; j++)
            {
                for (int k = 0; k < jointSize; k++)
                {
                    val = out_conf[0, j, i, k];
                    if (val < 0.6 && val > 0.5)
                    {
                        int x = (int)(i * inputWidth / outputWidth);
                        int y = inputHeight - (int)(j * inputHeight / outputHeight) - 1;
                        for (int n=-1; n<2; n++)
                        {
                            for (int m=-1; m<2; m++)
                            {
                                frame.SetPixel(x + m, y + n, pen[k]);
                            }
                        }
                    }
                }
            }
        }
        out_paf.Dispose();
        out_conf.Dispose();
    }

    void WriteImage()
    {
        byte[] _bytes = frame.EncodeToPNG();
        File.WriteAllBytes("Assets/Images/out_coco.png", _bytes);
    }

}

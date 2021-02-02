using UnityEngine;
using UnityEngine.UI;
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

public class OpenPose : MonoBehaviour
{
    int camHeight = 720;
    int camWidth = 1280;
    int inputHeight = 368;
    int inputWidth = 656;
    int inputChannel = 3;
    int outputHeight = 46;
    int outputWidth = 82;
    int jointSize = 14;
    double threshold = 0.2;

    Tensor input;
    Tensor out_paf;
    Tensor out_conf;
    Texture2D frame;
    Model model;
    IWorker engine;
    Color[] pen;
    Color c;
    WebCamTexture webcamTexture;
    RawImage rawImage;

    private void Awake()
    {
        /*frame = new Texture2D(656, 369, TextureFormat.RGBA32, false);
        byte[] bytes = File.ReadAllBytes("Assets/Images/image2.png");
        frame.LoadImage(bytes);*/

        rawImage = GetComponent<RawImage>();
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
            new Color(1, 0.5f, 1, 1),
            new Color(1, 1, 0.5f, 1),
            new Color(0.5f, 1, 1, 1),
            new Color(0.5f, 0.5f, 1, 1),
            new Color(0.5f, 1, 0.5f, 1),
            new Color(1, 0.5f, 0.5f, 1),
        };
    }

    void Start()
    {
        if (webcamTexture == null)
            webcamTexture = new WebCamTexture();
        webcamTexture.deviceName = WebCamTexture.devices[1].name;
        if (!webcamTexture.isPlaying)
            webcamTexture.Play();
        camHeight = webcamTexture.height;
        camWidth = webcamTexture.width;
        StartCoroutine(RunPoseEstimation());
    }

    void Update()
    {

    }

    IEnumerator RunPoseEstimation()
    {
        while (true)
        {
            SetFrame();
            ExecuteModel();
            CreateJoints();
            frame.Apply();
            rawImage.texture = frame;
            //WriteImage();
            yield return new WaitForSeconds(1);
        }
    }

    void SetFrame()
    {
        frame = new Texture2D(camWidth, camHeight, TextureFormat.RGBA32, false);
        frame.SetPixels(0, 0, camWidth, camHeight, webcamTexture.GetPixels());
        TextureScale.Bilinear(frame, 656, 369);
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
        double val_conf;
        double val_paf1;
        double val_paf2;
        int x, y;
        for (int i = 0; i < outputWidth; i++)
        {
            for (int j = 0; j < outputHeight; j++)
            {
                for (int k = 0; k < jointSize; k++)
                {
                    val_conf = out_conf[0, j, i, k];
                    val_paf1 = out_paf[0, j, i, 2 * k];
                    val_paf2 = out_paf[0, j, i, 2 * k + 1];
                    x = (int)(i * inputWidth / outputWidth);
                    y = inputHeight - (int)(j * inputHeight / outputHeight) - 1;
                    if (val_conf > threshold)
                    {
                        for (int n=-1; n<2; n++)
                        {
                            for (int m=-1; m<2; m++)
                            {
                                frame.SetPixel(x + m, y + n, pen[k]);
                            }
                        }
                    }
                    else if (val_paf1 > threshold || val_paf2 > threshold)
                    {
                        frame.SetPixel(x, y, pen[7]);
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

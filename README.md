# PoseEstimationUnity

This repository is a Unity module for identifying human pose and rendering it in real-time.  
We use the compressed model of [OpenPose](https://arxiv.org/pdf/1812.08008.pdf) and use ONNX 
to decompress the model. 
The module is connected to your webcam and feeds the video stream data to the model. The pose 
is rendered on a display in real-time.

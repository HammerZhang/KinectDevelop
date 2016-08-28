using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Threading;
using System.IO;

namespace KinectAudioEx1
{
    class Program
    {
        static void Main(string[] args)
        {
            KinectSensor kinectDevice = (from sensors in KinectSensor.KinectSensors
                                         where sensors.Status == KinectStatus.Connected
                                         select sensors).FirstOrDefault();
            if (kinectDevice == null)
            {
                Console.WriteLine("No Kinect connected! \n" +
                    "Press any key to continue");
                Console.ReadKey(true);
                return;
            }

            try
            {
                kinectDevice.Start();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString() + "Press any to continue! \n");
                Console.ReadKey(true);
                return;
            }

            const int voiceRecordTime = 20;
            const int voiceRecordLength = voiceRecordTime * 2 * 16000;
            const string outputFileName = "RecordedVoice.wav";
            var voiceBuffer = new byte[4096];

            // 配置一个kinect音频源对象
            KinectAudioSource audioSource = kinectDevice.AudioSource;
            audioSource.AutomaticGainControlEnabled = false;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            audioSource.BeamAngleChanged += new 
                EventHandler<BeamAngleChangedEventArgs>(audioSource_BeamAngleChanged);

            using(var filestream = new FileStream(outputFileName,FileMode.Create))
            {
                //WriteWavHeader(filestream, voiceRecordLength);
                Console.WriteLine("Recording for {0} seconds...", voiceRecordTime);

                using(var audioStream = audioSource.Start())
                {
                    int count = 0;
                    int totalCount = 0;

                    while (totalCount < voiceRecordLength && 
                        (count = audioStream.Read(voiceBuffer,0,voiceBuffer.Length)) > 0)
                    {
                        filestream.Write(voiceBuffer,0,count);
                        totalCount += count;

                        if (audioSource.SoundSourceAngleConfidence > 0.85)
                        {
                            Console.Write("Sound source position (r):{0}\t\tBeam:{1}\r",
                                audioSource.SoundSourceAngle, audioSource.BeamAngle);
                        }
                    }
                }
            }
        }

        private static void audioSource_BeamAngleChanged(object sender, BeamAngleChangedEventArgs e)
        {
            Console.WriteLine("Beam angle changed:{0}", e.Angle);
        }
    }
}

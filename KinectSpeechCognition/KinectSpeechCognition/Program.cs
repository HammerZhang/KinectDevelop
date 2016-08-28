using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.IO;

namespace KinectSpeechCognition
{
    class Program
    {
        static void Main(string[] args)
        {
            KinectSensor kinectDevice = (from sensor in KinectSensor.KinectSensors
                                         where sensor.Status == KinectStatus.Connected
                                         select sensor).FirstOrDefault();

            if (kinectDevice == null)
            {
                Console.WriteLine("No device connected \n" + "Press any key to continue");
                Console.ReadKey(true);
                return;
            }

            kinectDevice.Start();

            // 配置一个声源
            KinectAudioSource audioSource = kinectDevice.AudioSource;
            audioSource.EchoCancellationMode = EchoCancellationMode.None;
            audioSource.AutomaticGainControlEnabled = false;

            // 创建一个语音识别引擎
            RecognizerInfo recognizerInfo = GetKinectRecognizer();
            using (var speechRecognitionEngine = new SpeechRecognitionEngine(recognizerInfo))
            {
                var colors = new Choices();
                colors.Add("Green");
                colors.Add("Red");
                colors.Add("Blue");

                var grammerBuilder = new GrammarBuilder();
                grammerBuilder.Append(colors);
                var grammer = new Grammar(grammerBuilder);

                speechRecognitionEngine.LoadGrammar(grammer);
                speechRecognitionEngine.SpeechRecognized += speechRecognitionEngine_SpeechRecognized;
                speechRecognitionEngine.SpeechHypothesized += speechRecognitionEngine_SpeechHypothesized;
                speechRecognitionEngine.SpeechRecognitionRejected += speechRecognitionEngine_SpeechRecognitionRejected;
           
                using(Stream s = audioSource.Start())
                {
                    speechRecognitionEngine.SetInputToAudioStream(
                        s,new SpeechAudioFormatInfo(EncodingFormat.Pcm,16000,16,1,
                            32000,2,null));
                    Console.WriteLine("Recognizing speech, say 'red','blue','green', Press any key to stop");

                    speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
                    Console.ReadLine();
                    Console.WriteLine("Stopping recognizer....");
                    speechRecognitionEngine.RecognizeAsyncStop();
                }
            }
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
                {
                    string value;
                    r.AdditionalInfo.TryGetValue("Kinect", out value);
                    return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase)
                        && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
                };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        private static void speechRecognitionEngine_SpeechRecognitionRejected(object sender, 
            SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine("\n Speech rejected");
            if (e.Result != null)
            {
                //DumpRecordedAudio(e.Result.Audio);
            }
        }

        private static void speechRecognitionEngine_SpeechRecognized(object sender,
            SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > 0.7)
            {
                Console.WriteLine("Speech recognized : \t{0}\t Confidence: \t{1}",
                    e.Result.Text, e.Result.Confidence);
            }
            else
            {
                Console.WriteLine("Speech Recognized but Confidence are too low: \t{0}",
                    e.Result.Confidence);
            }
        }

        private static void speechRecognitionEngine_SpeechHypothesized(object sender,
            SpeechHypothesizedEventArgs e)
        {
            Console.WriteLine("Speech Hypothesized: \t{0}", e.Result.Text);
        }
    }
}

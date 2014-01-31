using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using LIFX;
using System.IO;
using System.Threading;

namespace Lightspeak
{
    class Program
    {
        private KinectSensor sensor;
        private SpeechRecognitionEngine speechEngine;
        LIFXNetwork Network = new LIFXNetwork();
        UInt16 lightLevel = 0;
        UInt16 color = 0;
        UInt16 saturation = UInt16.MaxValue;
        UInt16 oldLightLevel = 0;

        static void Main(string[] args)
        {
            new Program().SetUpKinect();
            Console.WriteLine("Ready");
            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        private RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }
        /// <summary>
        /// Handler for recognized speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            const double ConfidenceThreshold = 0.7;

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                switch (e.Result.Semantics.Value.ToString())
                {
                    case "RED":
                        color = 0;
                        saturation = UInt16.MaxValue;
                        break;
                    case "BLUE":
                        color = 48000;
                        saturation = UInt16.MaxValue;
                        break;
                    case "GREEN":
                        color = 28000;
                        saturation = UInt16.MaxValue;
                        break;
                    case "WHITE":
                        color = 0;
                        saturation = 0;
                        break;
                    case "ON":
                        lightLevel = oldLightLevel;
                        break;
                    case "OFF":
                        oldLightLevel = lightLevel;
                        lightLevel = 0;
                        break;
                    case "BRIGHTER":
                        if (lightLevel < 54000)
                        {
                            lightLevel += 10000;
                        }
                        else
                        {
                            lightLevel = UInt16.MaxValue;
                        }
                        break;
                    case "DIMMER":
                        if (lightLevel > 10000)
                        {
                            lightLevel -= 10000;
                        }
                        else
                        {
                            lightLevel = 0;
                        }
                        break;
                }
                foreach (LIFXBulb bulb in Network.bulbs)
                {
                    Network.SetBulbValues(color, saturation, lightLevel, 4000, 10, bulb);
                }
            }
        }
        private void SetUpKinect()
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                try
                {
                    // Start the sensor!
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    // Some other application is streaming from the same Kinect sensor
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                return;
            }

            RecognizerInfo ri = GetKinectRecognizer();

            if (null != ri)
            {
                this.speechEngine = new SpeechRecognitionEngine(ri.Id);
                // Create a grammar from grammar definition XML file.
                using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.SpeechGrammar)))
                {
                    var g = new Grammar(memoryStream);
                    speechEngine.LoadGrammar(g);
                }
                speechEngine.SpeechRecognized += SpeechRecognized;
                speechEngine.UpdateRecognizerSetting("AdaptationOn", 0);
                speechEngine.SetInputToAudioStream(
                    sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            Network.Start();
            Console.WriteLine("Bulbs Found");
            Thread.Sleep(1000);
            lightLevel = Network.bulbs[0].Brightness;
            color = Network.bulbs[0].Hue;
            saturation = Network.bulbs[0].Saturation;
        }
    }
}

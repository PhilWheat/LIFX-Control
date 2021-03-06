﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.AudioFormat;
//using Microsoft.Speech.AudioFormat;
//using Microsoft.Speech.Recognition;
using System.IO;
using System.Threading;
using LIFX;

namespace Lightspeak
{
    class Program
    {
        private SpeechRecognitionEngine speechEngine;
        LIFXNetwork Network = new LIFXNetwork();
        UInt16 lightLevel = 0;
        UInt16 color = 0;
        UInt16 saturation = UInt16.MaxValue;
        UInt16 oldLightLevel = 0;

        static void Main(string[] args)
        {
            new Program().SetUpSpeech();
            Console.WriteLine("Ready");
            while (true)
            {
                Thread.Sleep(1000);
            }
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
        private void SetUpSpeech()
        {

            SpeechRecognitionEngine se = new SpeechRecognitionEngine();
            se.SetInputToDefaultAudioDevice();
            RecognizerInfo ri = se.RecognizerInfo;

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
                //speechEngine.SetInputToAudioStream(
                //    sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                speechEngine.SetInputToDefaultAudioDevice();
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

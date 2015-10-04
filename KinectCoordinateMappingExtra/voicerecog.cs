using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;


using System.IO;

namespace KinectCoordinateMapping
{
    class voicerecog
    {
        //hella rough speech recognition
    //may need a grammar.xaml

    //cobbled together from "http://kin-educate.blogspot.co.uk/2012/06/speech-recognition-for-kinect-easy-way.html"
    // and "https://github.com/zubairdotnet/KinectSpeechColorWPF"

  

    //Create an instance of the sensor and recognition engine
    public KinectSensor CurrentSensor;
    private  SpeechRecognitionEngine speechRecognizer;

    //Get speech recogniser
    private static RecognizerInfo GetKinectRecognizer()
    {
	    IEnumerable<RecognizerInfo> recognizers;
	
        recognizers = SpeechRecognitionEngine.InstalledRecognizers();

	    foreach (RecognizerInfo recognizer in recognizers)
	    {
		    string value;
		    recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
		    if ("True".Equals(value, StringComparison.InvariantCultureIgnoreCase) &&
			    "en-GB".Equals(recognizer.Culture.Name, StringComparison.InvariantCultureIgnoreCase))
		    {
			    return recognizer;
		    }

	    }
	    return null;
    }




    //Initialise the Kinect
    private KinectSensor InitialiseKinect()
    {
	    //set the current sensor to the first available
	    CurrentSensor = KinectSensor.GetDefault();
	    speechRecognizer = CreateSpeechRecognizer();


	    //start sensor
	    CurrentSensor.Open();

	    //start streaming audio
	    Starter();
	    return CurrentSensor;
    }


    //Start streaming audio
    private void Starter()
    {
	    var audioSource = CurrentSensor.AudioSource;


	    //start audiosource
        var kinectStream = audioSource.

        IReadOnlyList<AudioBeam> audioBeamList = kinectStream;
        System.IO.Stream audioStream = audioBeamList[0].OpenInputStream();

	    //configure audio stream
	    speechRecognizer.SetInputToAudioStream(kinectStream, new SpeechAudioFormatInfo(
	    EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));

	    //keep the recogniser going
	    speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);

    }


    //Speech recognizer
    private SpeechRecognitionEngine CreateSpeechRecognizer()
    {
	    RecognizerInfo ri = GetKinectRecognizer();

	    SpeechRecognitionEngine sre;
	    sre = new SpeechRecognitionEngine(ri.Id);

	    //words we need the program to recognise
	    var grammar = new Choices();
	    grammar.Add(new SemanticResultValue("moustache", "MOUSTACHE"));
	    grammar.Add(new SemanticResultValue("top hat", "TOP HAT"));
	    grammar.Add(new SemanticResultValue("glasses", "GLASSES"));
	    grammar.Add(new SemanticResultValue("sunglasses", "SUNGLASSES"));
	    grammar.Add(new SemanticResultValue("tie", "TIE"));
	    grammar.Add(new SemanticResultValue("bow", "BOW"));
	    grammar.Add(new SemanticResultValue("bear", "BEAR"));
	    //etc

	    var gb = new GrammarBuilder { Culture = ri.Culture };
	    gb.Append(grammar);

	    var g = new Grammar(gb);
	    sre.LoadGrammar(g);

	    //Events for recognising and rejecting speech
	    sre.SpeechRecognized += SreSpeechRecognized;
	    sre.SpeechRecognitionRejected += SreSpeechRecognitionRejected;
	    return sre;
    }


    //speech rejected
    private void RejectSpeech(RecognitionResult result)
    {
	    //something here
    }

    private void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
    {
	    RejectSpeech(e.Result);
    }


    //Speech recognised
    private void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
	    const double ConfidenceThreshold = 0.4;


	    //confidence level of recogniton, high values are highly accurate
	    if (e.Result.Confidence < ConfidenceThreshold)
	    {
		    RejectSpeech(e.Result);
	    }

	    //outcomes of recognised speech
	    switch (e.Result.Semantics.Value.ToString())
	    {
		    case "MOUSTACHE":
			    // summon moustache
			    break;
		    case "TOP HAT":
			    //summon top hat
			    break;
		    case "GLASSES":
			    //glasses
			    break;
		    case "SUNGLASSES":
			    //sunglasses
			    break;
		    case "TIE":
			    //tie
			    break;
		    case "BOW":
			    //bow
			    break;
		    case "BEAR":
			    //bear
			    break;
		    //etc
		    default:
			    break;
	    }
    }
    }
}

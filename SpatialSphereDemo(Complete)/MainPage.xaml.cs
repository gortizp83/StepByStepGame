using Microsoft.PartnerCatalyst.IntelligentHeadset;
using Microsoft.PartnerCatalyst.NavigationSensorPeripheral;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
//using osc.net;
//using System.Net;

namespace SpatialSphereDemo
{
    public sealed partial class MainPage : Page
    {
        
        /// <summary>
        ///1. Configure all spatial audio positions
        /// </summary>
        private SpatialSoundSource[] _spatialSounds = new SpatialSoundSource[] {      
            //Go around the sphere horizontally
            // Left-/Right+,Up+/Down-,Front-/Back+
            new SpatialSoundSource (new Vector3( 0f, -1f, -4f), "Files/Audio/FrontPosition.wav",       1f,   new Point(298d,208d)),
            new SpatialSoundSource (new Vector3( 3f, -1f, -3f), "Files/Audio/FrontRightPosition.wav",  1f,   new Point(417d,215d)),
            new SpatialSoundSource (new Vector3( 4f, -1f,  0f), "Files/Audio/RightPosition.wav",       1f,   new Point(543d,270d)),
            new SpatialSoundSource (new Vector3( 3f, -1f,  3f), "Files/Audio/RearRightPosition.wav",   1f,   new Point(417d,347d)),
            new SpatialSoundSource (new Vector3(-3f, -1f,  3f), "Files/Audio/RearLeftPosition.wav",    1f,   new Point(151d,345d)),
            new SpatialSoundSource (new Vector3(-4f, -1f,  0f), "Files/Audio/LeftPosition.wav",        1f,   new Point(5d,270d)),
            new SpatialSoundSource (new Vector3(-3f, -1f, -3f), "Files/Audio/FrontLeftPosition.wav",   1f,   new Point(149d,215d)),
            new SpatialSoundSource (new Vector3( 0f, -1f, -4f), "Files/Audio/FrontPosition.wav",       1f,   new Point(298d,208d)),

            //Go around the sphere vertically
            // Left-/Right+,Up+/Down-,Front-/Back+
            new SpatialSoundSource (new Vector3( 0f,  4f, -0f),  "Files/Audio/TopPosition.wav",        1f,   new Point(280d,0d)),
            new SpatialSoundSource (new Vector3( 3f,  3f, -0f),  "Files/Audio/TopRightPosition.wav",   1f,   new Point(454d,79d)),
            new SpatialSoundSource (new Vector3( 4f, -1f,  0f),  "Files/Audio/RightPosition.wav",      1f,   new Point(545d,269d)),
            new SpatialSoundSource (new Vector3( 4f, -4f,  0f),  "Files/Audio/BottomRightPosition.wav",1f,   new Point(495d,440d)),
            new SpatialSoundSource (new Vector3(-4f, -4f,  0f),  "Files/Audio/BottomLeftPosition.wav", 1f,   new Point(66d,445d)),
            new SpatialSoundSource (new Vector3(-4f, -1f,  0f),  "Files/Audio/LeftPosition.wav",       1f,   new Point(5d,270d)),
            new SpatialSoundSource (new Vector3(-3f,  3f, -0f),  "Files/Audio/TopLeftPosition.wav",    1f,   new Point(97d,78d)),
            new SpatialSoundSource (new Vector3( 0f,  4f, -0f),  "Files/Audio/TopPosition.wav",        1f,   new Point(280d,0d)),
        };
        private AudioDeviceOutputNode _deviceOutput;
        private AudioGraph _graph;
        private IntelligentHeadsetSensorPeripheral _intelligentHeadsetSensorPeripheral;
        private AudioFileInputNode _currentAudioFileInputNode = null;
        private int _spatialSoundsIdx = 0;
        private GraphStateEnum _graphState;
        private AudioSubmixNode _submixNode = null;
        private double scaleFactor = 1;



        /// <summary>
        /// 2. This page is loaded, look and see if there is an available Intelligent Headset.
        /// </summary>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            //if (client == null)
            {
            }
            base.OnNavigatedTo(e);

            this.SizeChanged += MainPage_SizeChanged;

            var intelligentHeadsetSensorDevices = await IntelligentHeadsetSensorPeripheral.FindDevicesAsync();
            var intelligentHeadsetSensorDevice = intelligentHeadsetSensorDevices?.FirstOrDefault();
            if (intelligentHeadsetSensorDevice != null)
            {
                _intelligentHeadsetSensorPeripheral = new IntelligentHeadsetSensorPeripheral(intelligentHeadsetSensorDevice);
            }

            GraphState = GraphStateEnum.NotReady;
        }

        /// <summary>
        /// 3. Stop using the Intelligent Headset when navigating away from the page.
        /// </summary>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (_intelligentHeadsetSensorPeripheral != null)
                _intelligentHeadsetSensorPeripheral.Detach();
        }

        /// <summary>
        /// 4. When a user clicks on the start button, initialize the headset, calibrate the headset
        ///    build the AudioGraph, and Start the graph.
        /// </summary>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (this.GraphState)
            {
                case GraphStateEnum.NotReady:
                    {
                        await AttachAndCallibrateIntelligentHeadset();
                        await BuildAndStartAudioGraph();
                        break;
                    }
                case GraphStateEnum.Paused:
                    {
                        this.GraphState = GraphStateEnum.Playing;
                        break;
                    }
                case GraphStateEnum.Playing:
                    {
                        this.GraphState = GraphStateEnum.Paused;
                        break;
                    }
            }
        }
   
        /// <summary>
        /// 5. Build and Start the AudiGraph
        /// </summary>
        /// <returns></returns>
        private async Task BuildAndStartAudioGraph()
        {
            _spatialSoundsIdx = 0;

            //Initialize AudioGraph settings
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.GameEffects);
            settings.EncodingProperties = AudioEncodingProperties.CreatePcm(48000, 2, 32);
            settings.EncodingProperties.Subtype = MediaEncodingSubtypes.Float;
            settings.DesiredRenderDeviceAudioProcessing = Windows.Media.AudioProcessing.Raw;

            //Create AudioGraph
            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);

            if (result.Status == AudioGraphCreationStatus.Success)
            {
                _graph = result.Graph;
                //Attach a DeviceOutput node to the AudioGraph
                CreateAudioDeviceOutputNodeResult deviceResult = await _graph.CreateDeviceOutputNodeAsync();

                if (deviceResult.Status == AudioDeviceNodeCreationStatus.Success)
                {
                    _deviceOutput = deviceResult.DeviceOutputNode;
                    
                    //Create the emitter.
                    AudioNodeEmitter emitter = new AudioNodeEmitter(
                        AudioNodeEmitterShape.CreateOmnidirectional(),
                        AudioNodeEmitterDecayModels.CreateNatural(),
                        AudioNodeEmitterSettings.None);
                    emitter.Position = _spatialSounds[0].Position;
                    //Create the AudioFileInputNode
                    CreateAudioFileInputNodeResult inCreateResult = await _graph.CreateFileInputNodeAsync(await GetSoundFileAsync(0), emitter);
                    _currentAudioFileInputNode = inCreateResult.FileInputNode;
                    //Attach the AudioFileInputNode to the DeviceOutput in the Graph
                    _currentAudioFileInputNode.AddOutgoingConnection(_deviceOutput);
                    //When the file is done playing, update the graph with a new emitter for the next spatial sound 
                    _currentAudioFileInputNode.FileCompleted += CurrentAudioFileInputNode_FileCompleted;

                    _currentAudioFileInputNode.OutgoingGain = 5;

                    //Create the submix node for the reverb
                    _submixNode = _graph.CreateSubmixNode();
                    _submixNode.OutgoingGain = 0.125d;
                    //Add the submix node to the DeviceOutput.
                    _submixNode.AddOutgoingConnection(_deviceOutput);
                    //Add Reverb to the Submix
                    ReverbEffectDefinition reverb = ReverbEffectDefinitionFactory.GetReverbEffectDefinition(_graph, ReverbEffectDefinitions.SmallRoom);
                    _submixNode.EffectDefinitions.Add(reverb);
                    // Add the AudioFileInputNode to the submix
                    _currentAudioFileInputNode.AddOutgoingConnection(_submixNode);

                    //Show the position of the active sound
                    Canvas.SetLeft(this.ActiveSound, _spatialSounds[0].ImageResourceLocation.X * scaleFactor);
                    Canvas.SetTop(this.ActiveSound, _spatialSounds[0].ImageResourceLocation.Y * scaleFactor);
                    ActiveSound.Visibility = Visibility.Visible;

                    //Start the graph
                    _graph.Start();
                    GraphState = GraphStateEnum.Playing;
                }
            }
        }

        /// <summary>
        /// 6. Called when the current AudioFileInputNode completed playing.
        /// </summary>
        private async void CurrentAudioFileInputNode_FileCompleted(AudioFileInputNode sender, object args)
        {
            await Task.Delay(new TimeSpan(0, 0, 1));
            _currentAudioFileInputNode.FileCompleted -= CurrentAudioFileInputNode_FileCompleted;
            UpdateEmitter();
        }

        /// <summary>
        /// 7. Called when the Yaw, Pitch Roll for the Intelligent Headset changes.
        /// </summary>
        private async void IntelligentHeadsetSensorPeripheral_GyroChanged(INavigationSensorPeripheral sender, GyroChangedEventArgs args)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                //Update the TopHead Image position
                UpdateTopHeadPosition(args);
                //Update the Listener Orientation in the graph
                await UpdateListenerOrientation();
            });
        }

        private async void _intelligentHeadsetSensorPeripheralGPS_GeocoordinateChanged(INavigationSensorPeripheral sender, GeocoordinateChangedEventArgs args)
        {
            double lat = args.SensorGeocoordinate.Latitude;
            double lon = args.SensorGeocoordinate.Longitude;

            //this.ListenerLat.Text = lat.ToString("0.0") + " N";
            //this.ListenerLon.Text = lon.ToString("0.0") + " E";

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                double lat2 = args.SensorGeocoordinate.Latitude;
                double lon2 = args.SensorGeocoordinate.Longitude;

                this.ListenerLat.Text = lat2.ToString("0.0") + " N";
                this.ListenerLon.Text = lon2.ToString("0.0") + " E";
                //Update the TopHead Image position
                //UpdateTopHeadPosition(args);
                //Update the Listener Orientation in the graph
                //await UpdateListenerOrientation();
            });
        }

        /// <summary>
        /// 8. Update the listener position
        /// </summary>
        private async Task UpdateListenerOrientation()
        {
            //Get the current listener position from the IMU
            Vector4 listenerOrientation = await GetHeadingYawPitchRollAsync();
            //Vector2 GPSData = await GetGPS();
            //Vector3 listenerOrientation = await GetYawPitchRollAsync();
            
            //Update the listener only if the graph is attached to a device output node
            if (_deviceOutput != null)
            {
                //Convert yaw,pitch,roll in degrees from the IMU to radial angles
                Vector3 radialListnerOrientation = new Vector3((float)(Math.PI * listenerOrientation.X / 180f), (float)(Math.PI * listenerOrientation.Y / 180f), (float)(Math.PI * listenerOrientation.Z / 180f));
                //Create a Quaternion from the Radial yaw,pitch,roll IMU  orientation 
                //Quaternion  is a four-dimensional vector (x,y,z,w), which is used to efficiently rotate an object about the (x, y, z) vector by the angle theta.
                //float heading = listenerOrientation.W; // should be degrees from north?
                //Debug.WriteLine(heading);
                //Debug.WriteLine("asd");
                Quaternion q = Quaternion.Normalize(Quaternion.CreateFromYawPitchRoll((float)radialListnerOrientation.X, (float)radialListnerOrientation.Y, (float)radialListnerOrientation.Z));
                _deviceOutput.Listener.Orientation = q;
            }
        }

        /// <summary>
        /// 9. Update the emitter position when Spatial Sound originating position.
        /// </summary>
        private async void UpdateEmitter()
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (GraphState == GraphStateEnum.Playing)
                    _spatialSoundsIdx++;

                if (_spatialSoundsIdx < _spatialSounds.Length)
                {
                    ActiveSound.Visibility = Visibility.Collapsed;

                    //Create the AudioNodeEmitter
                    AudioNodeEmitter emitter = new AudioNodeEmitter(AudioNodeEmitterShape.CreateOmnidirectional(), AudioNodeEmitterDecayModels.CreateNatural(), AudioNodeEmitterSettings.None);
                    emitter.Position = _spatialSounds[_spatialSoundsIdx].Position;
                    //Create a new AudioFileInputNode and add an emitter to it
                    CreateAudioFileInputNodeResult inCreateResult = await _graph.CreateFileInputNodeAsync(await GetSoundFileAsync(_spatialSoundsIdx), emitter);
                    inCreateResult.FileInputNode.OutgoingGain = 5;// _spatialSounds[_spatialSoundsIdx].Gain;
                    //Add the new AudioFileInputNode to the AudioGraph
                    inCreateResult.FileInputNode.AddOutgoingConnection(_deviceOutput);
                    inCreateResult.FileInputNode.AddOutgoingConnection(_submixNode);
                    //Subscribe to the FileCompleted event so that we can go to the next spatial sound
                    inCreateResult.FileInputNode.FileCompleted += CurrentAudioFileInputNode_FileCompleted;

                    //Remove the old AudioFileOutputNode from the AudioGraph
                    _currentAudioFileInputNode.RemoveOutgoingConnection(_deviceOutput);
                    _currentAudioFileInputNode.RemoveOutgoingConnection(_submixNode);

                    //Set the current node
                    _currentAudioFileInputNode = inCreateResult.FileInputNode;

                    // Update the visual indicater 
                    Canvas.SetLeft(this.ActiveSound, _spatialSounds[_spatialSoundsIdx].ImageResourceLocation.X * scaleFactor);
                    Canvas.SetTop(this.ActiveSound, _spatialSounds[_spatialSoundsIdx].ImageResourceLocation.Y * scaleFactor);
                    ActiveSound.Visibility = Visibility.Visible;
                }
                else
                {
                    this.GraphState = GraphStateEnum.Stopped;

                    _graph.Stop();
                    _graph = null;
                }
            });
        }

        #region Helper Functions

        /// <summary>
        /// State variable that controls the UI.
        /// </summary>
        public GraphStateEnum GraphState
        {
            get { return _graphState; }
            set
            {
                _graphState = value;
                switch (_graphState)
                {
                    case (GraphStateEnum.NotReady):
                        {
                            GraphButton.Content = "Start";
                            GraphButton.IsEnabled = true;
                            break;
                        }
                    case (GraphStateEnum.Ready):
                        {
                            GraphButton.Content = "Start";
                            GraphButton.IsEnabled = true;
                            break;
                        }
                    case (GraphStateEnum.Paused):
                        {
                            GraphButton.Content = "Resume";
                            GraphButton.IsEnabled = true;
                            break;
                        }
                    case (GraphStateEnum.Playing):
                        {
                            GraphButton.Content = "Pause";
                            GraphButton.IsEnabled = true;
                            break;
                        }
                    case (GraphStateEnum.Stopped):
                        {
                            GraphButton.Content = "Done!";
                            GraphButton.IsEnabled = false;
                            break;
                        }
                }
            }
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double dim = e.NewSize.Width - 150 > 595 ? 595 : e.NewSize.Width - 150;

            backgroundImage.Width = dim;
            backgroundImage.Height = dim;
            middlegroudndImage.Width = dim;
            middlegroudndImage.Height = dim;
            foregroudndImage.Width = dim;
            foregroudndImage.Height = dim;

            scaleFactor = dim / 595d;
            ActiveSound.Width = 45d * scaleFactor;
            ActiveSound.Height = 45d * scaleFactor;

            //Show the position of the active sound
            Canvas.SetLeft(this.ActiveSound, _spatialSounds[0].ImageResourceLocation.X * scaleFactor);
            Canvas.SetTop(this.ActiveSound, _spatialSounds[0].ImageResourceLocation.Y * scaleFactor);
        }


        /// <summary>
        /// Default Constructor
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void UpdateTopHeadPosition(GyroChangedEventArgs args)
        {
            //Update the top Head graphic
            TopHeadPlaneProjection.RotationZ = 360 - args.Yaw;
            TopHeadPlaneProjection.RotationX = 360 - args.Pitch;
            TopHeadPlaneProjection.RotationY = 360 - args.Roll;

            //Update the textblocks with the IMU orientation values
            this.ListenerYaw.Text = args.Yaw.ToString("0.0") + " degrees";
            this.ListenerPitch.Text = args.Pitch.ToString("0.0") + " degrees";
            this.ListenerRoll.Text = args.Roll.ToString("0.0") + " degrees";
            this.ListenerCompass.Text = args.Compass.ToString("0.0") + " degrees";
            //string tosend = "" + this.ListenerYaw.Text + "," + this.ListenerPitch.Text + "," + this.ListenerRoll.Text;
            //var message = new SharpOSC.OscMessage("/test/1", (float)args.Yaw, (float)args.Pitch, (float)args.Roll);
            //var sender = new SharpOSC.UDPSender("127.0.0.1", 55555);
            //sender.Send(message);
        }


        /// <summary>
        /// Read the current yaw, picth, roll position from IMU or (0,0,0) if no IMU is connected
        /// </summary>
        /// <returns></returns>
        private async Task<Vector4> GetHeadingYawPitchRollAsync()
        {
            if (_intelligentHeadsetSensorPeripheral != null && _intelligentHeadsetSensorPeripheral.Connected)
            {
                CurrentIntelligentHeadsetReading reading = await _intelligentHeadsetSensorPeripheral.GetCurrentReadingAsync();
                if (reading == null)
                    return new Vector4(0, 0, 0, 0);

                return new Vector4((float)reading.CurrentComboHprReading.CompassHeading, 360f - (float)reading.CurrentComboHprReading.Yaw, 360f - (float)reading.CurrentComboHprReading.Pitch, 360f - (float)reading.CurrentComboHprReading.Roll);
            }
            else
            {
                return new Vector4(0, 0, 0, 0);
            }
        }

        //private async Task<Vector2> GetGPS()
        //{
        //    if (_intelligentHeadsetSensorPeripheralGPS != null && _intelligentHeadsetSensorPeripheralGPS.Connected)
        //    {
        //        if (reading == null)
        //            return new Vector2(0, 0);

        //        return new Vector2(360f - (float)reading.CurrentComboHprReading.Yaw, 360f - (float)reading.CurrentComboHprReading.Pitch, 360f - (float)reading.CurrentComboHprReading.Roll);
        //    }
        //    else
        //    {
        //        return new Vector2(0, 0);
        //    }
        //}

        /// <summary>
        /// Gets the spatial sound file for an index in the list of spatial sounds.
        /// </summary>
        /// <param name="idx">Index of the spatial sound</param>
        /// <returns>StorageFile for the spatial sound.</returns>
        private async Task<StorageFile> GetSoundFileAsync(int idx)
        {
            //Get the sound file 
            StorageFile soundFile = null;
            if (UseSingleSound.IsChecked == true)
                soundFile = await SpatialSoundSource.GetMonoSoundFile();
            else
                soundFile = await _spatialSounds[idx].GetSoundFileAsync();

            return soundFile;
        }

        /// <summary>
        /// Discover and Callibrate Intelligent Headset
        /// </summary>
        private async Task AttachAndCallibrateIntelligentHeadset()
        {
            // If we found an IntelligentHeadset that was paired.
            if (_intelligentHeadsetSensorPeripheral != null)
            {
                //Attach to the headset
                await _intelligentHeadsetSensorPeripheral.AttachAsync();
                //Wait for 2 seconds because it might take a sec
                await Task.Delay(new TimeSpan(0, 0, 2));
                if (_intelligentHeadsetSensorPeripheral.Connected)
                {
                    await _intelligentHeadsetSensorPeripheral.Calibrate();
                    _intelligentHeadsetSensorPeripheral.AccelerometerValueChanged += IntelligentHeadsetSensorPeripheral_AccelerometerValueChanged;
                    _intelligentHeadsetSensorPeripheral.GyroChanged += IntelligentHeadsetSensorPeripheral_GyroChanged;
                    _intelligentHeadsetSensorPeripheral.GeocoordinateChanged += _intelligentHeadsetSensorPeripheralGPS_GeocoordinateChanged;
                }
                else
                {
                    HeadTrackingPanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void IntelligentHeadsetSensorPeripheral_AccelerometerValueChanged(INavigationSensorPeripheral sender, AccelerometerValueChangedEventArgs args)
        {
            DetectSteps(args);

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                //Update the TopHead Image position
                UpdateAccelerometerPosition(args);
                //Update the Listener Orientation in the graph
                await UpdateListenerOrientation();
            });
        }

        private void DetectSteps(AccelerometerValueChangedEventArgs args)
        {
            float stepThreshold = 0.35f;

            var curr = args.CurrentAccelerometerReading;
            var prev = args.PreviousAccelerometerReading;
            var deltaX = prev.AccelerationX - curr.AccelerationX;
            var deltaY = prev.AccelerationY - curr.AccelerationY;
            var deltaZ = prev.AccelerationZ - curr.AccelerationZ;

            if (deltaY > stepThreshold)
            {
                TriggerStep();
            }
        }


        Timer Timer = null;
        int StepsCounted = 0;

        private void Timer_Elapsed(object state)
        {
            Timer.Dispose();
            Timer = null;
        }

        private void TriggerStep()
        {
            if (Timer == null)
            {
                StepsCounted++;
                Timer = new Timer(Timer_Elapsed, null, 1000, 0);
            }
        }

        private void UpdateAccelerometerPosition(AccelerometerValueChangedEventArgs args)
        {
            //Update the top Head graphic
            //TopHeadPlaneProjection.RotationZ = 360 - args.Yaw;
            //TopHeadPlaneProjection.RotationX = 360 - args.Pitch;
            //TopHeadPlaneProjection.RotationY = 360 - args.Roll;

            var curr = args.CurrentAccelerometerReading;
            var prev = args.PreviousAccelerometerReading;
            var deltaX = prev.AccelerationX - curr.AccelerationX;
            var deltaY = prev.AccelerationY - curr.AccelerationY;
            var deltaZ = prev.AccelerationZ - curr.AccelerationZ;

            //Update the textblocks with the IMU orientation values
            //this.ListenerAccelerometerX.Text = args.CurrentAccelerometerReading.AccelerationX.ToString("0.0");
            //this.ListenerAccelerometerY.Text = args.CurrentAccelerometerReading.AccelerationY.ToString("0.0");
            //this.ListenerAccelerometerZ.Text = args.CurrentAccelerometerReading.AccelerationZ.ToString("0.0");

            this.ListenerAccelerometerX.Text = deltaX.ToString("0.00");
            this.ListenerAccelerometerY.Text = deltaY.ToString("0.00");
            this.ListenerAccelerometerZ.Text = deltaZ.ToString("0.00");
            this.ListenerStepCount.Text = StepsCounted.ToString();

            Debug.WriteLine(deltaY.ToString());

            //string tosend = "" + this.ListenerYaw.Text + "," + this.ListenerPitch.Text + "," + this.ListenerRoll.Text;
            //var message = new SharpOSC.OscMessage("/test/1", (float)args.Yaw, (float)args.Pitch, (float)args.Roll);
            //var sender = new SharpOSC.UDPSender("127.0.0.1", 55555);
            //sender.Send(message);
        }


        #endregion
    }
}

namespace VideoApp
{
    using Android.App;
    using Android.Hardware;
    using Android.Media;
    using Android.OS;
    using Android.Util;
    using Android.Widget;

    using Java.IO;
    using Java.Lang;
    using Java.Text;
    using Java.Util;

    using Exception = System.Exception;

    [Activity(Label = "VideoApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private const int MediaTypeImage = 1;

        private const int MediaTypeVideo = 2;

        private const string MyAppName = "MyCameraApp";

        private Camera camera;

        private CameraPreview cameraPreview;

        private MediaRecorder mediaRecorder;

        private bool isRecording;

        public static Camera GetDefaultCamera()
        {
            Camera camera = null;

            try
            {
                camera = Camera.Open();
            }
            catch (Exception)
            {
                throw;
            }

            return camera;
        }

        public static Camera GetFrontCamera()
        {
            int numberOfCameras = Camera.NumberOfCameras;

            Camera camera = null;

            if (numberOfCameras > 1)
            {
                for (int i = 0; i < numberOfCameras; i++)
                {
                    var info = new Camera.CameraInfo();
                    Camera.GetCameraInfo(i, info);

                    if (info.Facing == CameraFacing.Front)
                    {
                        try
                        {
                            camera = Camera.Open(i);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }

            return camera;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            this.SetContentView(Resource.Layout.Main);

            // Create a Camera instance.
            this.camera = GetFrontCamera();

            // Create the preview view and set it as the content of this activity.
            if (this.camera != null)
            {
                // Set the recording hint to true.
                Camera.Parameters parameters = this.camera.GetParameters();
                parameters.SetRecordingHint(true);
                this.camera.SetParameters(parameters);

                this.cameraPreview = new CameraPreview(this, this.camera);
                FrameLayout previewLayout = this.FindViewById<FrameLayout>(Resource.Id.camera_preview);
                previewLayout.AddView(this.cameraPreview);
            }

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.btnCapture);

            button.Click += delegate
                {
                    if (this.isRecording)
                    {
                        // Stop the recording.
                        this.mediaRecorder.Stop();

                        // Release the media recorder.
                        this.ReleaseMediaRecorder();

                        // Take camera access back from media recorder.
                        this.camera.Lock();

                        this.isRecording = false;

                        // Inform the user that recording has stopped.
                        //button.SetText("Record", TextView.BufferType.Normal);
                    }
                    else
                    {
                        // Initialize the video camera.
                        if (this.PrepareVideoRecorder())
                        {
                            // Inform the user that recording has started.
                            //button.SetText("Stop", TextView.BufferType.Normal);

                            // Camera is available and unlocked and media recorder is prepared.
                            // At this point you can start recording video.
                            this.mediaRecorder.Start();

                            this.isRecording = true;
                        }
                        else
                        {
                            // Prepare did not work, so release the camera.
                            this.ReleaseMediaRecorder();
                            Log.Debug(MyAppName, "Prepare failed when clicking capture button.");
                        }
                    }
                };
        }

        protected override void OnPause()
        {
            base.OnPause();

            // Release the media recorder.
            this.ReleaseMediaRecorder();

            // Release the camera.
            this.ReleaseCamera();
        }

        /// <summary>
        /// Creates a File for saving an image or video.
        /// </summary>
        /// <param name="type">The media file type to be created.</param>
        /// <returns>An instance of File to use for saving media to.</returns>
        private static File GetOutputMediaFile(int type)
        {
            // TODO: Check that the SDCard is mounted using Environment.GetExternalStorageState() before doing this.
            // This location works best if you want the created images to be shared
            // between applications and persist after your app has been uninstalled.
            File mediaStorageDir = new File(
                Environment.GetExternalStoragePublicDirectory(
                Environment.DirectoryPictures),
                MyAppName);

            // Create the storage directory if it does not exist.
            if (!mediaStorageDir.Exists())
            {
                if (!mediaStorageDir.Mkdirs())
                {
                    Log.Debug(MyAppName, "failed to create directory");
                    return null;
                }
            }

            // Create a media file name.
            string timeStamp = new SimpleDateFormat("yyyyMMdd_HHmmss").Format(new Date());
            File mediaFile;

            if (type == MediaTypeImage)
            {
                mediaFile = new File(mediaStorageDir.Path + File.Separator + "IMG_" + timeStamp + ".jpg");
            }
            else if (type == MediaTypeVideo)
            {
                mediaFile = new File(mediaStorageDir.Path + File.Separator + "VID_" + timeStamp + ".mp4");
            }
            else
            {
                return null;
            }

            return mediaFile;
        }

        /// <summary>
        /// Prepares the media recorder for video recording by
        /// configuring recorder parameters in the proper order.
        /// </summary>
        /// <returns>True if the recorder is prepared for recording,
        /// False otherwise.</returns>
        private bool PrepareVideoRecorder()
        {
            //this.camera = GetFrontCamera();
            this.mediaRecorder = new MediaRecorder();

            // Step 1: Unlock and set the camera to MediaRecorder.
            this.camera.Unlock();
            this.mediaRecorder.SetCamera(this.camera);

            // Step 2: Set sources
            this.mediaRecorder.SetAudioSource(AudioSource.Camcorder);
            this.mediaRecorder.SetVideoSource(VideoSource.Camera);

            // Step 3: Set a CamcorderProfile (requires API Level 8 or higher).
            //this.mediaRecorder.SetProfile(CamcorderProfile.Get(CamcorderQuality.High));

            // When using the front facing camera we have to explicity set the output and encoding 
            // formats otherwise the start method of the media recorder throws an exception.
            this.mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
            this.mediaRecorder.SetAudioEncoder(AudioEncoder.Default);
            this.mediaRecorder.SetVideoEncoder(VideoEncoder.Default);

            // Step 4: Set output file.
            this.mediaRecorder.SetOutputFile(GetOutputMediaFile(MediaTypeVideo).ToString());

            // Step 5: Set the preview output.
            this.mediaRecorder.SetPreviewDisplay(this.cameraPreview.Holder.Surface);

            // Step 6: Prepare configured MediaRecorder.
            try
            {
                this.mediaRecorder.Prepare();
            }
            catch (IllegalStateException exception)
            {
                Log.Debug(MyAppName, string.Format("IllegalStateException preparing media recorder: {0}", exception.Message));
                this.ReleaseMediaRecorder();
                return false;
            }
            catch (IOException exception)
            {
                Log.Debug(MyAppName, string.Format("IOException preparing media recorder: {0}", exception.Message));
                this.ReleaseMediaRecorder();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Release the camera for other applications.
        /// </summary>
        private void ReleaseCamera()
        {
            if (this.camera != null)
            {
                this.camera.Release();
                this.camera = null;
            }
        }

        /// <summary>
        /// Releases resources used by the media recorder.
        /// </summary>
        private void ReleaseMediaRecorder()
        {
            if (this.mediaRecorder != null)
            {
                // Clear recorder configuration.
                this.mediaRecorder.Reset();

                // Release the recorder object.
                this.mediaRecorder.Release();

                this.mediaRecorder = null;

                // Lock the camera for later use.
                this.camera.Lock();
            }
        }
    }
}


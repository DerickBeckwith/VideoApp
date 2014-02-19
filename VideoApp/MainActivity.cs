namespace VideoApp
{
    using System;

    using Android.App;
    using Android.Hardware;
    using Android.Media;
    using Android.OS;
    using Android.Util;
    using Android.Widget;

    using Java.IO;
    using Java.Text;
    using Java.Util;

    using Uri = Android.Net.Uri;

    [Activity(Label = "VideoApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private const int MediaTypeImage = 1;

        private const int MediaTypeVideo = 2;

        private Camera camera;

        private CameraPreview cameraPreview;

        private MediaRecorder mediaRecorder;

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
                this.cameraPreview = new CameraPreview(this, this.camera);
                FrameLayout previewLayout = this.FindViewById<FrameLayout>(Resource.Id.camera_preview);
                previewLayout.AddView(this.cameraPreview);
            }

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.btnPreview);

            button.Click += delegate { this.Finish(); };
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
        /// Creates a file Uri for saving an image or video.
        /// </summary>
        /// <param name="type">The media file type.</param>
        /// <returns>An instance of Uri for saving a media file to.</returns>
        private static Uri GetOutputMediaFileUri(int type)
        {
            return Uri.FromFile(GetOutputMediaFile(type));
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
                Android.OS.Environment.GetExternalStoragePublicDirectory(
                Android.OS.Environment.DirectoryPictures), 
                "MyCameraApp");

            // Create the storage directory if it does not exist.
            if (!mediaStorageDir.Exists())
            {
                if (!mediaStorageDir.Mkdirs())
                {
                    Log.Debug("MyCameraApp", "failed to create directory");
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


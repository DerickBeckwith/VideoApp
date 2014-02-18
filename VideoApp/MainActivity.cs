using System;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace VideoApp
{
    [Activity(Label = "VideoApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private Camera camera;

        private CameraPreview cameraPreview;

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

            this.ReleaseCamera();
        }

        private void ReleaseCamera()
        {
            if (this.camera != null)
            {
                this.camera.Release();
                this.camera = null;
            }
        }
    }
}


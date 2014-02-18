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
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.btnPreview);

            button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };
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
    }
}


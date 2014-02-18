namespace VideoApp
{
    using System;

    using Android.Content;
    using Android.Graphics;
    using Android.Util;
    using Android.Views;

    using Java.IO;

    using Camera = Android.Hardware.Camera;

    public class CameraPreview : SurfaceView, ISurfaceHolderCallback
    {
        private ISurfaceHolder surfaceHolder;

        private readonly Camera camera;

        public CameraPreview(Context context, IAttributeSet attrs, Camera camera) :
            base(context, attrs)
        {
            this.camera = camera;
            this.Initialize();
        }

        public CameraPreview(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            this.Initialize();
        }

        private void Initialize()
        {
            // Add a surface holder callback so we get notified
            // when the underlying surface is created and destroyed.
            this.surfaceHolder = this.Holder;
            this.surfaceHolder.AddCallback(this);

            // Deprecated setting, but required on Android versions prior to 3.0.
            this.surfaceHolder.SetType(SurfaceType.PushBuffers);
        }

        /// <summary>
        /// If your preview can change or rotate, take care of those events here.
        /// Make sure to stop the preview before resizing or reformatting it.
        /// </summary>
        /// <param name="holder"></param>
        /// <param name="format"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
            if (this.surfaceHolder.Surface == null)
            {
                // The preview surface does not exist.
                return;
            }

            // Stop the preview before making changes.
            try
            {
                this.camera.StopPreview();
            }
            catch (Exception)
            {
                throw;
            }

            // set preview size and make any resize, rotate or
            // reformatting changes here.

            // Start the preview with new settings.
            try
            {
                this.camera.SetPreviewDisplay(this.surfaceHolder);
                this.camera.StartPreview();
            }
            catch (Exception exception)
            {
                Log.Debug(
                    this.Tag.ToString(), 
                    string.Format("Error starting camera preview: {0}", exception.Message));
            }
        }

        /// <summary>
        /// The surface has been created, now tell the camera
        /// where to draw the preview.
        /// </summary>
        /// <param name="holder"></param>
        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                this.camera.SetPreviewDisplay(this.surfaceHolder);
                this.camera.StartPreview();
            }
            catch (IOException exception)
            {
                Log.Debug(
                    this.Tag.ToString(), 
                    string.Format("Error setting camera preview: {0}", exception.Message));
            }
        }

        /// <summary>
        /// Take care of releasing the Camera preview in the activity.
        /// </summary>
        /// <param name="holder"></param>
        public void SurfaceDestroyed(ISurfaceHolder holder)
        {

        }
    }
}
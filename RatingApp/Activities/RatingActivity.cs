using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Content;
using Android.Provider;
using Java.IO;
using System;
using SharedProject;
using Android.Support.V7.App;
using RatingApp.Helpers;
using Android.Content.PM;
using System.Collections.Generic;

namespace RatingApp.Activities
{
    [Activity(Label = "RatingActivity")]
    public class RatingActivity : AppCompatActivity
    {
        public static File file;
        public static File dir;
        public static Bitmap bitmap;
        private ImageView imageView;
        private Button pictureButton;
        private TextView resultMessage;
        private TextView resultTextView;
        private bool isCaptureMode = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ratingLayout);
            resultTextView = FindViewById<TextView>(Resource.Id.resultText);
            resultMessage = FindViewById<TextView>(Resource.Id.resultMessage);
            imageView = FindViewById<ImageView>(Resource.Id.imageView1);
            pictureButton = FindViewById<Button>(Resource.Id.GetPictureButton);
            if (IsThereAnAppToTakePictures())
            {
                CreateDirectoryForPictures();
                pictureButton.Click += OnActionClick;
            }
        }

        private void CreateDirectoryForPictures()
        {
            dir = new File(
                   Android.OS.Environment.GetExternalStoragePublicDirectory(
                                Android.OS.Environment.DirectoryPictures), 
                                GetString(Resource.String.imageFolderName)
                           );
            if (!dir.Exists())
            {
                dir.Mkdirs();
            }
        }

        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(
                                     intent, 
                                     PackageInfoFlags.MatchDefaultOnly
                                     );
             return availableActivities != null && 
                    availableActivities.Count > 0;
        }

        private void OnActionClick(object sender, EventArgs eventArgs)
        {
            //Take Camera Button clicked.
            if (isCaptureMode == true)
            {
                CaptureImage();
            }
            else //submit new Image button clicked!
            {
                imageView.SetImageBitmap(null);
                if (bitmap != null)
                {
                    bitmap.Recycle();
                    bitmap.Dispose();
                    bitmap = null;
                }
                CaptureImage();
            }
        }
        
        public void CaptureImage()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            file = new File(dir, String.Format
                                    (
                                       GetString(Resource.String.imageName), 
                                       Guid.NewGuid()
                                    )
                             );
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(file));
            StartActivityForResult(intent, 0);
        }
        
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            try
            {
                //Get the bitmap with the right rotation
                bitmap = BitmapHelpers.GetAndRotateBitmap(file.Path);

                //Resize the picture to be under 4MB 
                //(Emotion API limitation and better for Android memory)
                bitmap = Bitmap.CreateScaledBitmap(bitmap, 2000, 
                                    (int)(2000 * bitmap.Height / bitmap.Width), 
                                    false);

                //Display the image
                imageView.SetImageBitmap(bitmap);

                //Loading message
                resultTextView.Text = GetString(Resource.String.loadText);
                pictureButton.Enabled = false;

                using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                {
                    //Get a stream
                    bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, stream);
                    stream.Seek(0, System.IO.SeekOrigin.Begin);

                    //Get and display the happiness score
                    var result = await Emotion_Core.GetAverageHappinessScore(stream);
                    resultTextView.Text = Emotion_Core.GetHappinessMessage(result);
                    resultMessage.Text = GetString(Resource.String.rateMessage);
                }
            }
            catch (Exception ex)
            {
                resultTextView.Text = ex.Message;
            }
            finally
            {
                pictureButton.Enabled = true;
                pictureButton.Text = GetString(Resource.String.imageSubmit);
                isCaptureMode = false;
            }
        }
    }
}
namespace Zebble
{
    using Android.App;
    using Android.Content;
    using Android.Gms.Common;
    using Android.Gms.Common.Apis;
    using Android.Gms.Drive;
    using Android.OS;
    using Android.Runtime;
    using System.Threading.Tasks;

    public partial class FilePicker
    {
        public interface IGoogleDrivePicker : GoogleApiClient.IConnectionCallbacks, IResultCallback, IDriveApiDriveContentsResult { }

        static GoogleApiClient GoogleApiClient;
        static IGoogleDrivePicker CurrentActivity;
        const int REQUEST_CODE_RESOLUTION = 3;
        const int REQUEST_CODE_PICKER_RESULT = 5498;

        internal Task GoogleDrive()
        {
            if (CurrentActivity == null)
            {
                Device.Log.Error("GoogleDrive not initialized. Please add the Filepicker InitializeGoogleDrive method to the OnCreate method of your activity");
                return Task.CompletedTask;
            }

            if (GoogleApiClient == null)
            {
                GoogleApiClient = new GoogleApiClient.Builder(Renderer.Context)
                  .AddApi(DriveClass.API)
                  .AddScope(DriveClass.ScopeFile)
                  .AddConnectionCallbacks(CurrentActivity)
                  .AddOnConnectionFailedListener(OnConnectionFailed)
                  .Build();
            }
            if (!GoogleApiClient.IsConnected)
                GoogleApiClient.Connect();

            return Task.CompletedTask;
        }

        public static void InitializeGoogleDrive(IGoogleDrivePicker activity)
        {
            CurrentActivity = activity;
            UIRuntime.OnActivityResult.Handle(arg => ConfigActivityResult(arg.Item1, arg.Item2, arg.Item3));
        }

        public static void OnConnected(Bundle connectionHint)
        {
            DriveClass.DriveApi.NewDriveContents(GoogleApiClient).SetResultCallback(CurrentActivity);
        }

        public static void OnResult(Java.Lang.Object result)
        {
            var contentResults = (result).JavaCast<IDriveApiDriveContentsResult>();
            if (!contentResults.Status.IsSuccess) // handle the error
                return;
            Task.Run(() =>
            {
                var picker = DriveClass.DriveApi.NewOpenFileActivityBuilder();
                var intentSender = picker.Build(GoogleApiClient);

                UIRuntime.CurrentActivity.StartIntentSenderForResult(intentSender, REQUEST_CODE_PICKER_RESULT, null, ActivityFlags.ClearTop, ActivityFlags.ClearTop, 0);
            });
        }

        static void ConfigActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == REQUEST_CODE_PICKER_RESULT)
            {
                if (resultCode == Result.Ok)
                {
                    var fileId = data.GetParcelableExtra(OpenFileActivityBuilder.ExtraResponseDriveId);
                    var resourceId = fileId.GetType().GetProperty("ResourceId").GetValue(fileId).ToString();
                    var url = "" + resourceId;
                }
            }
            else if (requestCode == REQUEST_CODE_RESOLUTION)
            {
                switch (resultCode)
                {
                    case Result.Ok:
                        GoogleApiClient.Connect();
                        break;
                    case Result.Canceled:
                        Device.Log.Error("Unable to sign in, is app registered for Drive access in Google Dev Console?");
                        break;
                    case Result.FirstUser:
                        Device.Log.Error("Unable to sign in: RESULT_FIRST_USER");
                        break;
                    default:
                        Device.Log.Error("Should never be here: " + resultCode);
                        return;
                }
            }
        }

        static void OnConnectionFailed(ConnectionResult result)
        {
            Device.Log.Message("GoogleApiClient connection failed: " + result);
            if (!result.HasResolution)
            {
                GoogleApiAvailability.Instance.GetErrorDialog(UIRuntime.CurrentActivity, result.ErrorCode, 0).Show();
                return;
            }
            try
            {
                result.StartResolutionForResult(UIRuntime.CurrentActivity, REQUEST_CODE_RESOLUTION);
            }
            catch (IntentSender.SendIntentException e)
            {
                Device.Log.Error("Exception while starting resolution activity: " + e);
            }
        }
    }
}
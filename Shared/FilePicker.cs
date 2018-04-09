namespace Zebble
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class FilePicker : Stack, FormField.IControl
    {
        public readonly Button Button = new Button { Id = "Button", Text = "Select" };
        public readonly ImageView Preview = new ImageView { Id = "Preview" };

        public MediaSource[] AllowedSources { get; set; }
        public VideoQuality VideoQuality { get; set; } = VideoQuality.High;

        public FilePicker() : base(RepeatDirection.Horizontal)
        {
            AllowedSources = Enum.GetValues(typeof(MediaSource)).Cast<MediaSource>().ToArray();
        }

        public FileInfo File { get; set; }

        object FormField.IControl.Value
        {
            get => File;
            set => File = (FileInfo)value;
        }

        public override async Task OnInitializing()
        {
            await base.OnInitializing();

            await Add(Button);

            await Add(Preview);

            Button.On(x => x.Tapped, HandleTapped);

            Shown.Handle(() => { if (File.Exists()) Preview.Path(File.FullName); });
        }

        public void AllowOnly(params MediaSource[] sources) => AllowedSources = sources;

        async Task HandleTapped()
        {
            if (AllowedSources.None())
                AllowedSources = Enum.GetValues(typeof(MediaSource)).Cast<MediaSource>().ToArray();

            var dialog = new Dialog(this) { Id = "FilePickerDialog" };

            if (AllowedSources.HasMany()) await Nav.ShowPopUp(dialog);
            else
            {
                // Single item: Just open that.
                dialog.GetButton(AllowedSources.Single()).RaiseTapped();
            }
        }

        public async Task PickPhoto()
        {
            if (!Device.Media.SupportsPickingPhoto())
            {
                await CloseWithError("Picking photo is not supported.");
            }
            else
            {
                File = await Device.Media.PickPhoto();
                if (File.Exists()) Preview.Path(File.FullName);
                await Nav.HidePopUp();
            }
        }

        public async Task TakePhoto()
        {
            if (!await Device.Media.IsCameraAvailable())
            {
                await CloseWithError("Camera is not avalible.");
            }
            else if (!Device.Media.SupportsTakingPhoto())
            {
                await CloseWithError("Taking photo is not supported.");
            }
            else
            {
                File = await Device.Media.TakePhoto();
                if (File.Exists()) Preview.Path(File.FullName);
                await Nav.HidePopUp();
            }
        }

        public async Task PickVideo()
        {
            if (!Device.Media.SupportsPickingVideo())
            {
                await CloseWithError("Picking video is not supported.");
            }
            else
            {
                File = await Device.Media.PickVideo();
                await Nav.HidePopUp();
            }
        }

        public async Task TakeVideo()
        {
            if (!await Device.Media.IsCameraAvailable())
            {
                await CloseWithError("Camera is not avalible.");
            }
            else if (!Device.Media.SupportsTakingVideo())
            {
                await CloseWithError("Taking video is not supported.");
            }
            else
            {
                File = await Device.Media.TakeVideo(new Device.MediaCaptureSettings { VideoQuality = VideoQuality });
                await Nav.HidePopUp();
            }
        }

        async Task CloseWithError(string error)
        {
            await Nav.HidePopUp();
            await Alert.Show(error);
        }

        public class Dialog : Zebble.Dialog
        {
            FilePicker Owner;

            public readonly Stack Stack = new Stack();

            public readonly Button CancelButton = new Button { Text = "Cancel" };
            public readonly IconButton PickPhotoButton = new IconButton { Id = "PickPhotoButton", Text = "Pick Photo" };
            public readonly IconButton TakePhotoButton = new IconButton { Id = "TakePhotoButton", Text = "Take Photo" };
            public readonly IconButton PickVideoButton = new IconButton { Id = "PickVideoButton", Text = "Pick Video" };
            public readonly IconButton TakeVideoButton = new IconButton { Id = "TakeVideoButton", Text = "Take Video" };

            public readonly IconButton PickFromGoogleDrive = new IconButton { Id = "PickFromGoogleDrive", Text = "Pick From Drive" };
            public readonly IconButton PickFromICloud = new IconButton { Id = "PickFromICloud", Text = "Pick From iCloud" };

            public Dialog(FilePicker mediaButton)
            {
                Title.Text = "Please select";
                Owner = mediaButton;

                GetPickerButtons().Do(x => x.Css.BackgroundImageAlignment = Alignment.Left);

                PickPhotoButton.On(x => x.Tapped, Owner.PickPhoto);
                TakePhotoButton.On(x => x.Tapped, Owner.TakePhoto);
                PickVideoButton.On(x => x.Tapped, Owner.PickVideo);
                TakeVideoButton.On(x => x.Tapped, Owner.TakeVideo);
#if ANDROID
                PickFromGoogleDrive.On(x => x.Tapped, Owner.GoogleDrive);
#elif IOS
                PickFromICloud.On(x => x.Tapped, Owner.ICloud);
#endif
            }

            public override async Task OnInitializing()
            {
                await base.OnInitializing();

                await Content.Add(Stack);

                await Stack.AddRange(Owner.AllowedSources.Select(GetButton));

                await ButtonsRow.Add(CancelButton.On(x => x.Tapped, () => Nav.HidePopUp()));
            }

            public IconButton GetButton(MediaSource source)
            {
                switch (source)
                {
                    case MediaSource.PickPhoto: return PickPhotoButton;
                    case MediaSource.PickVideo: return PickVideoButton;
                    case MediaSource.TakePhoto: return TakePhotoButton;
                    case MediaSource.TakeVideo: return TakeVideoButton;
#if ANDROID
                    case MediaSource.GoogleDrive: return PickFromGoogleDrive;
#elif IOS
                    case MediaSource.ICloud: return PickFromICloud;
#endif
                    default: throw new NotSupportedException(source + " is not supported for button selection!");
                }
            }

            public IEnumerable<IconButton> GetPickerButtons()
            {
                return new[] { PickPhotoButton, PickVideoButton, TakePhotoButton, TakeVideoButton };
            }
        }
    }
}
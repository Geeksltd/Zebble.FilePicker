namespace Zebble
{
    using CoreGraphics;
    using MobileCoreServices;
    using System.Threading.Tasks;
    using UIKit;

    public partial class FilePicker
    {
        static CGRect Frame;
        static UIApplicationDelegate AppDelegate;

        internal async Task ICloud()
        {
            var allowedUTIs = new string[] { UTType.UTF8PlainText, UTType.PlainText, UTType.RTF, UTType.PNG, UTType.Text, UTType.PDF, UTType.Image };

            var viewController = (UIViewController)UIRuntime.NativeRootScreen;
            var pickerMenu = new UIDocumentMenuViewController(allowedUTIs, UIDocumentPickerMode.Open);
            pickerMenu.DidPickDocumentPicker += (sender, args) =>
            {
                args.DocumentPicker.DidPickDocument += (sndr, pArgs) =>
                {
                    var securityEnabled = pArgs.Url.StartAccessingSecurityScopedResource();
                    var document = new UIDocument(pArgs.Url);
                    document.Open(success =>
                    {
                        if (success) Device.Log.Message("Selected file opend successfully");
                        else Device.Log.Error("The selected file could not open");
                    });
                    pArgs.Url.StopAccessingSecurityScopedResource();
                };

                viewController.PresentViewController(args.DocumentPicker, animated: true, completionHandler: null);
            };

            pickerMenu.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            viewController.PresentViewController(pickerMenu, animated: true, completionHandler: null);

            var presentationPopover = pickerMenu.PopoverPresentationController;
            if (presentationPopover == null) return;

            presentationPopover.SourceView = viewController.View;
            presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Down;
            presentationPopover.SourceRect = Frame;

            await Task.CompletedTask;
        }

        public static void InitializeiCloud(UIApplicationDelegate appDelegate, CGRect popoverFrame)
        {
            AppDelegate = appDelegate;
            Frame = popoverFrame;
        }
    }
}
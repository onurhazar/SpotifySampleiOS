// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SpotifySampleiOS
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		UIKit.UIImageView albumImageView { get; set; }

		[Outlet]
		UIKit.UIButton connectButton { get; set; }

		[Outlet]
		UIKit.UIButton disconnectButton { get; set; }

		[Outlet]
		UIKit.UIButton playButton { get; set; }

		[Outlet]
		UIKit.UILabel songTitleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (albumImageView != null) {
				albumImageView.Dispose ();
				albumImageView = null;
			}

			if (connectButton != null) {
				connectButton.Dispose ();
				connectButton = null;
			}

			if (disconnectButton != null) {
				disconnectButton.Dispose ();
				disconnectButton = null;
			}

			if (playButton != null) {
				playButton.Dispose ();
				playButton = null;
			}

			if (songTitleLabel != null) {
				songTitleLabel.Dispose ();
				songTitleLabel = null;
			}
		}
	}
}

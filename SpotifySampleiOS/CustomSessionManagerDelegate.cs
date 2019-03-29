using System;
using System.Diagnostics;
using Foundation;
using SpotifyBindingiOS;

namespace SpotifySampleiOS
{
    public class CustomSessionManagerDelegate: NSObject, ISPTSessionManagerDelegate
    {
        [Export("sessionManager:didFailWithError:")]
        public void DidFailWithError(SPTSessionManager manager, NSError error)
        {
            Debug.WriteLine("Authorization Failed. Error:" + error.LocalizedDescription);
        }

        [Export("sessionManager:didInitiateSession:")]
        public void DidInitiateSession(SPTSessionManager manager, SPTSession session)
        {
            Debug.WriteLine("Authorization Success. " + session.Description);
        }

        [Export("sessionManager:didRenewSession:")]
        public void DidRenewSession(SPTSessionManager manager, SPTSession session)
        {
            Debug.WriteLine("Session Renewed. " + session.Description);
        }
    }
}

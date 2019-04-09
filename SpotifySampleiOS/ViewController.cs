using System;
using UIKit;
using SpotifyBindingiOS;
using Foundation;
using System.Diagnostics;
using ObjCRuntime;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;

namespace SpotifySampleiOS
{
    public partial class ViewController : UIViewController, ISPTSessionManagerDelegate, ISPTAppRemoteDelegate, ISPTAppRemotePlayerStateDelegate
    {
        string spotifyClientId = "74a6c04e47494a06b9144b4d80df47a0";
        NSUrl spotifyRedirectURI = new NSUrl("spotify-start://spotify-login-callback");

        SPTConfiguration _configuration;

        public SPTConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = new SPTConfiguration(spotifyClientId, spotifyRedirectURI)
                    {
                        PlayURI = "",
                        TokenSwapURL = new NSUrl("https://sweltering-risk.glitch.me/api/token"),//new NSUrl("http://192.168.21.177:17022/swap"),
                        TokenRefreshURL = new NSUrl("https://sweltering-risk.glitch.me/api/refresh_token")//new NSUrl("http://192.168.21.177:17022/refresh")
                    };
                }
                return _configuration;
            }
        }

        SPTSessionManager _sessionManager;

        public SPTSessionManager SessionManager
        {
            get
            {
                if (_sessionManager == null)
                {
                    _sessionManager = new SPTSessionManager(Configuration, this);
                }

                Session.SessionManager = _sessionManager;
                return _sessionManager;
            }
        }

        SPTAppRemote _appRemote;

        public SPTAppRemote AppRemote
        {
            get
            {
                if (_appRemote == null)
                {
                    _appRemote = new SPTAppRemote(Configuration, SPTAppRemoteLogLevel.Debug);
                    _appRemote.Delegate = this;
                }
                Session.AppRemote = _appRemote;
                return _appRemote;
            }
        }

        ISPTAppRemotePlayerState lastPlayerState;

        protected ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            connectButton.TouchUpInside += HandleConnectButton;
            disconnectButton.TouchUpInside += HandleDisconnectButton;
            playButton.TouchUpInside += HandlePlayButton;

            changeVolumeButton.TouchUpInside += HandleChangeVolumeButton;

            UpdateViewBasedOnConnected();
        }

        //Actions

        private void HandleChangeVolumeButton(object sender, EventArgs e)
        {
            try
            {
                var res = int.TryParse(volumeTextField.Text, out int newVol);
                if(res)
                    ChangeVolume(newVol);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to change volume. Error: " + ex.Message);
            }
        }

        void ChangeVolume(int volume)
        {
            var client = new RestClient("https://api.spotify.com/v1/me/player/volume");

            var request = new RestRequest(Method.PUT);
            request.AddQueryParameter("volume_percent", "1");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");

            var bearerToken = "BQCbNcxTnTCrs-h7snfVE4Xl0NHNWc9dPlkXmvQPy2x9ReUvqDYj1wqCmOMIWiYYv-U2UF2wGfOuAFb5hQ_luBslnK8pWlqeOmxmJn4UtmQxdW9mlm8Lz4H-o9GhfdvZjI5ovgPMKbCNrni9IWjr5agvS0OKhwLY_2L2M4RRe2ePn73vCgWXQR1BTAuxWWs0dhFRjgKCaiToVVmixbrbfMtTzi5MOHzgKhS25QHOf6x0PHMtiEOLHeJZUDaca7bxBm2plS4zMm8WXX38rA";
            request.AddHeader("Authorization", string.Format("Bearer {0}", bearerToken));

            //var client = new RestClient("https://jsonplaceholder.typicode.com/todos/1");
            //var request = new RestRequest(Method.GET);
            //request.AddHeader("Accept", "application/json");
            //request.AddHeader("Content-Type", "application/json");

            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = response.Content;
            }
        }

        private void HandlePlayButton(object sender, EventArgs e)
        {
            if (lastPlayerState != null && lastPlayerState.Paused)
            {
                AppRemote.PlayerAPI.Resume(null);
            }
            else
            {
                AppRemote.PlayerAPI.Pause(null);
            }
        }

        private void HandleConnectButton(object sender, EventArgs e)
        {

            //Scopes let you specify exactly what types of data your application wants to
            //access, and the set of scopes you pass in your call determines what access
            //permissions the user is asked to grant.
            //For more information, see https://developer.spotify.com/web-api/using-scopes/.
            var scope = SPTScope.AppRemoteControlScope | SPTScope.PlaylistReadPrivateScope; //| SPTScope.StreamingScope | SPTScope.UserReadCurrentlyPlayingScope | SPTScope.UserFollowReadScope;

            SessionManager.InitiateSessionWithScope(scope, SPTAuthorizationOptions.ClientAuthorizationOption);
        }

        private void HandleDisconnectButton(object sender, EventArgs e)
        {
            try
            {
                if (AppRemote.Connected)
                {
                    AppRemote.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to disconnect app remote. Error: " + ex.Message);
            }
        }

        private void UpdateViewBasedOnConnected()
        {
            if (AppRemote.Connected)
            {
                connectButton.Hidden = true;
                disconnectButton.Hidden = false;
                albumImageView.Hidden = false;
                songTitleLabel.Hidden = false;
                playButton.Hidden = false;
            }
            else
            {
                connectButton.Hidden = false;
                disconnectButton.Hidden = true;
                albumImageView.Hidden = true;
                songTitleLabel.Hidden = true;
                playButton.Hidden = true;
            }
        }

        private void UpdatePlayerState(ISPTAppRemotePlayerState playerState)
        {
            if (lastPlayerState == null || lastPlayerState.Track.URI != playerState.Track.URI)
            {
                FetchArtwork(playerState.Track);
            }

            lastPlayerState = playerState;
            songTitleLabel.Text = playerState.Track.Name;

            if (playerState.Paused)
            {
                playButton.SetTitle("Play", UIControlState.Normal);
            }
            else
            {
                playButton.SetTitle("Pause", UIControlState.Normal);
            }
        }

        private void FetchArtwork(ISPTAppRemoteTrack track)
        {
            try
            {
                AppRemote.ImageAPI.FetchImageForItem(track, CoreGraphics.CGSize.Empty, HandleFetchArtworkCallback);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to fetch image for item. Error: " + ex.Message);
            }
        }

        void HandleFetchArtworkCallback(NSObject image, NSError error)
        {
            if (error != null)
            {
                Debug.WriteLine("Error fetching track image: " + error.LocalizedDescription);
            }
            else if (image != null)
            {
                var img = image as UIImage;
                if (img != null)
                {
                    albumImageView.Image = img;
                }
            }
        }

        private void FetchPlayerState()
        {
            try
            {
                AppRemote.PlayerAPI.GetPlayerState(HandleGetPlayerStateCallback);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to get player state. Error: " + ex.Message);
            }
        }

        void HandleGetPlayerStateCallback(ISPTAppRemotePlayerState playerState, NSError error)
        {
            if (error != null)
            {
                Debug.WriteLine("Error getting player state: " + error.LocalizedDescription);
            }
            else if (playerState != null)
            {
                if (playerState != null)
                {
                    UpdatePlayerState(playerState);
                }
            }
        }

        #region ISPTSessionManagerDelegate Methods

        //[Export("sessionManager:didInitiateSession:")]
        public void DidInitiateSession(SPTSessionManager manager, SPTSession session)
        {
            Debug.WriteLine("Authorization Success. " + session.Description);
            AppRemote.ConnectionParameters.AccessToken = session.AccessToken;
            AppRemote.Connect();
        }

        //[Export("sessionManager:didFailWithError:")]
        public void DidFailWithError(SPTSessionManager manager, NSError error)
        {
            Debug.WriteLine("Authorization Failed. Error:" + error.LocalizedDescription);
        }

        [Export("sessionManager:didRenewSession:")] //needed for optional delegate method
        public void DidRenewSession(SPTSessionManager manager, SPTSession session)
        {
            Debug.WriteLine("Session Renewed. " + session.Description);
        }

        #endregion

        #region ISPTAppRemoteDelegate Methods

        [Export("appRemoteDidEstablishConnection:")]
        public void DidEstablishConnection(SPTAppRemote appRemote)
        {
            Debug.WriteLine("Connected");
            UpdateViewBasedOnConnected();
            var playerAPI = appRemote.PlayerAPI;
            //var playerapi = Runtime.GetNSObject<SPTAppRemotePlayerAPI>(playerAPI.Handle);
            playerAPI.SetWeakDelegate(this);
            playerAPI.SubscribeToPlayerState(HandleSubscribeToPlayerStateCallback);
            FetchPlayerState();
        }

        void HandleSubscribeToPlayerStateCallback(NSObject success, NSError error)
        {
            if (error != null)
            {
                Debug.WriteLine("Error subscribing to player state: " + error.LocalizedDescription);
            }
        }

        [Export("appRemote:didFailConnectionAttemptWithError:")]
        public void DidFailConnectionAttemptWithError(SPTAppRemote appRemote, NSError error)
        {
            Debug.WriteLine("Failed");
            UpdateViewBasedOnConnected();
            lastPlayerState = null;
        }

        [Export("appRemote:didDisconnectWithError:")]
        public void DidDisconnectWithError(SPTAppRemote appRemote, NSError error)
        {
            Debug.WriteLine("Disconnected");
            UpdateViewBasedOnConnected();
            lastPlayerState = null;
        }

        #endregion

        #region ISPTAppRemotePlayerAPIDelegate Methods

        public void PlayerStateDidChange(ISPTAppRemotePlayerState playerState)
        {
            Debug.WriteLine("Player state changed");
            Debug.WriteLine("Track name: " + playerState.Track.Name);

            UpdatePlayerState(playerState);
        }

        #endregion
    }
}

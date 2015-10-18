using Lamna.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Lamna
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool locked = true;

        private bool m_addingAccount = true;
        private Account user;
        private bool m_passportAvailable = true;



        public MainPage()
        {
            this.InitializeComponent();
            SizeChanged += MainPage_SizeChanged;

            if (ApiInformation.IsTypePresent(typeof(StatusBar).FullName))
            {
                StatusBar.GetForCurrentView().HideAsync();
            }

            RootFrame.Navigated += RootFrame_Navigated;
            SystemNavigationManager.GetForCurrentView().BackRequested += BackButtonBackRequested; ;

            ((App)App.Current).MainFrame = RootFrame;

            RootFrame.Navigate(typeof(Views.HomeView));
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            user = DataSource.GetInstance().GetUser();
            var passportSetup = await KeyCredentialManager.IsSupportedAsync();
            if (user != null)
            {
                textbox_Username.Text = user.Email;
                textbox_Username.IsEnabled = false;
                m_addingAccount = false;

                // check if Hello is available on this machine
                // if not, needs to enable it in Windows Settings
                if (passportSetup && user.UsesPassport)
                {
                    SignInPassport(true);
                }
                
            }

            if (!passportSetup)
            {
                button_PassportSignIn.Visibility = Visibility.Collapsed;
            }
            
        }

        private void BackButtonBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (RootFrame.CanGoBack)
            {
                RootFrame.GoBack();
                e.Handled = true;
            }
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = 
                RootFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (locked)
            {
                //ContentTransform.TranslateY = e.NewSize.Height;
                LockScreenTransform.TranslateY = 0;
            }
            else
            {
               // ContentTransform.TranslateY = 20;
                LockScreenTransform.TranslateY = -e.NewSize.Height + 20;
            }

            Content.Height = e.NewSize.Height - 20;
            ContentTransform.CenterX = e.NewSize.Width / 2;
            ContentTransform.CenterY = e.NewSize.Height / 2;
            
        }

        private void Unlock()
        {
            locked = false;

            LoginAnimation.Stop();

            //ContentAnimation.To = 20;
            //ContentAnimation.From = Window.Current.Bounds.Height;

            ContentZoomXAnimation.To = ContentZoomYAnimation.To = 1;
            ContentZoomXAnimation.From = ContentZoomYAnimation.From = 0.8;

            ContentShadeAnimation.To = 0;
            ContentShadeAnimation.From = 1;

            LockScreenAnimation.To = -Window.Current.Bounds.Height + 20;
            LockScreenAnimation.From = 0;

            CityAnimation.To = 60;
            CityAnimation.From = 0;
            CityAnimation.Duration = TimeSpan.FromMilliseconds(2000);

            SkyAnimation.To = 1;
            SkyAnimation.From = 0;

            LockScreenAnimation.Duration =
            //ContentAnimation.Duration =
            SkyAnimation.Duration =
            ContentZoomXAnimation.Duration =
            ContentZoomYAnimation.Duration =
            ContentZoomYAnimation.Duration = TimeSpan.FromMilliseconds(500);

            LoginAnimation.Begin();
        }

        private void Lock()
        {
            if (!locked)
            {
                locked = true;

                LoginAnimation.Stop();

                //ContentAnimation.To = Window.Current.Bounds.Height;
                //ContentAnimation.From = 20;

                ContentZoomXAnimation.To = ContentZoomYAnimation.To = 0.8;
                ContentZoomXAnimation.From = ContentZoomYAnimation.From = 1;

                ContentShadeAnimation.To = 1;
                ContentShadeAnimation.From = 0;

                LockScreenAnimation.To = 0;
                LockScreenAnimation.From = -Window.Current.Bounds.Height + 20;

                CityAnimation.To = 0;
                CityAnimation.From = 60;
                CityAnimation.Duration = TimeSpan.FromMilliseconds(2000);

                SkyAnimation.To = 0;
                SkyAnimation.From = 1;

                LockScreenAnimation.Duration =
                //ContentAnimation.Duration =
                SkyAnimation.Duration =
                ContentZoomXAnimation.Duration =
                ContentZoomYAnimation.Duration =
                ContentZoomYAnimation.Duration = TimeSpan.FromMilliseconds(500);

                LoginAnimation.Begin();
            }
        }
        
        private void LockScreen_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Lock();
        }

        #region Windows Hello

        private void Button_SignIn_Click(object sender, RoutedEventArgs e)
        {
            SignInPassword(false);
        }

        private void Button_Passport_Click(object sender, RoutedEventArgs e)
        {
            if (!m_addingAccount)
            {
                SignInPassport(user.UsesPassport);
            }
            else
            {
                SignInPassport(false);
            }
        }
        
        private async void SignInPassport(bool passportIsPrimaryLogin)
        {
            if (passportIsPrimaryLogin)
            {
                if (await AuthenticatePassport())
                {
                    SuccessfulSignIn(user);
                    return;
                }
            }
            else if (await SignInPassword(true))
            {
                if (await CreatePassportKey(textbox_Username.Text) == true)
                {
                    bool serverAddedPassportToAccount = await AddPassportToAccountOnServer();

                    if (serverAddedPassportToAccount == true)
                    {
                        if (m_addingAccount == true)
                        {
                            Account goodAccount = new Account() { Name = textbox_Username.Text, Email = textbox_Username.Text, UsesPassport = true };
                            SuccessfulSignIn(goodAccount);
                        }
                        else
                        {
                            user.UsesPassport = true;
                            SuccessfulSignIn(user);
                        }
                        return;
                    }
                }
                textblock_ErrorField.Text = "Sign In with Passport failed. Try later.";
                button_PassportSignIn.IsEnabled = false;
            }
            textblock_ErrorField.Text = "Invalid username or password.";
        }

        private async Task<bool> SignInPassword(bool calledFromPassport)
        {
            textblock_ErrorField.Text = "";

            if (textbox_Username.Text.Length == 0 || passwordbox_Password.Password.Length == 0)
            {
                textblock_ErrorField.Text = "Username/Password cannot be blank.";
                return false;
            }

            try
            {
                bool signedIn = await AuthenticatePasswordCredentials();

                if (!signedIn)
                {
                    textblock_ErrorField.Text = "Unable to sign you in with those credentials.";
                }
                else
                {
                    // TODO: Roaming Passport settings. Make it so the server can tell us if they prefer to use Passport and upsell immediately.

                    Account goodAccount = new Account() { Name = textbox_Username.Text, Email = textbox_Username.Text, UsesPassport = false };
                    if (calledFromPassport == false)
                    {
                        SuccessfulSignIn(goodAccount);
                    }

                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private async Task<bool> AuthenticatePasswordCredentials()
        {
            // TODO: Authenticate with server once that part is done for the sample.

            return true;
        }

        private async Task<bool> AuthenticatePassport()
        {
            IBuffer message = CryptographicBuffer.ConvertStringToBinary("LoginAuth", BinaryStringEncoding.Utf8);
            IBuffer authMessage = await GetPassportAuthenticationMessage(message, user.Email);

            if (authMessage != null)
            {
                return true;
            }

            return false;
        }

        private void SuccessfulSignIn(Account account)
        {
            DataSource.GetInstance().SaveUser(account);
            textbox_Username.IsEnabled = false;
            passwordbox_Password.Password = "";
            textblock_ErrorField.Text = "";
            Unlock();


        }

        private async Task<bool> AddPassportToAccountOnServer()
        {
            // TODO: Add Passport signing info to server when that part is done for the sample

            return true;
        }

        private async Task<bool> CreatePassportKey(string accountId)
        {
            KeyCredentialRetrievalResult keyCreationResult = await KeyCredentialManager.RequestCreateAsync(accountId, KeyCredentialCreationOption.ReplaceExisting);

            if (keyCreationResult.Status == KeyCredentialStatus.Success)
            {

                KeyCredential userKey = keyCreationResult.Credential;
                IBuffer publicKey = userKey.RetrievePublicKey();
                KeyCredentialAttestationResult keyAttestationResult = await userKey.GetAttestationAsync();

                IBuffer keyAttestation = null;
                IBuffer certificateChain = null;
                bool keyAttestationIncluded = false;
                bool keyAttestationCanBeRetrievedLater = false;
                KeyCredentialAttestationStatus keyAttestationRetryType = 0;

                if (keyAttestationResult.Status == KeyCredentialAttestationStatus.Success)
                {
                    keyAttestationIncluded = true;
                    keyAttestation = keyAttestationResult.AttestationBuffer;
                    certificateChain = keyAttestationResult.CertificateChainBuffer;
                }
                else if (keyAttestationResult.Status == KeyCredentialAttestationStatus.TemporaryFailure)
                {
                    keyAttestationRetryType = KeyCredentialAttestationStatus.TemporaryFailure;
                    keyAttestationCanBeRetrievedLater = true;
                }
                else if (keyAttestationResult.Status == KeyCredentialAttestationStatus.NotSupported)
                {
                    keyAttestationRetryType = KeyCredentialAttestationStatus.NotSupported;
                    keyAttestationCanBeRetrievedLater = false;
                }

                // Package public key, keyAttesation if available, 
                // certificate chain for attestation endorsement key if available,  
                // status code of key attestation result: keyAttestationIncluded or 
                // keyAttestationCanBeRetrievedLater and keyAttestationRetryType
                // and send it to application server to register the user.
                bool serverAddedPassportToAccount = await AddPassportToAccountOnServer();

                if (serverAddedPassportToAccount == true)
                {
                    return true;
                }
            }
            else if (keyCreationResult.Status == KeyCredentialStatus.UserCanceled)
            {
                // User cancelled the Passport enrollment process
            }
            else if (keyCreationResult.Status == KeyCredentialStatus.NotFound)
            {
                // User needs to create PIN
                //textblock_PassportStatusText.Text = "Microsoft Passport is almost ready!\nPlease go to Windows Settings and set up a PIN to use it.";
                //grid_PassportStatus.Background = new SolidColorBrush(Color.FromArgb(255, 50, 170, 207));
                //button_PassportSignIn.IsEnabled = false;

                m_passportAvailable = false;
            }
            else
            {
            }

            return false;
        }

        private async Task<IBuffer> GetPassportAuthenticationMessage(IBuffer message, string accountId)
        {
            KeyCredentialRetrievalResult openKeyResult = await KeyCredentialManager.OpenAsync(accountId);

            if (openKeyResult.Status == KeyCredentialStatus.Success)
            {
                KeyCredential userKey = openKeyResult.Credential;
                IBuffer publicKey = userKey.RetrievePublicKey();

                KeyCredentialOperationResult signResult = await userKey.RequestSignAsync(message);

                if (signResult.Status == KeyCredentialStatus.Success)
                {
                    return signResult.Result;
                }
                else if (signResult.Status == KeyCredentialStatus.UserCanceled)
                {
                    // User cancelled the Passport PIN entry.
                    //
                    // We will return null below this and the username/password
                    // sign in form will show.
                }
                else if (signResult.Status == KeyCredentialStatus.NotFound)
                {
                    // Must recreate Passport key
                }
                else if (signResult.Status == KeyCredentialStatus.SecurityDeviceLocked)
                {
                    // Can't use Passport right now, remember that hardware failed and suggest restart
                }
                else if (signResult.Status == KeyCredentialStatus.UnknownError)
                {
                    // Can't use Passport right now, try again later
                }

                return null;
            }
            else if (openKeyResult.Status == KeyCredentialStatus.NotFound)
            {
                // Passport key lost, need to recreate it
                //textblock_PassportStatusText.Text = "Microsoft Passport is almost ready!\nPlease go to Windows Settings and set up a PIN to use it.";
                //grid_PassportStatus.Background = new SolidColorBrush(Color.FromArgb(255, 50, 170, 207));
                //button_PassportSignIn.IsEnabled = false;

                m_passportAvailable = false;
            }
            else
            {
                // Can't use Passport right now, try again later
            }
            return null;
        }

        #endregion
    }
}

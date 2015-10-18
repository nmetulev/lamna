using Lamna.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;

namespace LamnaCortanaService
{
    public sealed class Service : XamlRenderingBackgroundTask
    {
        private BackgroundTaskDeferral serviceDeferral;
        VoiceCommandServiceConnection voiceServiceConnection;

        protected async override void OnRun(IBackgroundTaskInstance taskInstance)
        {
            this.serviceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            VoiceCommandUserMessage userMessage;
            VoiceCommandResponse response;
            try
            {
                voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
                voiceServiceConnection.VoiceCommandCompleted += VoiceCommandCompleted;
                VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

                switch (voiceCommand.CommandName)
                {
                    case "next":

                        var appointment = (await DataSource.GetInstance().GetAppointmentsAsync()).FirstOrDefault();
                        if (appointment != null)
                        {
                            var tiles = new List<VoiceCommandContentTile>();
                            var tile = new VoiceCommandContentTile();

                            tile.ContentTileType = VoiceCommandContentTileType.TitleWith280x140IconAndText;

                            tiles.Add(tile);
                            tile.Image = await MapService.GetStaticRoute(280, 140, DataSource.GetCurrentLocation(), appointment.Location);

                            userMessage = new VoiceCommandUserMessage()
                            {
                                SpokenMessage = "Your next appointment is the " + appointment.FamilyName + " family. It looks like you are 20 minutes away, should I let them know?",
                                DisplayMessage = "Your next appointment is the " + appointment.FamilyName + " family."
                            };

                            var repromptMessage = new VoiceCommandUserMessage()
                            {
                                DisplayMessage = "Should I let the " + appointment.FamilyName + " family know of your arrival?",
                                SpokenMessage = "Should I let the " + appointment.FamilyName + " family know of your arrival?"
                            };

                            response = VoiceCommandResponse.CreateResponseForPrompt(userMessage, repromptMessage, tiles);
                            var userResponse = await voiceServiceConnection.RequestConfirmationAsync(response);
                            string secondResponse;
                            if (userResponse.Confirmed)
                            {
                                secondResponse = "Great, I'll take care of it!";
                            }
                            else
                            {
                                secondResponse = "OK, no problem!";

                            }

                            //userMessage = new VoiceCommandUserMessage()
                            //{
                            //    DisplayMessage = secondResponse + "Should I start navigation?",
                            //    SpokenMessage = secondResponse + "Should I start navigation?",
                            //};

                            //repromptMessage = new VoiceCommandUserMessage()
                            //{
                            //    DisplayMessage = "Should I start navigation?",
                            //    SpokenMessage = "Should I start navigation?",
                            //};

                            //response = VoiceCommandResponse.CreateResponseForPrompt(userMessage, repromptMessage, tiles);
                            //var userResponse = await voiceServiceConnection.RequestConfirmationAsync(response);

                            ////await Task.Delay(1000);


                            userMessage = new VoiceCommandUserMessage()
                            {
                                DisplayMessage = secondResponse + " Safe driving!",
                                SpokenMessage = secondResponse + " Safe driving!",
                            };

                            response = VoiceCommandResponse.CreateResponse(userMessage, tiles);
                            await voiceServiceConnection.ReportSuccessAsync(response);
                                                       
                        }
                        else
                        {
                            userMessage = new VoiceCommandUserMessage()
                            {
                                DisplayMessage = "All done for today!",
                                SpokenMessage = "You don't have any appointments for the rest of the day!"
                            };

                            response = VoiceCommandResponse.CreateResponse(userMessage);
                            await voiceServiceConnection.ReportSuccessAsync(response);
                        }

                        

                        break;
                    case "what":

                        var appointments = (await DataSource.GetInstance().GetAppointmentsAsync()).Take(3);
                        if (appointments != null & appointments.Count() > 0)
                        {
                            var tiles = new List<VoiceCommandContentTile>();

                            foreach (var app in appointments)
                            {
                                var tile = new VoiceCommandContentTile();

                                tile.ContentTileType = VoiceCommandContentTileType.TitleWith68x68IconAndText;

                                tile.TextLine1 = app.FamilyName;
                                tile.TextLine2 = app.Address;
                                tile.Title = "2:00 PM";
                                tile.Image = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/house.png"));
                                tiles.Add(tile);
                            }
                            
                            userMessage = new VoiceCommandUserMessage()
                            {
                                SpokenMessage = "Here are your next " + appointments.Count() + " appointments",
                                DisplayMessage = "Here are your next " + appointments.Count() + " appointments",
                            };

                            response = VoiceCommandResponse.CreateResponse(userMessage, tiles);
                            await voiceServiceConnection.ReportSuccessAsync(response);

                        }
                        else
                        {
                            userMessage = new VoiceCommandUserMessage()
                            {
                                DisplayMessage = "All done for today!",
                                SpokenMessage = "You don't have any appointments for the rest of the day!"
                            };

                            response = VoiceCommandResponse.CreateResponse(userMessage);
                            await voiceServiceConnection.ReportSuccessAsync(response);
                        }



                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                if (this.serviceDeferral != null)
                {
                    //Complete the service deferral
                    this.serviceDeferral.Complete();
                }
            }
        }

        private void VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }
    }
}

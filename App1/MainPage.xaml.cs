using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Media.SpeechSynthesis;
using Windows.Media.SpeechRecognition;
using Microsoft.Bot.Connector.DirectLine;
using System.Text.RegularExpressions;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace App1
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内移動できる空白ページ。
    /// </summary>に
    public sealed partial class MainPage : Page
    {
        private static string directLineSecret = "QJaBeEbHBZM.cwA.yM8.OXoBIbbcLunjh0iV4GdJb20PjaewlmRpReZt7awRQOo";
        private static string fromUser = "DirectLineSampleClientUser";
        DirectLineClient client;
        Conversation conversation;

        public MainPage()
        {
            this.InitializeComponent();

            client = new DirectLineClient(directLineSecret);
            conversation = client.Conversations.StartConversation();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonSubmit.Content = "音声認識中...";
                var speechRecognizer = new SpeechRecognizer();
                await speechRecognizer.CompileConstraintsAsync();
                SpeechRecognitionResult result = await speechRecognizer.RecognizeAsync();
                buttonSubmit.Content = "話しかけ";

                if (result.Status == SpeechRecognitionResultStatus.Success)
                {
                    // 音声認識の結果が得られた時の処理
                    SendMessageToBot(result.Text);
                }
            }
            catch (Exception err)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(err.Message, err.HResult.ToString());
                await messageDialog.ShowAsync();
            }
        }

        private async void SendMessageToBot(string message)
        {
            if (message.Length > 0)
            {
                listbox.Items.Add("自分 ： " + message);
                listbox.UpdateLayout();
                listbox.ScrollIntoView(listbox.Items.Last());

                Activity userMessage = new Activity
                {
                    From = new ChannelAccount(fromUser),
                    Text = message,
                    Type = ActivityTypes.Message
                };

                await client.Conversations.PostActivityAsync(conversation.ConversationId, userMessage);

                try
                {
                    ActivitySet activitySet = await client.Conversations.GetActivitiesAsync(conversation.ConversationId, null);
                    Activity activitie = activitySet.Activities.Last();

                    string response = activitie.Text;
                    listbox.Items.Add("Cortana： " + response);
                    listbox.UpdateLayout();
                    listbox.ScrollIntoView(listbox.Items.Last());

                    if (response.Length > 0)
                            TextToSpeech(response);
                }
                catch (Exception err)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(err.Message, err.HResult.ToString());
                    await messageDialog.ShowAsync();
                }


            }
        }

        private async void TextToSpeech(string response)
        {
            MediaElement mediaElement = new MediaElement();
            var synth = new SpeechSynthesizer();
            SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(response);
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }

    }
}

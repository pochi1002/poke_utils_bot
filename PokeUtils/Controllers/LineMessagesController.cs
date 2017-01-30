using LineMessagingAPISDK;
using LineMessagingAPISDK.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace PokeUtils.Controllers
{
    public class LineMessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post(HttpRequestMessage request)
        {
            if (!await VaridateSignature(request))
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            Activity activity = JsonConvert.DeserializeObject<Activity>
                (await request.Content.ReadAsStringAsync());

            // Line may send multiple events in one message, so need to handle them all.
            foreach (Event lineEvent in activity.Events)
            {
                LineMessageHandler handler = new LineMessageHandler(lineEvent);

                Profile profile = await handler.GetProfile(lineEvent.Source.UserId);
                //if(profile == null)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK);
                //}
                switch (lineEvent.Type)
                {
                    case EventType.Beacon:
                        await handler.HandleBeaconEvent();
                        break;
                    case EventType.Follow:
                        await handler.HandleFollowEvent();
                        break;
                    case EventType.Join:
                        await handler.HandleJoinEvent();
                        break;
                    case EventType.Leave:
                        await handler.HandleLeaveEvent();
                        break;
                    case EventType.Message:
                        Message message = JsonConvert.DeserializeObject<Message>(lineEvent.Message.ToString());
                        switch (message.Type)
                        {
                            case MessageType.Text:
                                await handler.HandleTextMessage();
                                break;
                            case MessageType.Audio:
                            case MessageType.Image:
                            case MessageType.Video:
                                await handler.HandleMediaMessage();
                                break;
                            case MessageType.Sticker:
                                await handler.HandleStickerMessage();
                                break;
                            case MessageType.Location:
                                await handler.HandleLocationMessage();
                                break;
                        }
                        break;
                    case EventType.Postback:
                        await handler.HandlePostbackEvent();
                        break;
                    case EventType.Unfollow:
                        await handler.HandleUnfollowEvent();
                        break;
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private async Task<bool> VaridateSignature(HttpRequestMessage request)
        {
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["ChannelSecret"].ToString()));
            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(await request.Content.ReadAsStringAsync()));
            var contentHash = Convert.ToBase64String(computeHash);
            var headerHash = Request.Headers.GetValues("X-Line-Signature").First();

            return contentHash == headerHash;
        }
    }

    public class LineMessageHandler
    {
        private Event lineEvent;
        private LineClient lineClient = new LineClient(ConfigurationManager.AppSettings["ChannelToken"].ToString());

        public LineMessageHandler(Event lineEvent)
        {
            this.lineEvent = lineEvent;
        }

        public async Task HandleBeaconEvent()
        {
        }

        public async Task HandleFollowEvent()
        {
        }

        public async Task HandleJoinEvent()
        {
        }

        public async Task HandleLeaveEvent()
        {
        }

        public async Task HandlePostbackEvent()
        {
            var replyMessage = new TextMessage(lineEvent.Postback.Data);
            await Reply(replyMessage);
        }

        public async Task HandleUnfollowEvent()
        {
        }

        public async Task<Profile> GetProfile(string mid)
        {
            return await lineClient.GetProfile(mid);
        }

        public async Task HandleTextMessage()
        {
            var textMessage = JsonConvert.DeserializeObject<TextMessage>(lineEvent.Message.ToString());
            Message replyMessage = null;
            if (textMessage.Text.ToLower() == "buttons")
            {
                List<TemplateAction> actions = new List<TemplateAction>();
                actions.Add(new MessageTemplateAction("Message Label", "sample data"));
                actions.Add(new PostbackTemplateAction("Postback Label", "sample data"));
                actions.Add(new UriTemplateAction("Uri Label", "https://github.com/kenakamu"));
                ButtonsTemplate buttonsTemplate = new ButtonsTemplate("https://github.com/apple-touch-icon.png", "Sample Title", "Sample Text", actions);

                replyMessage = new TemplateMessage("Buttons", buttonsTemplate);
            }
            else if (textMessage.Text.ToLower() == "confirm")
            {
                List<TemplateAction> actions = new List<TemplateAction>();
                actions.Add(new MessageTemplateAction("Yes", "yes"));
                actions.Add(new MessageTemplateAction("No", "no"));
                ConfirmTemplate confirmTemplate = new ConfirmTemplate("Confirm Test", actions);
                replyMessage = new TemplateMessage("Confirm", confirmTemplate);
            }
            else if (textMessage.Text.ToLower() == "carousel")
            {
                List<TemplateColumn> columns = new List<TemplateColumn>();
                List<TemplateAction> actions = new List<TemplateAction>();
                actions.Add(new MessageTemplateAction("Message Label", "sample data"));
                actions.Add(new PostbackTemplateAction("Postback Label", "sample data"));
                actions.Add(new UriTemplateAction("Uri Label", "https://github.com/kenakamu"));
                columns.Add(new TemplateColumn() { Title = "Casousel 1 Title", Text = "Casousel 1 Text", ThumbnailImageUrl = "https://github.com/apple-touch-icon.png", Actions = actions });
                columns.Add(new TemplateColumn() { Title = "Casousel 2 Title", Text = "Casousel 2 Text", ThumbnailImageUrl = "https://github.com/apple-touch-icon.png", Actions = actions });
                CarouselTemplate carouselTemplate = new CarouselTemplate(columns);
                replyMessage = new TemplateMessage("Carousel", carouselTemplate);
            }
            else
            {
                var intValue = 0;
                if (int.TryParse(textMessage.Text, out intValue))
                {
                    var personarity = (Personarities)(intValue % 25);
                    replyMessage = new TextMessage($"{personarity.PersonarityToString()} なメタモンだね。");
                } else
                {
                    replyMessage = new TextMessage(null);
                }
            }
            await Reply(replyMessage);
        }



        public async Task HandleMediaMessage()
        {
            Message message = JsonConvert.DeserializeObject<Message>(lineEvent.Message.ToString());
            // Get media from Line server.
            Media media = await lineClient.GetContent(message.Id);
            Message replyMessage = null;

            // Reply Image 
            switch (message.Type)
            {
                case MessageType.Image:
                case MessageType.Video:
                case MessageType.Audio:
                    replyMessage = new ImageMessage("https://github.com/apple-touch-icon.png", "https://github.com/apple-touch-icon.png");
                    break;
            }

            await Reply(replyMessage);
        }

        public async Task HandleStickerMessage()
        {
            //https://devdocs.line.me/files/sticker_list.pdf
            var stickerMessage = JsonConvert.DeserializeObject<StickerMessage>(lineEvent.Message.ToString());
            var replyMessage = new StickerMessage("1", "1");
            await Reply(replyMessage);
        }

        public async Task HandleLocationMessage()
        {
            var locationMessage = JsonConvert.DeserializeObject<LocationMessage>(lineEvent.Message.ToString());
            LocationMessage replyMessage = new LocationMessage(
                locationMessage.Title,
                locationMessage.Address,
                locationMessage.Latitude,
                locationMessage.Longitude);
            await Reply(replyMessage);
        }

        private async Task Reply(Message replyMessage)
        {
            try
            {
                await lineClient.ReplyToActivityAsync(lineEvent.CreateReply(message: replyMessage));
            }
            catch
            {
                await lineClient.PushAsync(lineEvent.CreatePush(message: replyMessage));
            }
        }
    }

    public enum Personarities
    {
        Ganbaruya = 0,
        Samisigari,
        Yuukan,
        Ijippari,
        Yancha,
        Zubutoi,
        Sunao,
        Nonki,
        Wanpaku,
        Noutenki,
        Okubyou,
        Sekkachi,
        Majime,
        Youki,
        Mujaki,
        Hikaeme,
        Ottori,
        Reisei,
        Tereya,
        Ukkariya,
        Odayaka,
        Otonashii,
        Namaiki,
        Shincho,
        Kimagure = 24,
        Unknown,
    }

    public static class PersonalityUtils
    {
        public static Personarities GetPersonarityFromString(string str)
        {
            switch (str)
            {
                case "がんばりや": return Personarities.Ganbaruya;
                case "さみしがり": return Personarities.Samisigari;
                case "ゆうかん": return Personarities.Yuukan;
                case "いじっぱり": return Personarities.Ijippari;
                case "やんちゃ": return Personarities.Yancha;
                case "ずぶとい": return Personarities.Zubutoi;
                case "すなお": return Personarities.Sunao;
                case "のんき": return Personarities.Nonki;
                case "わんぱく": return Personarities.Wanpaku;
                case "のうてんき": return Personarities.Noutenki;
                case "おくびょう": return Personarities.Okubyou;
                case "せっかち": return Personarities.Sekkachi;
                case "まじめ": return Personarities.Majime;
                case "ようき": return Personarities.Youki;
                case "むじゃき": return Personarities.Mujaki;
                case "ひかえめ": return Personarities.Hikaeme;
                case "おっとり": return Personarities.Ottori;
                case "れいせい": return Personarities.Reisei;
                case "てれや": return Personarities.Tereya;
                case "うっかりや": return Personarities.Ukkariya;
                case "おだやか": return Personarities.Odayaka;
                case "おとなしい": return Personarities.Otonashii;
                case "なまいき": return Personarities.Namaiki;
                case "しんちょう": return Personarities.Shincho;
                case "きまぐれ": return Personarities.Kimagure;
                default:
                    return Personarities.Unknown;
            }
        }
        public static string PersonarityToString(this Personarities self)
        {
            switch (self)
            {
                case Personarities.Ganbaruya: return "がんばりや";
                case Personarities.Hikaeme: return "ひかえめ";
                case Personarities.Ijippari: return "いじっぱり";
                case Personarities.Kimagure: return "きまぐれ";
                case Personarities.Majime: return "まじめ";
                case Personarities.Mujaki: return "むじゃき";
                case Personarities.Namaiki: return "なまいき";
                case Personarities.Nonki: return "のんき";
                case Personarities.Noutenki: return "のうてんき";
                case Personarities.Odayaka: return "おだやか";
                case Personarities.Okubyou: return "おくびょう";
                case Personarities.Otonashii: return "おとなしい";
                case Personarities.Ottori: return "おっとり";
                case Personarities.Reisei: return "れいせい";
                case Personarities.Samisigari: return "さみしがり";
                case Personarities.Sekkachi: return "せっかち";
                case Personarities.Shincho: return "しんちょう";
                case Personarities.Sunao: return "すなお";
                case Personarities.Tereya: return "てれや";
                case Personarities.Ukkariya: return "うっかりや";
                case Personarities.Wanpaku: return "わんぱく";
                case Personarities.Yancha: return "やんちゃ";
                case Personarities.Youki: return "ようき";
                case Personarities.Yuukan: return "ゆうかん";
                case Personarities.Zubutoi: return "ずぶとい";
                case Personarities.Unknown:
                default: return "ふめい";
            }
        }
    }
}

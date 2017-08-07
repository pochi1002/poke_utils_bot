using LineMessagingAPISDK;
using LineMessagingAPISDK.Models;
using Newtonsoft.Json;
using PokeUtils.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
                                await handler.HandleTextMessage(MessageHandler.Current);
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

        public async Task HandleTextMessage(MessageHandler handler)
        {
            var textMessage = JsonConvert.DeserializeObject<TextMessage>(lineEvent.Message.ToString());

            var reply = handler.HandleTextMessage(lineEvent.Source.UserId + lineEvent.Source.GroupId, textMessage);

            await Reply(new TextMessage(reply));
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
            //var stickerMessage = JsonConvert.DeserializeObject<StickerMessage>(lineEvent.Message.ToString());
            //var replyMessage = new StickerMessage("1", "1");
            //await Reply(replyMessage);
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

}

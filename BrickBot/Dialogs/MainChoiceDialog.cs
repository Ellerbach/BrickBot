using BrickBot.Models;
using BrickBot.Properties;
using BrickBot.Services.Bricklink;
using BrickBot.Services.BricksetService;
using BrickBot.Services.Peeron;
using BrickBot.Services.Rebrickable;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using System.Web;

namespace BrickBot.Dialogs
{
    [Serializable]
    public class MainChoiceDialog : IDialog<object>
    {
        //store the service URL to display logo and other HTML elements
        private string URL = ConfigurationManager.AppSettings["serviceurl"];
        //to store user currency preferences
        private ResumptionCookie resumptionCookie;
        private List<IBrickService> brickservices = new List<IBrickService>() { new BricklinkService(), new BricksetServiceAPI(), new RebrickableService(), new PeeronService() };

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            try
            {
                var message = await result;
                if (this.resumptionCookie == null)
                {
                    this.resumptionCookie = new ResumptionCookie(message);
                }
                if (message.Text == BrickBotRes.WelcomeSetCurrency)
                {
                    var reply = context.MakeMessage();
                    try
                    {
                        string setcurrency;
                        //need to implement a way to check the currncy.
                        context.PrivateConversationData.TryGetValue(BrickBotRes.CurrencyValue, out setcurrency);
                        reply.Text = BrickBotRes.CurrencyThankYou + $"Currency set: {setcurrency}";
                    }
                    catch (Exception err)
                    {
                        reply.Text = BrickBotRes.BrickBotError + $"{err.Message}";
                    }
                    await context.PostAsync(reply);
                }

                //string currency;

                //if ((!context.PrivateConversationData.TryGetValue(BrickBotRes.CurrencyValue, out currency)))
                //{
                //    PromptDialog.Text(context, this.ResumeAfterCurrency, BrickBotRes.CurrencyGiveMeCurrency);
                //    return;
                //}
                //else
                //{
                //    //myWivaldy.Connection = currency;
                //}

                if ((message.Text == BrickBotRes.WelcomeBricklink) || (message.Text == BrickBotRes.WelcomeBrickset) ||
                    (message.Text == BrickBotRes.WelcomePeeron) || (message.Text == BrickBotRes.WelcomeRebrickable))
                {
                    await this.BrickService(context, result);
                    return;
                }
                //if (message.Text == BrickBotRes.WelcomeBricklink)
                //{
                //    await this.Bricklink(context);
                //    return;
                //}
                //else if (message.Text == BrickBotRes.WelcomeBrickset)
                //{
                //    await this.Brickset(context);
                //    return;
                //}
                //else if (message.Text == BrickBotRes.WelcomeRebrickable)
                //{
                //    await this.Rebrickable(context);
                //    return;
                //}
                else if (message.Text == BrickBotRes.WelcomeAll)
                {
                    await this.BrickAll(context);
                    return;
                }
                //else if (message.Text == BrickBotRes.WelcomePeeron)
                //{
                //    await this.Peeron(context);
                //    return;
                //}
                await this.WelcomeMessageAsync(context);
            }
            catch (Exception ex)
            {
                var reply = context.MakeMessage();

                reply.Text = $"Ups, big error, {ex.Message}";

                await context.PostAsync(reply);
            }

        }
        #region Welcome
        private async Task WelcomeMessageAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();

            reply.Attachments = new List<Attachment>();
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: $"{URL}/Images/bricklogo-small.png"));
            List<CardAction> cardButtons = new List<CardAction>();
            cardButtons.Add(new CardAction() { Title = BrickBotRes.WelcomeBricklink, Value = BrickBotRes.WelcomeBricklink, Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = BrickBotRes.WelcomeBrickset, Value = BrickBotRes.WelcomeBrickset, Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = BrickBotRes.WelcomeRebrickable, Value = BrickBotRes.WelcomeRebrickable, Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = BrickBotRes.WelcomePeeron, Value = BrickBotRes.WelcomePeeron, Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = BrickBotRes.WelcomeAll, Value = BrickBotRes.WelcomeAll, Type = "postBack" });
            HeroCard plCard = new HeroCard()
            {
                Title = "Select the service youn want to search",
                //Subtitle = "Pig Latin Wikipedia Page",
                Images = cardImages,
                Buttons = cardButtons
            };

            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);

            await context.PostAsync(reply);

            context.Wait(this.MessageReceivedAsync);
        }

        private async Task OnOptionSelectedWelcome(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var reply = context.MakeMessage();
            string strresp = "You have selected ";

            if (message.Text == BrickBotRes.WelcomeBricklink)
            {
                strresp += "Bricklink ";
            }
            else if (message.Text == BrickBotRes.WelcomeBrickset)
            {
                strresp += "Brickset ";
            }
            else if (message.Text == BrickBotRes.WelcomeRebrickable)
            {
                strresp += "Rebrickable ";
            }
            else if (message.Text == BrickBotRes.WelcomeAll)
            {
                strresp += "all services ";
            }
            else if (message.Text == BrickBotRes.WelcomeSetCurrency)
            {
                strresp += "to change currency ";
            }

            reply.Text = strresp;
            reply.TextFormat = "markdown";
            await context.PostAsync(reply);
            //context.Wait(MessageReceivedAsync);
            await this.WelcomeMessageAsync(context);
        }

        private async Task ResumeAfterCurrency(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var currencyvalue = await result;
                //need to check the currency in real!
                await context.PostAsync(BrickBotRes.CurrencyThankYou);
                //myWivaldy.Connection = wivaldyconnection;
                context.PrivateConversationData.SetValue(BrickBotRes.CurrencyValue, currencyvalue);
            }
            catch (Exception ex)
            {
                await context.PostAsync(BrickBotRes.BrickBotError + $"{ex.Message}");
            }

            context.Wait(this.MessageReceivedAsync);
        }
        #endregion

        private IMessageActivity BuildMessage(IDialogContext context, BrickItem retbrick)
        {
            var reply = context.MakeMessage();
            if ((retbrick.BrickService == ServiceProvider.Bricklink) || (retbrick.BrickService == ServiceProvider.Brickset) ||
                (retbrick.BrickService == ServiceProvider.Rebrickable) || (retbrick.BrickService == ServiceProvider.Peeron))
            {

                reply.Attachments = new List<Attachment>();
                List<CardAction> cardButtons = new List<CardAction>();
                if (retbrick.Instructions != null)
                    foreach (var inst in retbrick.Instructions)
                    {
                        cardButtons.Add(new CardAction() { Title = inst.Name, Value = inst.URL, Type = "openUrl" });
                    }
                HeroCard plCard = new HeroCard()
                {
                    Title = $"# {retbrick.Number} - {retbrick.Name}"
                };
                if (retbrick.ThumbnailUrl != null)
                    if (retbrick.ThumbnailUrl != "")
                    {
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: retbrick.ThumbnailUrl));
                        plCard.Images = cardImages;
                    }
                if (retbrick.BrickURL != null)
                    if (retbrick.BrickURL != "")
                    {
                        cardButtons.Add(new CardAction() { Title = $"{retbrick.BrickService.ToString()} page", Value = retbrick.BrickURL, Type = "openUrl" });

                    }
                plCard.Buttons = cardButtons;
                plCard.Subtitle = $"Theme: {retbrick.Theme}";
                if (retbrick.YearReleased != 0)
                    plCard.Subtitle += $" - Year: {retbrick.YearReleased}";
                else
                    plCard.Subtitle += " - Year unknown";
                if (retbrick.Color != null)
                    if (retbrick.Color != "")
                        plCard.Subtitle += $" - Color: {retbrick.Color}";
                //string setcurrency;
                //need to implement a way to check the currncy.
                //context.PrivateConversationData.TryGetValue(BrickBotRes.CurrencyValue, out setcurrency);
                if (retbrick.BrickService == ServiceProvider.Bricklink)
                {
                    if (retbrick.New.Average != 0)
                        plCard.Text += $"New min {retbrick.New.Min.ToString("0.00")}; max {retbrick.New.Max.ToString("0.00")}; avg {retbrick.New.Average.ToString("0.00")} {retbrick.New.Currency}. ";
                    if (retbrick.Used.Average != 0)
                        plCard.Text += $"Used min {retbrick.Used.Min.ToString("0.00")}; max {retbrick.Used.Max.ToString("0.00")}; avg {retbrick.Used.Average.ToString("0.00")} {retbrick.Used.Currency}. ";
                }
                else
                    if (retbrick.New?.Average != null)
                    if (retbrick.New.Average != 0)
                        plCard.Text += $"Price {retbrick.New.Average} {retbrick.New.Currency}";

                reply.Attachments.Add(plCard.ToAttachment());
            }
            return reply;
        }

        #region All
        private async Task BrickAll(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: $"{URL}/Images/bricklogo.png"));
            List<CardAction> cardButtons = new List<CardAction>();
            foreach (var serv in Enum.GetValues(typeof(ItemType)))
            {
                cardButtons.Add(new CardAction() { Title = serv.ToString(), Value = serv.ToString(), Type = "postBack" });
            }
            HeroCard plCard = new HeroCard()
            {
                Title = BrickBotRes.BrickServiceSearchFor,
                Images = cardImages,
                Buttons = cardButtons
            };
            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
            await context.PostAsync(reply);
            context.Wait(this.OnOptionSelectedBrickAll);
        }

        private async Task OnOptionSelectedBrickAll(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            string retstr = "";
            foreach (var serv in Enum.GetValues(typeof(ItemType)))
            {
                if (serv.ToString() == message.Text)
                    retstr = BrickBotRes.ResourceManager.GetString($"{message.Text}Number");
            }
            context.PrivateConversationData.SetValue(BrickBotRes.WhatSearFor, message.Text);
            if (retstr != "")
                PromptDialog.Text(context, this.ResumeAfterAll, retstr);
            else
                await this.WelcomeMessageAsync(context);
        }

        private async Task ResumeAfterAll(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var number = await result;
                var reply = context.MakeMessage();
                await context.PostAsync(BrickBotRes.ThanksGiveMeASec);
                //find what was requested
                string whatyouwant;
                //need to implement a way to check the currncy.
                context.PrivateConversationData.TryGetValue(BrickBotRes.WhatSearFor, out whatyouwant);
                reply.Attachments = new List<Attachment>();

                foreach (var bs in brickservices)
                {
                    if (bs.GetSupportedInfo.Where(x => x.ToString() == whatyouwant).Any())
                    {
                        ItemType item;
                        var ret = Enum.TryParse<ItemType>(whatyouwant, out item);
                        if (ret)
                        {
                            var retbrick = bs.GetBrickInfo(number, item);
                            if (retbrick != null)
                            {
                                var replytmp = BuildMessage(context, retbrick);
                                reply.Attachments.Add(replytmp.Attachments.First());
                            }
                        }
                    }
                }
                if (!reply.Attachments.Any())
                {
                    reply.Text = BrickBotRes.SearchError;
                }

                await context.PostAsync(reply);

            }
            catch (Exception ex)
            {
                await context.PostAsync(BrickBotRes.BrickBotError + $"{ex.Message}");
            }

            //context.Wait(this.MessageReceivedAsync);
            await this.WelcomeMessageAsync(context);
        }
        #endregion

        #region IBrickService
        private async Task BrickService(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            // check which service has been called
            var message = await result;
            var bs = brickservices.Where(x => x.GetServiceProvider.ToString().ToLowerInvariant() == message.Text.ToLowerInvariant());
            var reply = context.MakeMessage();
            if (!bs.Any())
            {
                reply.Text = BrickBotRes.BrickBotError;
                await context.PostAsync(reply);
                await this.WelcomeMessageAsync(context);
            }

            reply.Attachments = new List<Attachment>();
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: $"{URL}/Images/{bs.First().GetServiceProvider.ToString().ToLower()}-square.png"));
            List<CardAction> cardButtons = new List<CardAction>();
            foreach (var serv in bs.First().GetSupportedInfo)
            {
                cardButtons.Add(new CardAction() { Title = serv.ToString(), Value = serv.ToString(), Type = "postBack" });
            }
            HeroCard plCard = new HeroCard()
            {
                Title = BrickBotRes.BrickServiceSearchFor,
                Images = cardImages,
                Buttons = cardButtons
            };
            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
            context.PrivateConversationData.SetValue(BrickBotRes.ServiceUsed, message.Text);

            await context.PostAsync(reply);

            context.Wait(this.OnOptionSelectedBrickService);
        }

        private async Task OnOptionSelectedBrickService(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            string retstr = "";
            string serviceused;
            context.PrivateConversationData.TryGetValue(BrickBotRes.ServiceUsed, out serviceused);
            var bs = brickservices.Where(x => x.GetServiceProvider.ToString().ToLowerInvariant() == serviceused.ToLowerInvariant());
            if (!bs.Any())
            {
                var reply = context.MakeMessage();
                reply.Text = BrickBotRes.BrickBotError;
                await context.PostAsync(reply);
                await this.WelcomeMessageAsync(context);
            }
            // check what is selected and make sure it is part of supported features
            foreach (var serv in bs.First().GetSupportedInfo)
            {
                if (serv.ToString().ToLowerInvariant() == message.Text.ToLowerInvariant())
                {
                    retstr = BrickBotRes.ResourceManager.GetString($"{message.Text}Number");
                }
            }
            if (message.Text == BrickBotRes.BricklinkSet)
            {
                retstr = BrickBotRes.SetNumber;
            }
            else if (message.Text == BrickBotRes.BricklinkPart)
            {
                retstr = BrickBotRes.PartNumber;
            }
            else if (message.Text == BrickBotRes.BricklinkMinifig)
            {
                retstr = BrickBotRes.MinifigNumber;
            }
            else if (message.Text == BrickBotRes.BricklinkBook)
            {
                retstr = BrickBotRes.BookNumber;
            }

            context.PrivateConversationData.SetValue(BrickBotRes.WhatSearFor, message.Text);
            if (retstr != "")
                PromptDialog.Text(context, this.ResumeAfterBrickService, retstr);
            else
                await this.WelcomeMessageAsync(context);
        }

        private async Task ResumeAfterBrickService(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var number = await result;
                var reply = context.MakeMessage();
                string serviceused;
                context.PrivateConversationData.TryGetValue(BrickBotRes.ServiceUsed, out serviceused);
                var bs = brickservices.Where(x => x.GetServiceProvider.ToString().ToLowerInvariant() == serviceused.ToLowerInvariant());
                //find what was requested
                string whatyouwant;
                //need to implement a way to check the currncy.
                context.PrivateConversationData.TryGetValue(BrickBotRes.WhatSearFor, out whatyouwant);
                ItemType selectitem;
                var retok = Enum.TryParse<ItemType>(whatyouwant, out selectitem);
                if (!bs.Any() || (!retok))
                {
                    reply.Text = BrickBotRes.BrickBotError;
                    await context.PostAsync(reply);
                    await this.WelcomeMessageAsync(context);
                }
                //need to check the currency in real!
                await context.PostAsync(BrickBotRes.ThanksGiveMeASec);
                //BricklinkService bs = new BricklinkService();

                var retbrick = bs.First().GetBrickInfo(number, selectitem);
                if (retbrick == null)
                    reply.Text = BrickBotRes.SearchError;
                else
                {
                    reply = BuildMessage(context, retbrick);
                }

                reply.TextFormat = "markdown";
                await context.PostAsync(reply);
            }
            catch (Exception ex)
            {
                await context.PostAsync(BrickBotRes.BrickBotError + $"{ex.Message}");
            }

            //context.Wait(this.MessageReceivedAsync);
            await this.WelcomeMessageAsync(context);
        }

        #endregion

    }
}
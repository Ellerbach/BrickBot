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

                if (message.Text == BrickBotRes.WelcomeBricklink)
                {
                    await this.Bricklink(context);
                    return;
                }
                else if (message.Text == BrickBotRes.WelcomeBrickset)
                {
                    await this.Brickset(context);
                    return;
                }
                else if (message.Text == BrickBotRes.WelcomeRebrickable)
                {
                    await this.Rebrickable(context);
                    return;
                }
                else if (message.Text == BrickBotRes.WelcomeAll)
                {
                    await this.BrickAll(context);
                    return;
                }
                else if (message.Text == BrickBotRes.WelcomePeeron)
                {
                    await this.Peeron(context);
                    return;
                }
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
            //else if ((retbrick.BrickService == ServiceProvider.Brickset) || (retbrick.BrickService == ServiceProvider.Rebrickable) || (retbrick.BrickService == ServiceProvider.Peeron))
            //{
            //    if (retbrick.Instructions != null)
            //    {
            //        reply.Attachments = new List<Attachment>();
            //        List<CardAction> cardAction = new List<CardAction>();
            //        foreach (var inst in retbrick.Instructions)
            //        {
            //            cardAction.Add(new CardAction() { Title = inst.Name, Value = inst.URL, Type = "openUrl" });
            //        }
            //        if (retbrick.BrickURL != "")
            //            cardAction.Add(new CardAction() { Title = $"{retbrick.BrickService.ToString()} page", Value = retbrick.BrickURL, Type = "openUrl" });
            //        HeroCard plCard = new HeroCard()
            //        {
            //            Title = $"# {retbrick.Number} - {retbrick.Name}",
            //            Buttons = cardAction
            //        };
            //        if (retbrick.ThumbnailUrl != null)
            //            if (retbrick.ThumbnailUrl != "")
            //            {
            //                List<CardImage> cardImages = new List<CardImage>();
            //                cardImages.Add(new CardImage(url: retbrick.ThumbnailUrl));

            //                plCard.Images = cardImages;
            //            }
            //        if (retbrick.YearReleased != 0)
            //            reply.Text += $"Year {retbrick.YearReleased} \r\n";

            //        reply.Text += $"Theme {retbrick.Theme} \r\n";
            //        if (retbrick.Color != null)
            //            if (retbrick.Color != "")
            //                reply.Text += $"Color {retbrick.Color} \r\n";
            //        if (retbrick.New?.Average != null)
            //            if (retbrick.New.Average != 0)
            //                reply.Text += $"Price {retbrick.New.Average} {retbrick.New.Currency}\r\n";

            //        reply.Attachments.Add(plCard.ToAttachment());
            //    }
            //    else
            //    {
            //        if (retbrick.ThumbnailUrl != "")
            //        {
            //            reply.Attachments = new List<Attachment>();
            //            List<CardImage> cardImages = new List<CardImage>();
            //            cardImages.Add(new CardImage(url: retbrick.ThumbnailUrl));
            //            HeroCard plCard = new HeroCard()
            //            {
            //                Title = $"\r\n# {retbrick.Number} - {retbrick.Name} \r\n",
            //                Images = cardImages
            //            };
            //            reply.Attachments.Add(plCard.ToAttachment());
            //        }
            //        reply.Text = $"\r\n# {retbrick.Number} - {retbrick.Name} \r\n";
            //        if (retbrick.YearReleased != 0)
            //            reply.Text += $"Year {retbrick.YearReleased} \r\n";

            //        reply.Text += $"Theme {retbrick.Theme} \r\n";
            //        if (retbrick.Color != null)
            //            if (retbrick.Color != "")
            //                reply.Text += $"Color {retbrick.Color} \r\n";
            //        if (retbrick.New?.Average != 0)
            //            reply.Text += $"Color {retbrick.Color} \r\n";
            //    }
            //}
            //reply.TextFormat = "markdown";
            return reply;
        }

        #region Bricklink
        private async Task Bricklink(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: $"{URL}/Images/bricklink.png"));
            List<CardAction> cardButtons = new List<CardAction>();
            cardButtons.Add(new CardAction() { Title = BrickBotRes.BricklinkSet, Value = BrickBotRes.BricklinkSet, Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = BrickBotRes.BricklinkPart, Value = BrickBotRes.BricklinkPart, Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = BrickBotRes.BricklinkMinifig, Value = BrickBotRes.BricklinkMinifig, Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = BrickBotRes.BricklinkBook, Value = BrickBotRes.BricklinkBook, Type = "postBack" });
            HeroCard plCard = new HeroCard()
            {
                Title = "Select what you want to search",
                //Subtitle = "Pig Latin Wikipedia Page",
                Images = cardImages,
                Buttons = cardButtons
            };
            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);

            await context.PostAsync(reply);

            context.Wait(this.OnOptionSelectedBricklink);
        }

        private async Task OnOptionSelectedBricklink(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            string retstr = "";

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
                PromptDialog.Text(context, this.ResumeAfterBricklink, retstr);
            else
                await this.WelcomeMessageAsync(context);
        }

        private async Task ResumeAfterBricklink(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var number = await result;
                var reply = context.MakeMessage();
                string strresp = "";
                //need to check the currency in real!
                await context.PostAsync(BrickBotRes.ThanksGiveMeASec);
                //find what was requested
                string whatyouwant;
                //need to implement a way to check the currncy.
                context.PrivateConversationData.TryGetValue(BrickBotRes.WhatSearFor, out whatyouwant);
                BricklinkService bs = new BricklinkService();

                if (whatyouwant == BrickBotRes.BricklinkSet)
                {
                    var retbrick = bs.GetCatalogItem(number, Models.Bricklink.TypeDescription.SET);
                    if (retbrick == null)
                    {
                        number += "-1";
                        retbrick = bs.GetCatalogItem(number, Models.Bricklink.TypeDescription.SET);
                    }
                    if (retbrick == null)
                        reply.Text = BrickBotRes.SearchError;
                    else
                    {
                        reply = BuildMessage(context, retbrick);
                    }
                }
                else if (whatyouwant == BrickBotRes.BricklinkPart)
                {
                    var retbrick = bs.GetCatalogItem(number, Models.Bricklink.TypeDescription.PART);
                    if (retbrick == null)
                        reply.Text = BrickBotRes.SearchError;
                    else
                    {
                        reply = BuildMessage(context, retbrick);
                    }

                }
                else if (whatyouwant == BrickBotRes.BricklinkMinifig)
                {
                    var retbrick = bs.GetCatalogItem(number, Models.Bricklink.TypeDescription.MINIFIG);
                    if (retbrick == null)
                        reply.Text = BrickBotRes.SearchError;
                    else
                    {
                        reply = BuildMessage(context, retbrick);
                    }

                }
                else if (whatyouwant == BrickBotRes.BricklinkBook)
                {
                    var retbrick = bs.GetCatalogItem(number, Models.Bricklink.TypeDescription.BOOK);
                    if (retbrick == null)
                        reply.Text = BrickBotRes.SearchError;
                    else
                    {
                        reply = BuildMessage(context, retbrick);
                    }

                }
                else
                {
                    reply.Text = BrickBotRes.BrickBotError;
                }

                //reply.Text = strresp;
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

        #region Brickset
        private async Task Brickset(IDialogContext context)
        {
            var reply = context.MakeMessage();

            reply.Attachments = new List<Attachment>();
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: $"{URL}/Images/brickset.png"));
            List<CardAction> cardButtons = new List<CardAction>();
            cardButtons.Add(new CardAction() { Title = BrickBotRes.BricksetSet, Value = BrickBotRes.BricksetSet, Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = BrickBotRes.BricksetInstructions, Value = BrickBotRes.BricksetInstructions, Type = "postBack" });
            HeroCard plCard = new HeroCard()
            {
                Title = "Select what you want to search",
                Images = cardImages,
                Buttons = cardButtons
            };
            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
            await context.PostAsync(reply);

            context.Wait(this.OnOptionSelectedBrickset);
        }

        private async Task OnOptionSelectedBrickset(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            string retstr = "";

            if (message.Text == BrickBotRes.BricksetSet)
            {
                retstr = BrickBotRes.SetNumber;
            }
            else if (message.Text == BrickBotRes.BricksetInstructions)
            {
                retstr = BrickBotRes.InstructionsNumber;
            }
            context.PrivateConversationData.SetValue(BrickBotRes.WhatSearFor, message.Text);
            if (retstr != "")
                PromptDialog.Text(context, this.ResumeAfterBrickset, retstr);
            else
                await this.WelcomeMessageAsync(context);
        }

        private async Task ResumeAfterBrickset(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var number = await result;
                var reply = context.MakeMessage();
                string strresp = "";
                //need to check the currency in real!
                await context.PostAsync(BrickBotRes.ThanksGiveMeASec);
                //find what was requested
                string whatyouwant;
                //need to implement a way to check the currncy.
                context.PrivateConversationData.TryGetValue(BrickBotRes.WhatSearFor, out whatyouwant);
                BricksetServiceAPI bs = new BricksetServiceAPI();

                if (whatyouwant == BrickBotRes.BricksetSet)
                {
                    var retbrick = bs.getSets(number);
                    if (retbrick == null)
                    {
                        number += "-1";
                        retbrick = bs.getSets(number);
                    }
                    if (retbrick == null)
                        reply.Text = BrickBotRes.SearchError;
                    else
                    {
                        reply = BuildMessage(context, retbrick);
                    }
                }
                else if (whatyouwant == BrickBotRes.BricksetInstructions)
                {
                    var retbrick = bs.getInstructions(number);
                    if (retbrick == null)
                    {
                        number += "-1";
                        retbrick = bs.getInstructions(number);
                    }
                    if (retbrick == null)
                        reply.Text = BrickBotRes.SearchError;
                    else
                    {
                        reply = BuildMessage(context, retbrick);
                    }
                }
                else
                {
                    reply.Text = BrickBotRes.BrickBotError;
                }

                //reply.Text = strresp;
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

        #region Rebrickable
        private async Task Rebrickable(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: $"{URL}/Images/rebrickable.png"));
            List<CardAction> cardButtons = new List<CardAction>();
            cardButtons.Add(new CardAction() { Title = BrickBotRes.RebrickableSet, Value = BrickBotRes.RebrickableSet, Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = BrickBotRes.RebrickablePart, Value = BrickBotRes.RebrickablePart, Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = BrickBotRes.RebrickableMoc, Value = BrickBotRes.RebrickableMoc, Type = "postBack" });
            HeroCard plCard = new HeroCard()
            {
                Title = "Select what you want to search",
                //Subtitle = "Pig Latin Wikipedia Page",
                Images = cardImages,
                Buttons = cardButtons
            };
            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
            await context.PostAsync(reply);
            context.Wait(this.OnOptionSelectedRebrickable);
        }

        private async Task OnOptionSelectedRebrickable(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            string retstr = "";

            if (message.Text == BrickBotRes.RebrickableSet)
            {
                retstr = BrickBotRes.SetNumber;
            }
            else if (message.Text == BrickBotRes.RebrickablePart)
            {
                retstr = BrickBotRes.PartNumber;
            }
            else if (message.Text == BrickBotRes.RebrickableMoc)
            {
                retstr = BrickBotRes.MocNumber;
            }
            context.PrivateConversationData.SetValue(BrickBotRes.WhatSearFor, message.Text);
            if (retstr != "")
                PromptDialog.Text(context, this.ResumeAfterRebrickable, retstr);
            else
                await this.WelcomeMessageAsync(context);
        }

        private async Task ResumeAfterRebrickable(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var number = await result;
                var reply = context.MakeMessage();
                string strresp = "";
                //need to check the currency in real!
                await context.PostAsync(BrickBotRes.ThanksGiveMeASec);
                //find what was requested
                string whatyouwant;
                //need to implement a way to check the currncy.
                context.PrivateConversationData.TryGetValue(BrickBotRes.WhatSearFor, out whatyouwant);
                RebrickableService bs = new RebrickableService();

                if (whatyouwant == BrickBotRes.RebrickableSet)
                {
                    var retbrick = bs.GetSetInfo(number);
                    if (retbrick == null)
                    {
                        number += "-1";
                        retbrick = bs.GetSetInfo(number);
                    }
                    if (retbrick == null)
                        reply.Text = BrickBotRes.SearchError;
                    else
                    {
                        reply = BuildMessage(context, retbrick);
                    }
                }
                else if (whatyouwant == BrickBotRes.RebrickableMoc)
                {
                    var retbrick = bs.GetMOCInfo(number);
                    if (retbrick == null)
                    {
                        number = "MOC-" + number;
                        retbrick = bs.GetMOCInfo(number);
                    }
                    if (retbrick == null)
                        reply.Text = BrickBotRes.SearchError;
                    else
                    {
                        reply = BuildMessage(context, retbrick);
                    }
                }
                else if (whatyouwant == BrickBotRes.RebrickablePart)
                {
                    var retbrick = bs.GetPartInfo(number);
                    if (retbrick == null)
                        reply.Text = BrickBotRes.SearchError;
                    else
                    {
                        reply = BuildMessage(context, retbrick);
                    }
                }
                else
                {
                    reply.Text = BrickBotRes.BrickBotError;
                }

                //reply.Text = strresp;
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

        #region Peeron
        private async Task Peeron(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: $"{URL}/Images/peeron.png"));
            List<CardAction> cardButtons = new List<CardAction>();
            cardButtons.Add(new CardAction() { Title = BrickBotRes.PeeronSet, Value = BrickBotRes.PeeronSet, Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = BrickBotRes.PeeronInstructions, Value = BrickBotRes.PeeronInstructions, Type = "postBack" });
            HeroCard plCard = new HeroCard()
            {
                Title = "Select what you want to search",
                //Subtitle = "Pig Latin Wikipedia Page",
                Images = cardImages,
                Buttons = cardButtons
            };
            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
            await context.PostAsync(reply);
            context.Wait(this.OnOptionSelectedPeeron);
        }

        private async Task OnOptionSelectedPeeron(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            string retstr = "";
            if (message.Text == BrickBotRes.PeeronSet)
            {
                retstr = BrickBotRes.SetNumber;
            }
            else if (message.Text == BrickBotRes.PeeronInstructions)
            {
                retstr = BrickBotRes.InstructionsNumber;
            }
            context.PrivateConversationData.SetValue(BrickBotRes.WhatSearFor, message.Text);
            if (retstr != "")
                PromptDialog.Text(context, this.ResumeAfterPeeron, retstr);
            else
                await this.WelcomeMessageAsync(context);

            //await this.WelcomeMessageAsync(context);
        }

        private async Task ResumeAfterPeeron(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var number = await result;
                var reply = context.MakeMessage();
                string strresp = "";
                //need to check the currency in real!
                await context.PostAsync(BrickBotRes.ThanksGiveMeASec);
                //find what was requested
                string whatyouwant;
                //need to implement a way to check the currncy.
                context.PrivateConversationData.TryGetValue(BrickBotRes.WhatSearFor, out whatyouwant);
                PeeronService bs = new PeeronService();

                if ((whatyouwant == BrickBotRes.PeeronSet) || (whatyouwant == BrickBotRes.PeeronInstructions))
                {
                    var retbrick = bs.GetSetDetails(number);
                    if (retbrick == null)
                        reply.Text = BrickBotRes.SearchError;
                    else
                    {
                        reply = BuildMessage(context, retbrick);
                    }
                }
                else
                {
                    reply.Text = BrickBotRes.BrickBotError;
                }

                //reply.Text = strresp;
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

        #region All
        private async Task BrickAll(IDialogContext context)
        {
            var reply = context.MakeMessage();

            var options = new[]
            {
                BrickBotRes.AllSet,
                BrickBotRes.AllInstructions
            };
            reply.AddHeroCard(
                "Select what you want to search",
                options,
                new[] { $"{URL}/Images/rebrickable.png" });

            await context.PostAsync(reply);

            context.Wait(this.OnOptionSelectedBrickAll);
        }

        private async Task OnOptionSelectedBrickAll(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var reply = context.MakeMessage();
            string strresp = "You have selected ";

            if (message.Text == BrickBotRes.AllSet)
            {
                strresp += "Set ";
            }
            else if (message.Text == BrickBotRes.AllInstructions)
            {
                strresp += "Instructions ";
            }

            reply.Text = strresp;
            reply.TextFormat = "markdown";
            await context.PostAsync(reply);
            //context.Wait(MessageReceivedAsync);
            //await this.WelcomeMessageAsync(context);
            await this.WelcomeMessageAsync(context);
        }
        #endregion
    }
}
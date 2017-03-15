using BrickBot.Models;
using BrickBot.Properties;
using BrickBot.Services.Bricklink;
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

                string currency;

                if ((!context.PrivateConversationData.TryGetValue(BrickBotRes.CurrencyValue, out currency)))
                {
                    PromptDialog.Text(context, this.ResumeAfterCurrency, BrickBotRes.CurrencyGiveMeCurrency);
                    return;
                }
                else
                {
                    //myWivaldy.Connection = currency;
                }

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

            var options = new[]
            {
                BrickBotRes.WelcomeBricklink,
                BrickBotRes.WelcomeBrickset,
                BrickBotRes.WelcomeRebrickable,
                BrickBotRes.WelcomeAll,
                BrickBotRes.WelcomeBricklink
            };
            reply.AddHeroCard(
                "Welcome to BrickBot",
                "Select your provider",
                options); //,
                          //new[] { $"{URL}/Images/bricklogo.png" });

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
            reply.Text = $"\r\n# {retbrick.Number} - {retbrick.Name} \r\n";
            reply.Text = $"Year {retbrick.YearReleased} \r\n";
            if (retbrick.ThumbnailUrl != "")
                reply.Attachments.Add(new Attachment(retbrick.ThumbnailUrl));
            reply.Text = $"Theme {retbrick.Theme} \r\n";
            if (retbrick.Color != "")
                reply.Text = $"Color {retbrick.Color} \r\n";
            if (retbrick.BrickService == ServiceProvider.Bricklink)
            {
                if (retbrick.New.Average != 0)
                    reply.Text = $"New: min {retbrick.New.Min} max {retbrick.New.Max} avg {retbrick.New.Average} \r\n";
                if (retbrick.Used.Average != 0)
                    reply.Text = $"Used: min {retbrick.Used.Min} max {retbrick.Used.Max} avg {retbrick.Used.Average} \r\n";
            }
            return reply;
        }

        #region Bricklink
        private async Task Bricklink(IDialogContext context)
        {
            var reply = context.MakeMessage();

            var options = new[]
            {
                BrickBotRes.BricklinkSet,
                BrickBotRes.BricklinkPart,
                BrickBotRes.BricklinkMinifig,
                BrickBotRes.BricklinkBook
            };
            reply.AddHeroCard(
                "Bricklink",
                "Select what you want to search",
                options,
                new[] { $"{URL}/Images/bricklink.png" });

            await context.PostAsync(reply);

            context.Wait(this.OnOptionSelectedBricklink);
        }

        private async Task OnOptionSelectedBricklink(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var reply = context.MakeMessage();
            string strresp = "You have selected ";
            string retstr = "";

            if (message.Text == BrickBotRes.BricklinkSet)
            {
                retstr = BrickBotRes.SetNumber;
                strresp += "Set ";
            }
            else if (message.Text == BrickBotRes.BricklinkPart)
            {
                retstr = BrickBotRes.PartNumber;
                strresp += "Part ";
            }
            else if (message.Text == BrickBotRes.BricklinkMinifig)
            {
                retstr = BrickBotRes.MinifigNumber;
                strresp += "Minifig ";
            }
            else if (message.Text == BrickBotRes.BricklinkBook)
            {
                retstr = BrickBotRes.BookNumber;
                strresp += "Book ";
            }

            context.ConversationData.SetValue(BrickBotRes.WhatSearFor, message.Text);
            reply.Text = strresp;
            reply.TextFormat = "markdown";
            await context.PostAsync(reply);
            //context.Wait(MessageReceivedAsync);
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
                        reply.Text = BrickBotRes.BrickBotError;
                    else
                    {
                        reply = BuildMessage(context, retbrick);
                    }
                }
                else if (whatyouwant == BrickBotRes.BricklinkPart)
                {

                }
                else if (whatyouwant == BrickBotRes.BricklinkMinifig)
                {

                }
                else if (whatyouwant == BrickBotRes.BricklinkBook)
                {

                }
                else
                {
                    reply.Text = BrickBotRes.BrickBotError;
                }

                reply.Text = strresp;
                reply.TextFormat = "markdown";
                await context.PostAsync(reply);
            }
            catch (Exception ex)
            {
                await context.PostAsync(BrickBotRes.BrickBotError + $"{ex.Message}");
            }

            context.Wait(this.MessageReceivedAsync);
        }

        #endregion
        #region Brickset
        private async Task Brickset(IDialogContext context)
        {
            var reply = context.MakeMessage();

            var options = new[]
            {
                BrickBotRes.BricksetSet,
                BrickBotRes.BricksetInstructions
            };
            reply.AddHeroCard(
                "Brickset",
                "Select what you want to search",
                options,
                new[] { $"{URL}/Images/brickset.png" });

            await context.PostAsync(reply);

            context.Wait(this.OnOptionSelectedBrickset);
        }

        private async Task OnOptionSelectedBrickset(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var reply = context.MakeMessage();
            string strresp = "You have selected ";

            if (message.Text == BrickBotRes.BricksetSet)
            {
                strresp += "Set ";
            }
            else if (message.Text == BrickBotRes.BricksetInstructions)
            {
                strresp += "Instructions ";
            }

            reply.Text = strresp;
            reply.TextFormat = "markdown";
            await context.PostAsync(reply);
            //context.Wait(MessageReceivedAsync);
            await this.WelcomeMessageAsync(context);
        }

        #endregion
        #region Rebrickable
        private async Task Rebrickable(IDialogContext context)
        {
            var reply = context.MakeMessage();

            var options = new[]
            {
                BrickBotRes.RebrickableSet,
                BrickBotRes.RebrickablePart,
                BrickBotRes.RebrickableMoc
            };
            reply.AddHeroCard(
                "Rebrickable",
                "Select what you want to search",
                options,
                new[] { $"{URL}/Images/Rebrickable.png" });

            await context.PostAsync(reply);

            context.Wait(this.OnOptionSelectedRebrickable);
        }

        private async Task OnOptionSelectedRebrickable(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var reply = context.MakeMessage();
            string strresp = "You have selected ";

            if (message.Text == BrickBotRes.RebrickableSet)
            {
                strresp += "Set ";
            }
            else if (message.Text == BrickBotRes.RebrickablePart)
            {
                strresp += "Part ";
            }
            else if (message.Text == BrickBotRes.RebrickableMoc)
            {
                strresp += "MOC ";
            }

            reply.Text = strresp;
            reply.TextFormat = "markdown";
            await context.PostAsync(reply);
            //context.Wait(MessageReceivedAsync);
            await this.WelcomeMessageAsync(context);
        }
        #endregion
        #region Peeron
        private async Task Peeron(IDialogContext context)
        {
            var reply = context.MakeMessage();

            var options = new[]
            {
                BrickBotRes.PeeronSet,
                BrickBotRes.PeeronInstructions
            };
            reply.AddHeroCard(
                "Rebrickable",
                "Select what you want to search",
                options,
                new[] { $"{URL}/Images/Rebrickable.png" });

            await context.PostAsync(reply);

            context.Wait(this.OnOptionSelectedPeeron);
        }

        private async Task OnOptionSelectedPeeron(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var reply = context.MakeMessage();
            string strresp = "You have selected ";

            if (message.Text == BrickBotRes.PeeronSet)
            {
                strresp += "Set ";
            }
            else if (message.Text == BrickBotRes.PeeronInstructions)
            {
                strresp += "Instructions ";
            }

            reply.Text = strresp;
            reply.TextFormat = "markdown";
            await context.PostAsync(reply);
            //context.Wait(MessageReceivedAsync);
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
                "All services",
                "Select what you want to search",
                options,
                new[] { $"{URL}/Images/Rebrickable.png" });

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
            await this.WelcomeMessageAsync(context);
        }
        #endregion
    }
}
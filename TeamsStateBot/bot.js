// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    ActivityHandler
} = require('botbuilder');

const {
    TeamsActivityProcessor,
    TeamsContext
} = require('botbuilder-teams');

const CONVERSATION_DATA_PROPERTY = 'conversationData';
const USER_PROFILE_PROPERTY = 'userProfile';

class MyBot extends ActivityHandler {
    constructor(conversationState, userState) {

        super();
        this.conversationState = conversationState;
        this.userState = userState;

        // Create accessors for state properties
        this.conversationData = conversationState.createProperty(CONVERSATION_DATA_PROPERTY);
        this.userProfile = userState.createProperty(USER_PROFILE_PROPERTY);

        this.onMessage(async (turnContext, next) => {

            const teamsContext = TeamsContext.from(turnContext);
            if (!teamsContext) {
                await turnContext.sendActivity('Sorry, this bot only works in Teams');
                await next();
                return;
            }

            // Get the state properties from the turn context.
            const userProfile = await this.userProfile.get(turnContext, {});
            const conversationData = await this.conversationData.get(
                turnContext, { promptedForUserName: false });

            if (!userProfile.name) {

                // First time around this is undefined, so we will prompt user for name.
                if (!conversationData.promptedForUserName) {
                    // We haven't prompted them yet, so do it now.
                    await turnContext.sendActivity('What is your name?');
                    // Set the flag to true, so we don't prompt in the next turn.
                    conversationData.promptedForUserName = true;
                } else {
                    // We prompted them, this must be their name.
                    userProfile.name = turnContext.activity.text;
                    // Acknowledge that we got their name.
                    await turnContext.sendActivity(`Thanks ${userProfile.name}, now I'll repeat everything you say.`);
                }

            } else {

                const text = teamsContext.getActivityTextWithoutMentions();
                await turnContext.sendActivity(`${userProfile.name} said, "${text}"`);

            }

            // Update turncount in conversation data
            let turnCount = conversationData.turnCount || 0;
            conversationData.turnCount = ++turnCount;

            // Add message details to the conversation data.
            if (teamsContext.channel) {
                // If we have a channel, show its id
                await turnContext.sendActivity(
                    `(We've been chatting over channel ${teamsContext.channel.id}
                         for ${conversationData.turnCount} turns)`);
            } else {
                // If we're not running in a channel, skip that
                await turnContext.sendActivity(
                    `(We've been chatting for ${conversationData.turnCount} turns)`);
            }

            // Persist any state changes during this turn.
            await this.conversationState.saveChanges(turnContext, false);
            await this.userState.saveChanges(turnContext, false);

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });

        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let cnt = 0; cnt < membersAdded.length; ++cnt) {
                if (membersAdded[cnt].id !== context.activity.recipient.id) {
                    await context.sendActivity('Welcome to StateBot!');
                }
            }
            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }
}

module.exports.MyBot = MyBot;

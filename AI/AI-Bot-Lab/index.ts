// Import required packages
import * as restify from "restify";

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
import {
  CloudAdapter,
  ConfigurationServiceClientCredentialFactory,
  ConfigurationBotFrameworkAuthentication,
  TurnContext,
} from "botbuilder";

// This bot's main dialog.
import { TeamsBot } from "./teamsBot";
import config from "./config";

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const credentialsFactory = new ConfigurationServiceClientCredentialFactory({
  MicrosoftAppId: config.botId,
  MicrosoftAppPassword: config.botPassword,
  MicrosoftAppType: "MultiTenant",
});

const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(
  {},
  credentialsFactory
);

const adapter = new CloudAdapter(botFrameworkAuthentication);

// Catch-all for errors.
const onTurnErrorHandler = async (context: TurnContext, error: Error) => {
  // This check writes out errors to console log .vs. app insights.
  // NOTE: In production environment, you should consider logging this to Azure
  //       application insights.
  console.error(`\n [onTurnError] unhandled error: ${error}`);

  // Send a trace activity, which will be displayed in Bot Framework Emulator
  await context.sendTraceActivity(
    "OnTurnError Trace",
    `${error}`,
    "https://www.botframework.com/schemas/error",
    "TurnError"
  );

  // Send a message to the user
  await context.sendActivity(`The bot encountered unhandled error:\n ${error.message}`);
  await context.sendActivity("To continue to run this bot, please fix the bot source code.");
};

// Set the onTurnError for the singleton CloudAdapter.
adapter.onTurnError = onTurnErrorHandler;

// Create the bot that will handle incoming messages.
const bot = new TeamsBot();

// Create HTTP server.
const server = restify.createServer();
server.use(restify.plugins.bodyParser());
server.listen(process.env.port || process.env.PORT || 3978, () => {
  console.log(`\nBot Started, ${server.name} listening to ${server.url}`);
});

import { 
  Application, 
  ConversationHistory, 
  DefaultPromptManager, 
  DefaultTurnState, 
  // OpenAIModerator, 
  OpenAIPlanner, 
  AI, 
  AzureOpenAIPlanner
} from '@microsoft/teams-ai';
import path from "path";
import { MemoryStorage } from "botbuilder";

// eslint-disable-next-line @typescript-eslint/no-empty-interface
interface ConversationState {}
type ApplicationTurnState = DefaultTurnState<ConversationState>;

// console.log( `Azure OpenAI Endpoint: ${config.endpoint}` );
// console.log( `Azure OpenAI Key: ${config.azureOpenAIKey}` );

// Create AI components
// THIS WORKS:
const planner = new OpenAIPlanner({
  apiKey: config.openAIKey,
  defaultModel: 'text-davinci-003',
  logRequests: true
});

//THIS FAILS:
// const planner = new AzureOpenAIPlanner({  // OpenAIPlanner({
//   apiKey: config.azureOpenAIKey, // config.openAIKey,
//   endpoint: config.endpoint,
//   defaultModel: 'text-davinci-003',
//   logRequests: true
// });

// REMOVED since supposedly moderation doesn't work w/Azure OpenAI
// per comment on https://github.com/microsoft/hack-together-teams/discussions/62
// const moderator = new OpenAIModerator({
//   apiKey: config.azureOpenAIKey,  //config.openAIKey,
//   moderate: 'both'
// });
const promptManager = new DefaultPromptManager(path.join(__dirname, './prompts' ));

// Define storage and application
const storage = new MemoryStorage();
const app = new Application<ApplicationTurnState>({
  storage,
  ai: {
      planner,
      // moderator,
      promptManager,
      // prompt: 'command',
      prompt: 'chat',
      history: {
          assistantHistoryType: 'text'
      }
  }
});

app.ai.action(AI.FlaggedInputActionName, async (context, state, data) => {
  await context.sendActivity(`I'm sorry your message was flagged: ${JSON.stringify(data)}`);
  return false;
});

app.ai.action(AI.FlaggedOutputActionName, async (context, state, data) => {
  await context.sendActivity(`I'm not allowed to talk about such things.`);
  return false;
});

app.message('/history', async (context, state) => {
  const history = ConversationHistory.toString(state, 2000, '\n\n');
  await context.sendActivity(history);
  });

// Listen for incoming requests.
server.post("/api/messages", async (req, res) => {
  await adapter.process(req, res, async (context) => {
    await app.run(context);
  });
});

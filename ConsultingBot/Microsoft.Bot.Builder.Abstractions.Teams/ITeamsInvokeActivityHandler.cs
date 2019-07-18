// <copyright file="ITeamsInvokeActivityHandler.cs" company="Microsoft">
// Licensed under the MIT License.
// </copyright>

namespace Microsoft.Bot.Builder.Abstractions.Teams
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema.Teams;

    /// <summary>
    /// Handles the teams invoke activities.
    /// </summary>
    /// <seealso cref="IInvokeActivityHandler" />
    public interface ITeamsInvokeActivityHandler : IInvokeActivityHandler
    {
        /// <summary>
        /// Handles the o365 connector card action asynchronously.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The o365 connector card action action.</param>
        /// <returns>Task tracking operation.</returns>
        Task<InvokeResponse> HandleO365ConnectorCardActionAsync(ITurnContext turnContext, O365ConnectorCardActionQuery query);

        /// <summary>
        /// Handles the signin state verification action asynchronously.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The signin state verification action.</param>
        /// <returns>Task tracking operation.</returns>
        Task<InvokeResponse> HandleSigninStateVerificationActionAsync(ITurnContext turnContext, SigninStateVerificationQuery query);

        /// <summary>
        /// Handles file consent response asynchronously.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The query object of file consent user's response.</param>
        /// <returns>Task tracking operation.</returns>
        Task<InvokeResponse> HandleFileConsentResponseAsync(ITurnContext turnContext, FileConsentCardResponse query);

        /// <summary>
        /// Handles the messaging extension action asynchronously.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The messaging extension action.</param>
        /// <returns>Task tracking operation.</returns>
        Task<InvokeResponse> HandleMessagingExtensionQueryAsync(ITurnContext turnContext, MessagingExtensionQuery query);

        /// <summary>
        /// Handles app-based link query asynchronously.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The app-based link query.</param>
        /// <returns>Task tracking operation.</returns>
        Task<InvokeResponse> HandleAppBasedLinkQueryAsync(ITurnContext turnContext, AppBasedLinkQuery query);

        /// <summary>
        /// Handles messaging extension action of "fetch task" asynchronously.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The query object of messaging extension action.</param>
        /// <returns>Task tracking operation.</returns>
        Task<InvokeResponse> HandleMessagingExtensionFetchTaskAsync(ITurnContext turnContext, MessagingExtensionAction query);

        /// <summary>
        /// Handles messaging extension action of "submit action" asynchronously.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The query object of messaging extension action.</param>
        /// <returns>Task tracking operation.</returns>
        Task<InvokeResponse> HandleMessagingExtensionSubmitActionAsync(ITurnContext turnContext, MessagingExtensionAction query);

        /// <summary>
        /// Handles task module fetch asynchronously.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The query object of task module request.</param>
        /// <returns>Task tracking operation.</returns>
        Task<InvokeResponse> HandleTaskModuleFetchAsync(ITurnContext turnContext, TaskModuleRequest query);

        /// <summary>
        /// Handles task module submit asynchronously.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">The query object of task module request.</param>
        /// <returns>Task tracking operation.</returns>
        Task<InvokeResponse> HandleTaskModuleSubmitAsync(ITurnContext turnContext, TaskModuleRequest query);
    }
}

using EchoBot1.Models;
using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBot1.Services
{
    public class BotStateService
    {
        // State variables
        public UserState UserState { get; }
        public ConversationState ConversationState { get; }

        // Property IDs
        public static string UserProfileId { get; } = $"{nameof(BotStateService)}.UserProfile";
        public static string ConversationDataId { get; } = $"{nameof(BotStateService)}.ConversationData";
        
        // Property accessors
        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }

        public BotStateService(UserState userState, ConversationState conversationState)
        {
            UserState = userState ?? throw new ArgumentException(nameof(userState));
            ConversationState = conversationState ?? throw new ArgumentException(nameof(conversationState));
            InitializeAccessors();
        }

        private void InitializeAccessors()
        {
            UserProfileAccessor = UserState.CreateProperty<UserProfile>(UserProfileId);
            ConversationDataAccessor = ConversationState.CreateProperty<ConversationData>(ConversationDataId);
        }
    }
}

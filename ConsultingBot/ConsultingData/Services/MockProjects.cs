using ConsultingData.Models;
using System.Collections.Generic;
using System.Linq;

namespace ConsultingData.Services
{
    public static class MockProjects
    {
        public static List<ConsultingProject> data = new List<ConsultingProject> 
            {
                new ConsultingProject()
                {
                    ProjectId = 1,
                    Client = MockClients.data.FirstOrDefault<ConsultingClient>(
                        (c) => c.ClientId == 1),
                    Name = "Teams Migration",
                    Description = "Get everybody on Teams",
                    DocumentsUrl = "https://bgtest18.sharepoint.com/sites/Bot4Test/Shared Documents/",
                    TeamUrl = "https://teams.microsoft.com/l/team/19%3ad90eb69425bc4cc7a6a43df04be83bba%40thread.skype/conversations?groupId=a54cd0e7-1d00-420a-8b51-060c288620eb&tenantId=a25d4ef1-c73a-4dc1-bdb1-9a342260f216"
                },
                new ConsultingProject()
                {
                    ProjectId = 2,
                    Client = MockClients.data.FirstOrDefault<ConsultingClient>(
                        (c) => c.ClientId == 2),
                    Name = "Back Burner",
                    Description = "This project is going nowhere",
                    DocumentsUrl = "https://bgtest18.sharepoint.com/sites/Bot4Test/Shared Documents/",
                    TeamUrl = "https://teams.microsoft.com/l/team/19%3ad90eb69425bc4cc7a6a43df04be83bba%40thread.skype/conversations?groupId=a54cd0e7-1d00-420a-8b51-060c288620eb&tenantId=a25d4ef1-c73a-4dc1-bdb1-9a342260f216"
                },
                new ConsultingProject()
                {
                    ProjectId = 3,
                    Client = MockClients.data.FirstOrDefault<ConsultingClient>(
                        (c) => c.ClientId == 2),
                    Name = "Teams Development",
                    Description = "Develop a killer app in Teams",
                    DocumentsUrl = "https://bgtest18.sharepoint.com/sites/Bot4Test/Shared Documents/",
                    TeamUrl = "https://teams.microsoft.com/l/team/19%3ad90eb69425bc4cc7a6a43df04be83bba%40thread.skype/conversations?groupId=a54cd0e7-1d00-420a-8b51-060c288620eb&tenantId=a25d4ef1-c73a-4dc1-bdb1-9a342260f216"
                },
                new ConsultingProject()
                {
                    ProjectId = 4,
                    Client = MockClients.data.FirstOrDefault<ConsultingClient>(
                        (c) => c.ClientId == 3),
                    Name = "O365 Consolidation",
                    Description = "Migrate newly acquired companies into main tenant",
                    DocumentsUrl = "https://bgtest18.sharepoint.com/sites/Bot4Test/Shared Documents/",
                    TeamUrl = "https://teams.microsoft.com/l/team/19%3ad90eb69425bc4cc7a6a43df04be83bba%40thread.skype/conversations?groupId=a54cd0e7-1d00-420a-8b51-060c288620eb&tenantId=a25d4ef1-c73a-4dc1-bdb1-9a342260f216"
                }

        };
    }
}


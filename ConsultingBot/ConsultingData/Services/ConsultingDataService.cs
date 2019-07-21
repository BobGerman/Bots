using ConsultingData.Models;
using System.Collections.Generic;
using System.Linq;

namespace ConsultingData.Services
{
    // Simple mock for now
    public class ConsultingDataService
    {
        public List<ConsultingProject> GetProjects(string query = null)
        {
            if (string.IsNullOrEmpty(query))
            {
                return MockProjects.data;
            }
            else
            {
                // Brute force string match on either client or project name
                return MockProjects.data.Where((p) =>
                    p.Client.Name.ToLower().IndexOf(query.ToLower()) >= 0 ||
                    p.Name.ToLower().IndexOf(query.ToLower()) >= 0).ToList();
            }
        }
    }
}

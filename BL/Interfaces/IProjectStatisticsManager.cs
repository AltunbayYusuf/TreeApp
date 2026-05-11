using IntegratieProject.BL.Domain.Statistics;

namespace IntegratieProject.BL.Interfaces;

public interface IProjectStatisticsManager
{
    ProjectStatistics GetProjectStatistics(int projectId);
}
using MyPortal.Core.Entities;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Interfaces;

public interface IDiaryEventRepository : IBaseEntityRepository<DiaryEvent, Guid>
{
    
}
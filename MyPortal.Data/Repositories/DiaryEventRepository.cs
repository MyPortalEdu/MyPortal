using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class DiaryEventRepository : BaseEntityRepository<DiaryEvent, Guid>, IDiaryEventRepository
{
    protected DiaryEventRepository(IConnectionFactory factory) : base(factory)
    {
    }
}
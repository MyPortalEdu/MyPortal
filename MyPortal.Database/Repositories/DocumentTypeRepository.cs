using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPortal.Database.Exceptions;
using MyPortal.Database.Helpers;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models.Connection;
using MyPortal.Database.Models.Entity;
using MyPortal.Database.Models.Filters;
using MyPortal.Database.Repositories.Base;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Database.Repositories
{
    public class DocumentTypeRepository : BaseReadWriteRepository<DocumentType>, IDocumentTypeRepository
    {
        public DocumentTypeRepository(DbUserWithContext dbUser) : base(dbUser)
        {
        }
        
        protected override string TableName => "DocumentTypes";

        public async Task<IEnumerable<DocumentType>> Get(DocumentTypeFilter filter)
        {
            var query = GetDefaultQuery();

            if (filter.Active)
            {
                query.Where($"{TableAlias}.Active", true);
            }

            if (filter.Staff)
            {
                query.Where($"{TableAlias}.Staff", true);
            }

            if (filter.Student)
            {
                query.Where($"{TableAlias}.Student", true);
            }

            if (filter.Contact)
            {
                query.Where($"{TableAlias}.Contact", true);
            }

            if (filter.General)
            {
                query.Where($"{TableAlias}.General", true);
            }

            if (filter.Sen)
            {
                query.Where($"{TableAlias}.Sen", true);
            }

            return await ExecuteQuery(query);
        }

        public async Task Update(DocumentType entity)
        {
            var documentType = await DbUser.Context.DocumentTypes.FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (documentType == null)
            {
                throw new EntityNotFoundException("Document type not found.");
            }

            if (documentType.System)
            {
                throw ExceptionHelper.UpdateSystemEntityException;
            }

            documentType.Description = entity.Description;
            documentType.Staff = entity.Staff;
            documentType.Student = entity.Student;
            documentType.Contact = entity.Contact;
            documentType.General = entity.General;
            documentType.Sen = entity.Sen;
        }
    }
}
using Neptune.NsPay.EntityFrameworkCore;

namespace Neptune.NsPay.Migrations.Seed.Host
{
    public class InitialHostDbBuilder
    {
        private readonly NsPayDbContext _context;

        public InitialHostDbBuilder(NsPayDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            new DefaultEditionCreator(_context).Create();
            new DefaultLanguagesCreator(_context).Create();
            new HostRoleAndUserCreator(_context).Create();
            new DefaultSettingsCreator(_context).Create();

            _context.SaveChanges();
        }
    }
}

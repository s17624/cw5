using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Cw5.Services
{
    public interface IDbService
    {
        public List<object[]> executeSelect(SqlCommand command);
        public void ExecuteInsert(SqlCommand command);
        public SqlConnection GetConnection();
    }
}

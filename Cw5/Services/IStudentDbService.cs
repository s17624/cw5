using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public interface IStudentDbService
    {
        DTOs.Responses.ResponseState EnrollStudent(DTOs.Requests.Student studentRequest);
        DTOs.Responses.ResponseState PromoteStudents(DTOs.Requests.Promotion promotionRequest);
        Model.Student getStudent(string index);
    }
}

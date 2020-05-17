using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Cw5.Model;
using Cw5.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cw5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {

        SqlSerrverDbService dbService;
        public EnrollmentsController(SqlSerrverDbService dbService)
        {
            this.dbService = dbService;
        }


        [Route("api/enrollments")]
        [HttpPost]
        public IActionResult EnrollStudent(DTOs.Requests.Student studentRequest)
        {
            int idStudy;

            if (studentRequest.indexNumber == null)
            {
                return NotFound("Brak indeksu");
            }
            if (studentRequest.firstName == null)
            {
                return NotFound("Brak imienia");
            }
            if (studentRequest.lastName == null)
            {
                return NotFound("Brak nazwiska");
            }
            if (studentRequest.birthDate == null)
            {
                return NotFound("Brak daty urodzenia");
            }
            if (studentRequest.studies == null)
            {
                return NotFound("Brak kierunku");
            }

            var com = new SqlCommand()
            {
                CommandText = "select IDSTUDY from STUDIES where NAME = @studies"
            };
            com.Parameters.AddWithValue("studies", studentRequest.studies);

            if (dbService.executeSelect(com).Count == 0)
            {
                return BadRequest("Brak kierunku");
            }

            idStudy = (int)dbService.executeSelect(com)[0][0];

            com = new SqlCommand()
            {
                CommandText = "select * from ENROLLMENT e inner join STUDENT S on e.IDENROLLMENT = s.IDENROLLMENT where e.SEMESTER = 1 and e.IDSTUDY = @idStudy and s.INDEXNUMBER = @indexNumber"
            };
            com.Parameters.AddWithValue("idStudy", idStudy);
            com.Parameters.AddWithValue("indexNumber", studentRequest.indexNumber);

            if(dbService.executeSelect(com).Count != 0)
            {
                return BadRequest("Rekord istnieje w BD");
            }

            com = new SqlCommand
            {
                CommandText = "select * from STUDENT where INDEXNUMBER = @indexNumber"
            };
            com.Parameters.AddWithValue("indexNumber", studentRequest.indexNumber);

            if(dbService.executeSelect(com).Count != 0)
            {
                return BadRequest("Index posiada wpis");
            }

            com = new SqlCommand()
            {
                CommandText = "select max(IDENROLLMENT) from ENROLLMENT"
            };
            int idEnrollment = (int)dbService.executeSelect(com)[0][0] + 1;

            var transaction = dbService.GetConnection().BeginTransaction();
            com = new SqlCommand()
            {
                CommandText = "insert into ENROLLMENT(IDENROLLMENT, STARTDATE, IDSTUDY, SEMESTER) values (@idEnrollment, @startDate, @idStudy, @semester"
            };
            DateTime dateTime = DateTime.Now;

            com.Parameters.AddWithValue("idEnrollment", idEnrollment);
            com.Parameters.AddWithValue("startDate", dateTime);
            com.Parameters.AddWithValue("idStudy", idStudy);
            com.Parameters.AddWithValue("semester", 1);
            dbService.ExecuteInsert(com);

            com = new SqlCommand()
            {
                CommandText = "insert into STUDENT(INDEXNUMBER, FIRSTNAME, LASTNAME, BIRTHDATE, IDENROLLMENT) VALUES (@indexNumber, @firstName, @lastName, @birthDate, @idEnrollment)"
            };
            com.Parameters.AddWithValue("indexNumber", studentRequest.indexNumber);
            com.Parameters.AddWithValue("firstName", studentRequest.firstName);
            com.Parameters.AddWithValue("lastName", studentRequest.lastName);
            com.Parameters.AddWithValue("birthdate", studentRequest.birthDate);
            com.Parameters.AddWithValue("idEnrollment", idEnrollment);
            dbService.ExecuteInsert(com);

            Enrollment enrollment = new Enrollment();
            enrollment.idEnrollment = idEnrollment;
            enrollment.semester = 1;
            enrollment.idStudy = idStudy;
            enrollment.startDate = dateTime;

            transaction.Commit();
            return Created("", enrollment);

        }

        [Route("api/enrollments/promotions")]
        [HttpPost]
        public IActionResult StudentPromotions(DTOs.Requests.Promotion promotionRequest)
        {
            int idStudy;

            if (promotionRequest.studies == null)
            {
                return NotFound("Brak studiów");
            }
            if(promotionRequest.semester < 1)
            {
                return NotFound("Brak semestru");
            }
 

            var com = new SqlCommand()
            {
                CommandText = "select IDSTUDY from STUDIES where NAME = @studyName"
            };
            com.Parameters.AddWithValue("studyName", promotionRequest.studies);


            if (dbService.executeSelect(com).Count == 0)
            {
                return BadRequest("Brak kierunku");
            }

            idStudy = (int)dbService.executeSelect(com)[0][0];

            com = new SqlCommand()
            {
                CommandText = "select * from ENROLLMENT WHERE SEMESTER = @semester and IDSTUDY = @idStudy"
            };

            com.Parameters.AddWithValue("semester", promotionRequest.semester);
            com.Parameters.AddWithValue("idStudy", idStudy);


            if (dbService.executeSelect(com).Count == 0)
            {
                return NotFound("Brak rekordów w BD");

            }

            com = new SqlCommand()
            {
                CommandText = "procedurePromoteStudents",
                CommandType = System.Data.CommandType.StoredProcedure,
            };

            com.Parameters.AddWithValue("semester", promotionRequest.semester);
            com.Parameters.AddWithValue("idStudy", idStudy);

            dbService.ExecuteInsert(com);

            return Ok();
        }
 

    }


}
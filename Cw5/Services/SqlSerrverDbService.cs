using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using Cw5.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public class SqlSerrverDbService : IDbService, IStudentDbService
    {
        private const String connectString = "Data Source=db-mssql;Initial Catalog=s17624;Integrated Security=True";
        private SqlConnection con;

        public void ExecuteInsert(SqlCommand command)
        {
            using (SqlConnection con = new SqlConnection(connectString))
            using (SqlCommand com = command)
            {
                con.Open();
                var transaction = con.BeginTransaction();

                try
                {
                    com.Connection = con;
                    com.Transaction = transaction;
                    com.ExecuteScalar();
                }
                catch (SqlException exp)
                {
                    transaction.Rollback();
                    Console.WriteLine("{0} Exception caught.", exp);
                }

            }
        }

        public List<object[]> executeSelect(SqlCommand command)
        {
            List<object[]> queryResults = new List<object[]>();

            using (SqlConnection con = new SqlConnection(connectString))
            using (SqlCommand com = command)
            {
                con.Open();
                var transaction = con.BeginTransaction();

                try
                {
                    com.Connection = con;
                    com.Transaction = transaction;

                    var queryReader = command.ExecuteReader();
                    while (queryReader.Read())
                    {
                        var temp = new Object[queryReader.FieldCount];
                        for (int i = 0; i < queryReader.FieldCount; i++)
                        {
                            temp[i] = queryReader[i];
                        }
                    }
                }
                catch (SqlException exp)
                {
                    Console.WriteLine("{0} Exception caught.", exp);
                }

            };

            return queryResults;
        }

        public SqlConnection GetConnection()
        {
            return con;
        }


        public ResponseState EnrollStudent(DTOs.Requests.Student studentRequest)
        {
            int idStudy;

            if (studentRequest.indexNumber == null)
            {
                return ResponseState.Fail;
            }
            if (studentRequest.firstName == null)
            {
                return ResponseState.Fail;
            }
            if (studentRequest.lastName == null)
            {
                return ResponseState.Fail;
            }
            if (studentRequest.birthDate == null)
            {
                return ResponseState.Fail;
            }
            if (studentRequest.studies == null)
            {
                return ResponseState.Fail;
            }

            var com = new SqlCommand()
            {
                CommandText = "select IDSTUDY from STUDIES where NAME = @studies"
            };
            com.Parameters.AddWithValue("studies", studentRequest.studies);

            if (executeSelect(com).Count == 0)
            {
                return ResponseState.Fail;
            }

            idStudy = (int)executeSelect(com)[0][0];

            com = new SqlCommand()
            {
                CommandText = "select * from ENROLLMENT e inner join STUDENT S on e.IDENROLLMENT = s.IDENROLLMENT where e.SEMESTER = 1 and e.IDSTUDY = @idStudy and s.INDEXNUMBER = @indexNumber"
            };
            com.Parameters.AddWithValue("idStudy", idStudy);
            com.Parameters.AddWithValue("indexNumber", studentRequest.indexNumber);

            if (executeSelect(com).Count != 0)
            {
                return ResponseState.NoDataChanged;
            }

            com = new SqlCommand
            {
                CommandText = "select * from STUDENT where INDEXNUMBER = @indexNumber"
            };
            com.Parameters.AddWithValue("indexNumber", studentRequest.indexNumber);

            if (executeSelect(com).Count != 0)
            {
                return ResponseState.NoDataChanged;
            }

            com = new SqlCommand()
            {
                CommandText = "select max(IDENROLLMENT) from ENROLLMENT"
            };
            int idEnrollment = (int)executeSelect(com)[0][0] + 1;

            var transaction = GetConnection().BeginTransaction();
            com = new SqlCommand()
            {
                CommandText = "insert into ENROLLMENT(IDENROLLMENT, STARTDATE, IDSTUDY, SEMESTER) values (@idEnrollment, @startDate, @idStudy, @semester"
            };
            DateTime dateTime = DateTime.Now;

            com.Parameters.AddWithValue("idEnrollment", idEnrollment);
            com.Parameters.AddWithValue("startDate", dateTime);
            com.Parameters.AddWithValue("idStudy", idStudy);
            com.Parameters.AddWithValue("semester", 1);
            ExecuteInsert(com);

            com = new SqlCommand()
            {
                CommandText = "insert into STUDENT(INDEXNUMBER, FIRSTNAME, LASTNAME, BIRTHDATE, IDENROLLMENT) VALUES (@indexNumber, @firstName, @lastName, @birthDate, @idEnrollment)"
            };
            com.Parameters.AddWithValue("indexNumber", studentRequest.indexNumber);
            com.Parameters.AddWithValue("firstName", studentRequest.firstName);
            com.Parameters.AddWithValue("lastName", studentRequest.lastName);
            com.Parameters.AddWithValue("birthdate", studentRequest.birthDate);
            com.Parameters.AddWithValue("idEnrollment", idEnrollment);
            ExecuteInsert(com);

            Model.Enrollment enrollment = new Enrollment();
            enrollment.idEnrollment = idEnrollment;
            enrollment.semester = 1;
            enrollment.idStudy = idStudy;
            enrollment.startDate = dateTime;

            transaction.Commit();
            return ResponseState.Success;
        }

        public ResponseState PromoteStudents(DTOs.Requests.Promotion promotionRequest)
        {
            int idStudy;

            if (promotionRequest.studies == null)
            {
                return ResponseState.Fail;
            }
            if (promotionRequest.semester < 1)
            {
                return ResponseState.Fail;
            }


            var com = new SqlCommand()
            {
                CommandText = "select IDSTUDY from STUDIES where NAME = @studyName"
            };
            com.Parameters.AddWithValue("studyName", promotionRequest.studies);


            if (executeSelect(com).Count == 0)
            {
                return ResponseState.Fail;
            }

            idStudy = (int)executeSelect(com)[0][0];

            com = new SqlCommand()
            {
                CommandText = "select * from ENROLLMENT WHERE SEMESTER = @semester and IDSTUDY = @idStudy"
            };

            com.Parameters.AddWithValue("semester", promotionRequest.semester);
            com.Parameters.AddWithValue("idStudy", idStudy);


            if (executeSelect(com).Count == 0)
            {
                return ResponseState.Fail;

            }

            com = new SqlCommand()
            {
                CommandText = "procedurePromoteStudents",
                CommandType = System.Data.CommandType.StoredProcedure,
            };

            com.Parameters.AddWithValue("semester", promotionRequest.semester);
            com.Parameters.AddWithValue("idStudy", idStudy);

            ExecuteInsert(com);

            return ResponseState.Success;
        }

        Model.Student IStudentDbService.getStudent(string index)
        {
            var student = new Model.Student();
            var com = new SqlCommand()
            {
                CommandText = "select * from STUDENT where INDEXNUMBER = @Index"
            };
            com.Parameters.AddWithValue("Index", index);

            var resultStudent = executeSelect(com);

            if (resultStudent.Count == 0)
            {
                return null;
            }

            student.indexNumber = (string)executeSelect(com)[0][0];
            student.firstName = (string)executeSelect(com)[0][1];
            student.lastName = (string)executeSelect(com)[0][2];
            student.birthDate = (DateTime)executeSelect(com)[0][3];
            student.idEnrollment = (int)executeSelect(com)[0][4];


            return student;

        }

    }
}

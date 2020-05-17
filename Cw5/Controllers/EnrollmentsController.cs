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
using Cw5.DTOs.Responses;
using System.Diagnostics;

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

        private IActionResult ProcessResponseState(DTOs.Responses.ResponseState response)
        {
            if(response == ResponseState.Fail)
            {
                return BadRequest();
            }else if(response == ResponseState.NoDataChanged)
            {
                return NoContent();
            }
            return Ok();
        }


        [Route("api/enrollments")]
        [HttpPost]
        public IActionResult EnrollStudent(DTOs.Requests.Student studentRequest)
        {

            return ProcessResponseState(dbService.EnrollStudent(studentRequest));
        }

        [Route("api/enrollments/promotions")]
        [HttpPost]
        public IActionResult StudentPromotions(DTOs.Requests.Promotion promotionRequest)
        {
            return ProcessResponseState(dbService.PromoteStudents(promotionRequest));
        }

 

    }


}